using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Prioritization;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Farol.Application.Services;

/// <summary>Alterações sobre a OS: status, revisão de prioridade, fatos, município, unificação e comentários.</summary>
public class OrdemServicoComandoService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    INotificadorFila notificador,
    EscopoMunicipio escopo,
    Auditor auditor,
    ServicoClassificacao classificacao)
{
    public async Task AlterarStatusAsync(Guid id, AlterarStatusRequest r, CancellationToken ct = default)
    {
        var os = await CarregarAsync(id, ct);
        var de = os.Status;
        var para = r.Status;

        if (!MaquinaEstadosOrdemServico.PodeTransitarManualmente(de, para))
            throw new ConflitoException($"Transição de {de} para {para} não é permitida.");
        if (MaquinaEstadosOrdemServico.EhReabertura(de, para) && !usuario.Tem(Permissoes.OsReabrir))
            throw new AcessoNegadoException("Reabrir uma OS concluída exige autorização de supervisor.");
        if (para == StatusOrdemServico.Cancelada && !usuario.Tem(Permissoes.OsEncerrar))
            throw new AcessoNegadoException("Você não pode cancelar OS.");
        if (MaquinaEstadosOrdemServico.ExigeJustificativa(de, para) && string.IsNullOrWhiteSpace(r.Justificativa))
            throw new RegraNegocioException(para == StatusOrdemServico.Cancelada ? "Informe o motivo do cancelamento." : "Informe a justificativa.");

        var agora = relogio.Agora;
        os.Status = para;
        os.AtualizadaEm = agora;
        if (para == StatusOrdemServico.Cancelada)
        {
            os.MotivoCancelamento = r.Justificativa!.Trim();
            os.EncerradaEm = agora;
        }
        if (MaquinaEstadosOrdemServico.EhReabertura(de, para)) os.EncerradaEm = null;

        Historico(os, "STATUS", $"Status alterado de {de} para {para}.", de, para, r.Justificativa);
        auditor.Registrar(para == StatusOrdemServico.Cancelada ? AcoesAuditoria.Cancelar : AcoesAuditoria.AlterarStatus,
            "OrdemServico", os.Id, os.MunicipioId, new { status = de }, new { status = para }, r.Justificativa);
        await db.SaveChangesAsync(ct);
        await notificador.FilaAlteradaAsync(os.MunicipioId, "status", ct);
    }

    /// <summary>Revisão manual da prioridade (seção 4.7): justificativa obrigatória; o cálculo fica registrado ao lado.</summary>
    public async Task ReclassificarAsync(Guid id, ReclassificarRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Justificativa))
            throw new RegraNegocioException("A revisão de prioridade exige justificativa.");
        var os = await CarregarAsync(id, ct, completo: true);
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status))
            throw new ConflitoException("OS encerrada não pode ser reclassificada.");
        if (r.PrioridadeManualId is { } pid && !await db.Prioridades.AnyAsync(p => p.Id == pid && p.Ativo, ct))
            throw new RegraNegocioException("Prioridade inexistente ou inativa.");

        var anterior = new { os.PrioridadeId, os.PrioridadeManualId, os.Pontuacao };
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        os.PrioridadeManualId = r.PrioridadeManualId;
        os.JustificativaManual = r.PrioridadeManualId is null ? null : r.Justificativa.Trim();
        await classificacao.ReclassificarAsync(os, MotivoClassificacao.Manual, usuario.Id, r.Justificativa.Trim(), ct: ct);
        Historico(os, "REVISAO_PRIORIDADE", r.PrioridadeManualId is null
            ? "Revisão manual removida; vale a prioridade calculada."
            : "Prioridade revista manualmente.", justificativa: r.Justificativa);
        auditor.Registrar(AcoesAuditoria.RevisaoManual, "OrdemServico", os.Id, os.MunicipioId, anterior,
            new { os.PrioridadeId, os.PrioridadeManualId, os.Pontuacao }, r.Justificativa);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(os.MunicipioId, "reclassificacao", ct);
    }

    /// <summary>Atualização de fatos confirmados (pessoas afetadas, risco, equipamento...) com reclassificação.</summary>
    public async Task AtualizarFatosAsync(Guid id, AtualizarFatosRequest r, MontadorOrdemServico montador, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Justificativa))
            throw new RegraNegocioException("Informe a justificativa da atualização.");
        var os = await CarregarAsync(id, ct, completo: true);
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status))
            throw new ConflitoException("OS encerrada não pode ser alterada.");

        // Reaproveita a validação do formulário para os códigos de opção.
        var validacao = new NovaSolicitacaoRequest
        {
            ChaveIdempotencia = Guid.NewGuid(), MunicipioId = os.MunicipioId, TipoOcorrenciaId = os.TipoOcorrenciaId,
            TipoManutencao = os.TipoManutencao, UcNaoInformada = true, MotivoUcNaoInformada = "-", Descricao = "-",
            Impacto = r.Impacto, ClassesIds = r.ClassesIds ?? os.Classes.Select(c => c.ClasseClienteId).ToList(),
        };
        var novo = (await montador.MontarAsync(validacao, relogio.Agora, ct)).Os;

        var anterior = Fatos(os);
        var i = r.Impacto;
        os.PessoasAfetadas = i.PessoasAfetadas; os.QuantidadePessoas = i.QuantidadePessoas;
        os.UcsAfetadas = i.UcsAfetadas; os.QuantidadeUcs = i.QuantidadeUcs;
        os.ServicoEssencial = i.ServicoEssencial; os.SituacaoCliente = i.SituacaoCliente;
        os.CondicaoFornecimento = i.CondicaoFornecimento; os.Redundancia = i.Redundancia; os.FonteReserva = i.FonteReserva;
        os.EquipeEspecializada = i.EquipeEspecializada; os.EquipamentoAfetado = i.EquipamentoAfetado;
        os.Abrangencia = i.Abrangencia; os.NivelRede = i.NivelRede; os.QuantidadeEquipamentos = i.QuantidadeEquipamentos;
        os.DuracaoEstimadaMin = i.DuracaoEstimadaMin; os.DataLimite = i.DataLimite;
        os.CondicoesSeguranca.Clear();
        os.CondicoesSeguranca.AddRange(novo.CondicoesSeguranca.Select(c => new OsCondicaoSeguranca { OrdemServicoId = os.Id, Codigo = c.Codigo }));
        if (r.ClassesIds is not null)
        {
            os.Classes.Clear();
            os.Classes.AddRange(novo.Classes.Select(c => new OsClasseCliente { OrdemServicoId = os.Id, ClasseClienteId = c.ClasseClienteId }));
        }
        os.RecursosNecessarios.Clear();
        os.RecursosNecessarios.AddRange(novo.RecursosNecessarios.Select(x => new OsRecurso { OrdemServicoId = os.Id, RecursoId = x.RecursoId }));
        os.RecursosEspeciais = os.RecursosNecessarios.Count > 0;

        if (r.Latitude is { } lat && r.Longitude is { } lng)
        {
            if (!new Coordenadas(lat, lng).DentroDosLimites) throw new RegraNegocioException("Coordenadas fora dos intervalos válidos.");
            os.Latitude = lat; os.Longitude = lng;
            os.OrigemCoordenada = r.OrigemCoordenada ?? OrigemCoordenada.Informada;
            os.CoordenadaRegistradaEm = relogio.Agora;
        }
        if (r.TrechoConfirmado is { } tc) os.TrechoConfirmado = tc;
        if (r.RedeConfirmada == true && os.OrigemRede != OrigemRede.NaoInformado) os.OrigemRede = OrigemRede.Confirmado;
        os.LocalizacaoPendente = os.Latitude is null && os.Logradouro is null;

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        Historico(os, "DADOS", "Dados da ocorrência atualizados.", justificativa: r.Justificativa);
        auditor.Registrar(AcoesAuditoria.Alterar, "OrdemServico", os.Id, os.MunicipioId, anterior, Fatos(os), r.Justificativa);
        await db.SaveChangesAsync(ct);
        await classificacao.ReclassificarAsync(os, MotivoClassificacao.AtualizacaoDados, usuario.Id, r.Justificativa.Trim(), ct: ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(os.MunicipioId, "fatos", ct);
    }

    /// <summary>Correção de município (seção 3A.6): autorizado, justificado, auditado; fila e versão recalculadas.</summary>
    public async Task TrocarMunicipioAsync(Guid id, TrocarMunicipioRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Justificativa)) throw new RegraNegocioException("A troca de município exige justificativa.");
        var os = await CarregarAsync(id, ct, completo: true);
        await escopo.ExigirAsync(r.MunicipioId, "Municipio", r.MunicipioId, ct);
        if (os.MunicipioId == r.MunicipioId) throw new RegraNegocioException("A OS já pertence a este município.");
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status)) throw new ConflitoException("OS encerrada não pode trocar de município.");
        if (await db.Despachos.AnyAsync(d => d.OrdemServicoId == id && d.Ativo, ct))
            throw new ConflitoException("Encerre o despacho ativo antes de trocar o município.");
        var destino = await db.Municipios.FirstOrDefaultAsync(m => m.Id == r.MunicipioId && m.Ativo, ct)
                      ?? throw new RegraNegocioException("Município inexistente ou inativo.");

        var origem = os.MunicipioId;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        os.MunicipioId = destino.Id;
        os.VersaoPontuacaoId = (await classificacao.VersaoVigenteAsync(destino.Id, relogio.Agora, ct)).Id;
        Historico(os, "MUNICIPIO", $"Município alterado para {destino.Nome}.", justificativa: r.Justificativa);
        auditor.Registrar(AcoesAuditoria.TrocarMunicipio, "OrdemServico", os.Id, destino.Id, new { municipioId = origem },
            new { municipioId = destino.Id }, r.Justificativa);
        await db.SaveChangesAsync(ct);
        await classificacao.ReclassificarAsync(os, MotivoClassificacao.TrocaMunicipio, usuario.Id, r.Justificativa.Trim(), ct: ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(origem, "troca-municipio", ct);
        await notificador.FilaAlteradaAsync(destino.Id, "troca-municipio", ct);
    }

    /// <summary>Unificação confirmada (seção 13): a duplicada é cancelada, as solicitações migram, nada se perde.</summary>
    public async Task UnificarAsync(Guid principalId, UnificarRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Justificativa)) throw new RegraNegocioException("A unificação exige justificativa.");
        if (principalId == r.OsDuplicadaId) throw new RegraNegocioException("Escolha uma OS diferente da principal.");
        var principal = await CarregarAsync(principalId, ct, completo: true);
        var duplicada = await CarregarAsync(r.OsDuplicadaId, ct, completo: true);

        if (principal.MunicipioId != duplicada.MunicipioId) throw new ConflitoException("Só é possível unificar OS do mesmo município.");
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(principal.Status) || MaquinaEstadosOrdemServico.Encerrados.Contains(duplicada.Status))
            throw new ConflitoException("Só é possível unificar OS abertas.");
        if (await db.Despachos.AnyAsync(d => d.OrdemServicoId == duplicada.Id && d.Ativo, ct))
            throw new ConflitoException("A OS duplicada tem despacho ativo; encerre-o antes de unificar.");

        var agora = relogio.Agora;
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var solicitacoes = await db.Solicitacoes.Where(s => s.OrdemServicoId == duplicada.Id).ToListAsync(ct);
        foreach (var s in solicitacoes)
        {
            s.OrdemServicoId = principal.Id;
            db.VinculosSolicitacao.Add(new VinculoSolicitacao
            {
                SolicitacaoId = s.Id, OrdemServicoId = principal.Id, OrdemServicoOrigemId = duplicada.Id,
                VinculadoPorId = usuario.Id, VinculadoEm = agora, Justificativa = r.Justificativa.Trim(),
            });
        }
        SolicitacaoService.FundirFatos(principal, duplicada);

        var statusAnterior = duplicada.Status;
        duplicada.Status = StatusOrdemServico.Cancelada;
        duplicada.MotivoCancelamento = $"Unificada em {principal.Numero}";
        duplicada.EncerradaEm = agora;
        duplicada.AtualizadaEm = agora;

        Historico(duplicada, "UNIFICACAO", $"OS unificada em {principal.Numero}; solicitações transferidas.", statusAnterior,
            StatusOrdemServico.Cancelada, r.Justificativa);
        Historico(principal, "UNIFICACAO", $"OS {duplicada.Numero} unificada nesta; {solicitacoes.Count} solicitação(ões) vinculada(s).",
            justificativa: r.Justificativa);
        auditor.Registrar(AcoesAuditoria.Unificar, "OrdemServico", principal.Id, principal.MunicipioId,
            novos: new { principal = principal.Numero, duplicada = duplicada.Numero }, justificativa: r.Justificativa);
        await db.SaveChangesAsync(ct);
        await classificacao.ReclassificarAsync(principal, MotivoClassificacao.Vinculo, usuario.Id, r.Justificativa.Trim(), ct: ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(principal.MunicipioId, "unificacao", ct);
    }

    public async Task<ComentarioDto> ComentarAsync(Guid id, ComentarioRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Texto) || r.Texto.Length > 2000) throw new RegraNegocioException("Comentário vazio ou longo demais (máx. 2000).");
        var os = await CarregarAsync(id, ct);
        var c = new Comentario { OrdemServicoId = os.Id, UsuarioId = usuario.Id, UsuarioNome = usuario.Nome, Texto = r.Texto.Trim(), CriadoEm = relogio.Agora };
        db.Comentarios.Add(c);
        await db.SaveChangesAsync(ct);
        return new ComentarioDto(c.Id, c.UsuarioNome, c.Texto, c.CriadoEm);
    }

    public const long TamanhoMaximoAnexo = 5 * 1024 * 1024;
    private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp", "application/pdf", "text/plain"];

    public async Task<AnexoDto> AnexarAsync(Guid id, string nome, string tipo, byte[] conteudo, CancellationToken ct = default)
    {
        if (conteudo.Length == 0 || conteudo.Length > TamanhoMaximoAnexo) throw new RegraNegocioException("Anexo vazio ou maior que 5 MB.");
        if (!TiposPermitidos.Contains(tipo)) throw new RegraNegocioException("Tipo de arquivo não permitido (JPEG, PNG, WEBP, PDF ou TXT).");
        var os = await CarregarAsync(id, ct);
        var anexo = new Anexo
        {
            Id = Guid.NewGuid(), OrdemServicoId = os.Id, NomeArquivo = Path.GetFileName(nome), TipoConteudo = tipo,
            Tamanho = conteudo.Length, Conteudo = conteudo, EnviadoPorId = usuario.Id, EnviadoEm = relogio.Agora,
        };
        db.Anexos.Add(anexo);
        auditor.Registrar(AcoesAuditoria.Criar, "Anexo", anexo.Id, os.MunicipioId, novos: new { anexo.NomeArquivo, anexo.Tamanho });
        await db.SaveChangesAsync(ct);
        return new AnexoDto(anexo.Id, anexo.NomeArquivo, anexo.TipoConteudo, anexo.Tamanho, anexo.EnviadoEm);
    }

    public async Task<Anexo> BaixarAnexoAsync(Guid osId, Guid anexoId, CancellationToken ct = default)
    {
        await CarregarAsync(osId, ct);
        return await db.Anexos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == anexoId && a.OrdemServicoId == osId, ct)
               ?? throw new NaoEncontradoException("Anexo não encontrado.");
    }

    private async Task<OrdemServico> CarregarAsync(Guid id, CancellationToken ct, bool completo = false)
    {
        IQueryable<OrdemServico> q = db.OrdensServico;
        if (completo)
            q = q.Include(o => o.CondicoesSeguranca).Include(o => o.Classes).Include(o => o.RecursosNecessarios).Include(o => o.RespostasPersonalizadas);
        var os = await q.FirstOrDefaultAsync(o => o.Id == id, ct) ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", id, ct);
        return os;
    }

    private void Historico(OrdemServico os, string tipo, string descricao, StatusOrdemServico? de = null, StatusOrdemServico? para = null, string? justificativa = null) =>
        db.HistoricosOs.Add(new HistoricoOs
        {
            OrdemServicoId = os.Id, OcorridoEm = relogio.Agora, UsuarioId = usuario.Id, UsuarioNome = usuario.Nome,
            Tipo = tipo, Descricao = descricao, StatusAnterior = de?.ToString(), StatusNovo = para?.ToString(),
            Justificativa = justificativa?.Trim(),
        });

    private static object Fatos(OrdemServico o) => new
    {
        o.PessoasAfetadas, o.QuantidadePessoas, o.UcsAfetadas, o.QuantidadeUcs, o.ServicoEssencial, o.SituacaoCliente,
        Condicoes = o.CondicoesSeguranca.Select(c => c.Codigo).ToList(), o.CondicaoFornecimento, o.Redundancia, o.FonteReserva,
        o.EquipeEspecializada, o.EquipamentoAfetado, o.Abrangencia, o.NivelRede, o.DuracaoEstimadaMin, o.DataLimite,
        o.Latitude, o.Longitude, o.TrechoConfirmado, o.OrigemRede,
    };
}
