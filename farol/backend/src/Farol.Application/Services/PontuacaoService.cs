using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Mapping;
using Farol.Application.Prioritization;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Services;

/// <summary>
/// Tela do Usuário Chave (seção 7A): rascunho, validação, simulação, impacto, publicação e versões.
/// O mecanismo é do código; os valores são da operação.
/// </summary>
public class PontuacaoService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    INotificadorFila notificador,
    Auditor auditor,
    EscopoMunicipio escopo,
    ServicoClassificacao classificacao,
    MontadorOrdemServico montador,
    IOptions<FarolOptions> opcoes)
{
    private readonly FarolOptions.PontuacaoOptions _cfg = opcoes.Value.Pontuacao;

    public async Task<ConfiguracaoPontuacaoDto> ObterAsync(int? municipioId, CancellationToken ct = default)
    {
        await ExigirEscopoAsync(municipioId, municipioId is { } id ? id : "global", ct);
        var agora = relogio.Agora;
        string escopo = "Global";
        VersaoPontuacao vigente;
        var herdada = false;
        if (municipioId is { } m)
        {
            var municipio = await db.Municipios.FirstOrDefaultAsync(x => x.Id == m, ct) ?? throw new NaoEncontradoException("Município não encontrado.");
            escopo = municipio.Nome;
            vigente = await classificacao.VersaoVigenteAsync(m, agora, ct);
            herdada = vigente.MunicipioId is null;
        }
        else vigente = await VigenteGlobalAsync(agora, ct);

        var rascunho = await db.VersoesPontuacao.FirstOrDefaultAsync(v => v.MunicipioId == municipioId && v.Status == StatusVersaoPontuacao.Rascunho, ct);
        var aguardando = await db.VersoesPontuacao.FirstOrDefaultAsync(v => v.MunicipioId == municipioId && v.Status == StatusVersaoPontuacao.AguardandoAprovacao, ct);

        return new ConfiguracaoPontuacaoDto(
            municipioId, escopo,
            await DetalheAsync(vigente.Id, ct),
            herdada,
            rascunho is null ? null : await DetalheAsync(rascunho.Id, ct),
            aguardando is null ? null : await DetalheAsync(aguardando.Id, ct),
            await RegrasAsync(ct),
            _cfg.ExigirAprovacao,
            _cfg.PontosMaximosPorOpcao,
            usuario.Tem(Permissoes.PontuacaoEditar),
            usuario.Tem(Permissoes.PontuacaoPublicar),
            usuario.Tem(Permissoes.PontuacaoAprovar));
    }

    /// <summary>Cria o rascunho do escopo copiando a versão vigente (ou a indicada, ao restaurar).</summary>
    public async Task<VersaoDetalheDto> CriarRascunhoAsync(int? municipioId, int? copiarDeId = null, CancellationToken ct = default)
    {
        await ExigirEscopoAsync(municipioId, municipioId is { } id ? id : "global", ct);
        if (await db.VersoesPontuacao.AnyAsync(v => v.MunicipioId == municipioId &&
                (v.Status == StatusVersaoPontuacao.Rascunho || v.Status == StatusVersaoPontuacao.AguardandoAprovacao), ct))
            throw new ConflitoException("Já existe um rascunho ou uma versão aguardando aprovação neste escopo.");

        var agora = relogio.Agora;
        var origemId = copiarDeId ?? (municipioId is { } m
            ? (await classificacao.VersaoVigenteAsync(m, agora, ct)).Id
            : (await VigenteGlobalAsync(agora, ct)).Id);
        var origem = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Pontos).Include(v => v.Faixas).Include(v => v.Criterios)
            .FirstOrDefaultAsync(v => v.Id == origemId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        if (copiarDeId is not null && origem.MunicipioId != municipioId)
            throw new RegraNegocioException("Só é possível restaurar versões do mesmo escopo.");

        var numero = (await db.VersoesPontuacao.Where(v => v.MunicipioId == municipioId).MaxAsync(v => (int?)v.Numero, ct) ?? 0) + 1;
        var rascunho = new VersaoPontuacao
        {
            MunicipioId = municipioId,
            Numero = numero,
            Status = StatusVersaoPontuacao.Rascunho,
            Demonstrativa = false,
            AutorId = usuario.Id,
            AutorNome = usuario.Nome,
            CriadaEm = agora,
            AlteradaEm = agora,
            RestauradaDeId = copiarDeId,
            Criterios = origem.Criterios.Select(c => new VersaoCriterio { CriterioId = c.CriterioId, Habilitado = c.Habilitado }).ToList(),
            Pontos = origem.Pontos.Select(p => new PontoOpcao
            {
                OpcaoId = p.OpcaoId, Pontos = p.Pontos, RequerConfirmacao = p.RequerConfirmacao, Origem = p.Origem,
                AlteradoPorId = p.AlteradoPorId, AlteradoPorNome = p.AlteradoPorNome, AlteradoEm = p.AlteradoEm,
            }).ToList(),
            Faixas = origem.Faixas.Select(f => new FaixaPrioridade { PrioridadeId = f.PrioridadeId, Minimo = f.Minimo, Maximo = f.Maximo }).ToList(),
        };
        db.VersoesPontuacao.Add(rascunho);
        auditor.Registrar(AcoesAuditoria.EditarPontuacao, "VersaoPontuacao", $"{municipioId?.ToString() ?? "global"}#{numero}", municipioId,
            novos: new { acao = copiarDeId is null ? "rascunho criado" : $"restauração da versão {origem.Numero}", origem = origem.Numero });
        await db.SaveChangesAsync(ct);
        return await DetalheAsync(rascunho.Id, ct);
    }

    public async Task DescartarRascunhoAsync(int versaoId, CancellationToken ct = default)
    {
        var v = await db.VersoesPontuacao.FirstOrDefaultAsync(x => x.Id == versaoId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(v.MunicipioId, versaoId, ct);
        if (v.Status != StatusVersaoPontuacao.Rascunho) throw new ConflitoException("Só rascunhos podem ser descartados.");
        db.VersoesPontuacao.Remove(v);
        auditor.Registrar(AcoesAuditoria.EditarPontuacao, "VersaoPontuacao", v.Id, v.MunicipioId, novos: new { acao = "rascunho descartado", v.Numero });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Grava o rascunho. Se outra pessoa gravou antes, a segunda gravação é recusada (409).</summary>
    public async Task<VersaoDetalheDto> SalvarRascunhoAsync(SalvarRascunhoRequest r, CancellationToken ct = default)
    {
        var v = await db.VersoesPontuacao.Include(x => x.Pontos).Include(x => x.Faixas).Include(x => x.Criterios)
            .FirstOrDefaultAsync(x => x.Id == r.VersaoId, ct) ?? throw new NaoEncontradoException("Rascunho não encontrado.");
        await ExigirEscopoAsync(v.MunicipioId, r.VersaoId, ct);
        if (v.Status != StatusVersaoPontuacao.Rascunho) throw new ConflitoException("Esta versão não é um rascunho editável.");
        if (!uint.TryParse(r.Token, out var token)) throw new RegraNegocioException("Token de versão inválido.");
        db.Entry(v).Property(x => x.Versao).OriginalValue = token;

        var erros = new List<string>();
        foreach (var p in r.Pontos)
        {
            if (p.Pontos is < 0) erros.Add($"Opção {p.OpcaoId}: pontos não podem ser negativos.");
            if (p.Pontos > _cfg.PontosMaximosPorOpcao) erros.Add($"Opção {p.OpcaoId}: máximo de {_cfg.PontosMaximosPorOpcao} pontos.");
        }
        foreach (var f in r.Faixas)
        {
            if (f.Minimo < 0 || f.Maximo < 0) erros.Add("Faixas não aceitam valores negativos.");
            if (f.Maximo is { } max && max < f.Minimo) erros.Add("Em cada faixa, o máximo deve ser maior ou igual ao mínimo.");
        }
        if (erros.Count > 0) throw new RegraNegocioException("Valores inválidos no rascunho.", new Dictionary<string, string[]> { ["pontos"] = erros.ToArray() });

        var agora = relogio.Agora;
        var anteriores = v.Pontos.ToDictionary(p => p.OpcaoId, p => p.Pontos);
        var opcoesValidas = await db.OpcoesCriterio.Select(o => o.Id).ToListAsync(ct);
        foreach (var p in r.Pontos.Where(p => opcoesValidas.Contains(p.OpcaoId)))
        {
            var atual = v.Pontos.FirstOrDefault(x => x.OpcaoId == p.OpcaoId);
            if (atual is null)
            {
                atual = new PontoOpcao { OpcaoId = p.OpcaoId };
                v.Pontos.Add(atual);
            }
            if (atual.Pontos != p.Pontos || atual.RequerConfirmacao != p.RequerConfirmacao || atual.AlteradoEm == default)
            {
                atual.Pontos = p.Pontos;
                atual.RequerConfirmacao = p.RequerConfirmacao;
                atual.Origem = OrigemPontos.UsuarioChave;
                atual.AlteradoPorId = usuario.Id;
                atual.AlteradoPorNome = usuario.Nome;
                atual.AlteradoEm = agora;
            }
        }
        foreach (var c in r.Criterios)
        {
            var atual = v.Criterios.FirstOrDefault(x => x.CriterioId == c.CriterioId);
            if (atual is null) v.Criterios.Add(new VersaoCriterio { CriterioId = c.CriterioId, Habilitado = c.Habilitado });
            else atual.Habilitado = c.Habilitado;
        }
        v.Faixas.Clear();
        v.Faixas.AddRange(r.Faixas.Select(f => new FaixaPrioridade { PrioridadeId = f.PrioridadeId, Minimo = f.Minimo, Maximo = f.Maximo }));
        v.AlteradaEm = agora;

        var alteracoes = r.Pontos.Where(p => anteriores.GetValueOrDefault(p.OpcaoId) != p.Pontos)
            .Select(p => new { p.OpcaoId, de = anteriores.GetValueOrDefault(p.OpcaoId), para = p.Pontos }).ToList();
        auditor.Registrar(AcoesAuditoria.EditarPontuacao, "VersaoPontuacao", v.Id, v.MunicipioId,
            novos: new { pontosAlterados = alteracoes, faixas = r.Faixas, criterios = r.Criterios });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflitoException("O rascunho foi alterado por outra pessoa. Recarregue para ver a versão atual antes de gravar.");
        }
        return await DetalheAsync(v.Id, ct);
    }

    /// <summary>Simulador: mesmo montador e mesmo motor da produção, com o rascunho ou a versão escolhida.</summary>
    public async Task<ClassificacaoDto> SimularAsync(SimularRequest r, CancellationToken ct = default)
    {
        await ExigirEscopoAsync(r.MunicipioId, r.MunicipioId, ct);
        var agora = relogio.Agora;
        var solicitacao = r.Solicitacao with { MunicipioId = r.MunicipioId, ChaveIdempotencia = Guid.NewGuid() };
        var montada = await montador.MontarAsync(solicitacao, agora, ct);

        VersaoPontuacao versao;
        if (r.VersaoId is { } vid) versao = await db.VersoesPontuacao.AsNoTracking().FirstAsync(v => v.Id == vid, ct);
        else if (r.UsarRascunho)
        {
            versao = await db.VersoesPontuacao.AsNoTracking()
                         .Where(v => v.Status == StatusVersaoPontuacao.Rascunho && (v.MunicipioId == r.MunicipioId || v.MunicipioId == null))
                         .OrderByDescending(v => v.MunicipioId).FirstOrDefaultAsync(ct)
                     ?? throw new ConflitoException("Não há rascunho para simular neste escopo.");
        }
        else versao = await classificacao.VersaoVigenteAsync(r.MunicipioId, agora, ct);

        var detalhe = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Municipio).FirstAsync(v => v.Id == versao.Id, ct);
        var config = await classificacao.CarregarAsync(versao.Id, r.MunicipioId, ct);
        var pendencias = Pendencias(config);
        var calculo = await classificacao.CalcularAsync(montada.Os, config, agora, ct);
        var prioridades = await db.Prioridades.AsNoTracking().ToDictionaryAsync(p => p.Id, ct);
        var alertas = montada.Alertas.Concat(pendencias.Select(p => "Rascunho incompleto: " + p)).ToList();
        return MapeamentoClassificacao.DeCalculo(calculo, prioridades, MapeamentoClassificacao.Versao(detalhe), agora, alertas);
    }

    /// <summary>Quais OS abertas mudariam de prioridade se a versão fosse aplicada a elas.</summary>
    public async Task<ImpactoVersaoDto> ImpactoAsync(int versaoId, CancellationToken ct = default)
    {
        var versao = await db.VersoesPontuacao.AsNoTracking().FirstOrDefaultAsync(v => v.Id == versaoId, ct)
                     ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(versao.MunicipioId, versaoId, ct);
        var afetadas = await OsAfetadasAsync(versao, ct);
        var prioridades = await db.Prioridades.AsNoTracking().ToDictionaryAsync(p => p.Id, ct);
        var nomes = await db.Municipios.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Nome, ct);
        var agora = relogio.Agora;

        var configs = new Dictionary<int, ConfiguracaoClassificacao>();
        var ordens = new List<ImpactoOsDto>();
        var porCidade = new Dictionary<int, (int Avaliadas, int Mudariam)>();
        foreach (var os in afetadas)
        {
            if (!configs.TryGetValue(os.MunicipioId, out var config))
                configs[os.MunicipioId] = config = await classificacao.CarregarAsync(versao.Id, os.MunicipioId, ct);
            var calculo = await classificacao.CalcularAsync(os, config, agora, ct);
            var novaId = os.PrioridadeManualId ?? calculo.Prioridade.Id;
            var (av, mu) = porCidade.GetValueOrDefault(os.MunicipioId);
            var mudou = novaId != os.PrioridadeId;
            porCidade[os.MunicipioId] = (av + 1, mu + (mudou ? 1 : 0));
            if (mudou)
                ordens.Add(new ImpactoOsDto(os.Id, os.Numero, nomes[os.MunicipioId],
                    os.PrioridadeId is { } pid ? prioridades[pid].Nome : "—", prioridades[novaId].Nome, os.Pontuacao, calculo.Pontuacao));
        }

        return new ImpactoVersaoDto(versao.Id, afetadas.Count, ordens.Count,
            porCidade.Select(x => new ImpactoCidadeDto(x.Key, nomes[x.Key], x.Value.Avaliadas, x.Value.Mudariam)).OrderBy(x => x.Municipio).ToList(),
            ordens.OrderBy(o => o.Municipio).ThenBy(o => o.Numero).ToList());
    }

    public async Task<VersaoDetalheDto> PublicarAsync(PublicarRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Justificativa)) throw new RegraNegocioException("A publicação exige justificativa.");
        var agora = relogio.Agora;
        var vigencia = r.VigenciaInicio ?? agora;
        if (vigencia < agora.AddMinutes(-5)) throw new RegraNegocioException("A vigência não pode começar no passado.");

        var v = await db.VersoesPontuacao.FirstOrDefaultAsync(x => x.Id == r.VersaoId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(v.MunicipioId, r.VersaoId, ct);
        if (v.Status != StatusVersaoPontuacao.Rascunho) throw new ConflitoException("Só um rascunho pode ser publicado.");

        var config = await classificacao.CarregarAsync(v.Id, v.MunicipioId, ct);
        var pendencias = Pendencias(config);
        if (pendencias.Count > 0) throw new NaoProcessavelException("A versão tem pendências que impedem a publicação.", pendencias);

        v.Justificativa = r.Justificativa.Trim();
        v.VigenciaInicio = vigencia;
        v.Aplicacao = r.Aplicacao;
        v.PublicadaPorId = usuario.Id;
        v.PublicadaPorNome = usuario.Nome;
        v.PublicadaEm = agora;
        v.AlteradaEm = agora;

        if (_cfg.ExigirAprovacao)
        {
            v.Status = StatusVersaoPontuacao.AguardandoAprovacao;
            auditor.Registrar(AcoesAuditoria.PublicarPontuacao, "VersaoPontuacao", v.Id, v.MunicipioId,
                novos: new { v.Numero, status = v.Status, vigencia, r.Aplicacao }, justificativa: v.Justificativa);
            await db.SaveChangesAsync(ct);
            return await DetalheAsync(v.Id, ct);
        }

        await EfetivarAsync(v, ct);
        return await DetalheAsync(v.Id, ct);
    }

    /// <summary>Segundo usuário aprova a publicação (quando exigido). O autor não aprova a própria versão.</summary>
    public async Task<VersaoDetalheDto> AprovarAsync(int versaoId, CancellationToken ct = default)
    {
        var v = await db.VersoesPontuacao.FirstOrDefaultAsync(x => x.Id == versaoId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(v.MunicipioId, versaoId, ct);
        if (v.Status != StatusVersaoPontuacao.AguardandoAprovacao) throw new ConflitoException("A versão não está aguardando aprovação.");
        if (v.PublicadaPorId == usuario.Id || v.AutorId == usuario.Id) throw new ConflitoException("A aprovação deve ser feita por outro usuário.");
        v.AprovadaPorId = usuario.Id;
        v.AprovadaPorNome = usuario.Nome;
        v.AprovadaEm = relogio.Agora;
        if (v.VigenciaInicio < relogio.Agora) v.VigenciaInicio = relogio.Agora;
        await EfetivarAsync(v, ct);
        return await DetalheAsync(v.Id, ct);
    }

    public async Task<VersaoDetalheDto> RejeitarAsync(int versaoId, string? motivo, CancellationToken ct = default)
    {
        var v = await db.VersoesPontuacao.FirstOrDefaultAsync(x => x.Id == versaoId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(v.MunicipioId, versaoId, ct);
        if (v.Status != StatusVersaoPontuacao.AguardandoAprovacao) throw new ConflitoException("A versão não está aguardando aprovação.");
        v.Status = StatusVersaoPontuacao.Rascunho;
        v.AlteradaEm = relogio.Agora;
        auditor.Registrar(AcoesAuditoria.AprovarPontuacao, "VersaoPontuacao", v.Id, v.MunicipioId, novos: new { resultado = "rejeitada" }, justificativa: motivo);
        await db.SaveChangesAsync(ct);
        return await DetalheAsync(v.Id, ct);
    }

    private async Task EfetivarAsync(VersaoPontuacao v, CancellationToken ct)
    {
        var agora = relogio.Agora;
        v.Status = StatusVersaoPontuacao.Publicada;
        var anteriores = await db.VersoesPontuacao
            .Where(x => x.MunicipioId == v.MunicipioId && x.Status == StatusVersaoPontuacao.Publicada && x.Id != v.Id)
            .ToListAsync(ct);
        foreach (var a in anteriores) a.Status = StatusVersaoPontuacao.Substituida;

        auditor.Registrar(v.AprovadaPorId is null ? AcoesAuditoria.PublicarPontuacao : AcoesAuditoria.AprovarPontuacao,
            "VersaoPontuacao", v.Id, v.MunicipioId,
            novos: new { v.Numero, v.VigenciaInicio, v.Aplicacao, substituidas = anteriores.Select(a => a.Numero) }, justificativa: v.Justificativa);
        await db.SaveChangesAsync(ct);

        if (v.Aplicacao == AplicacaoVersao.ReclassificarAbertas && v.VigenciaInicio <= agora)
            await AplicarAsync(v, ct);
    }

    /// <summary>Vincula as OS abertas do escopo à versão e as reclassifica (motivo NOVA_VERSAO). Histórico preservado.</summary>
    public async Task<int> AplicarAsync(VersaoPontuacao v, CancellationToken ct = default)
    {
        var afetadas = await OsAfetadasAsync(v, ct, rastrear: true);
        var configs = new Dictionary<int, ConfiguracaoClassificacao>();
        foreach (var os in afetadas)
        {
            if (!configs.TryGetValue(os.MunicipioId, out var config))
                configs[os.MunicipioId] = config = await classificacao.CarregarAsync(v.Id, os.MunicipioId, ct);
            os.VersaoPontuacaoId = v.Id;
            await classificacao.ReclassificarAsync(os, MotivoClassificacao.NovaVersao, v.AprovadaPorId ?? v.PublicadaPorId,
                $"Versão {v.Numero} de pontuação publicada com reclassificação das OS abertas.", config: config, ct: ct);
        }
        var versao = await db.VersoesPontuacao.FirstAsync(x => x.Id == v.Id, ct);
        versao.ReclassificacaoAplicadaEm = relogio.Agora;
        await db.SaveChangesAsync(ct);
        foreach (var m in afetadas.Select(o => o.MunicipioId).Distinct())
            await notificador.FilaAlteradaAsync(m, "nova-versao", ct);
        return afetadas.Count;
    }

    /// <summary>OS abertas que uma versão alcança: as da cidade (versão municipal) ou as de cidades sem versão própria (global).</summary>
    private async Task<List<OrdemServico>> OsAfetadasAsync(VersaoPontuacao v, CancellationToken ct, bool rastrear = false)
    {
        IQueryable<OrdemServico> q = db.OrdensServico.Include(o => o.CondicoesSeguranca).Include(o => o.Classes)
            .Include(o => o.RecursosNecessarios).Include(o => o.RespostasPersonalizadas).AsSplitQuery();
        if (!rastrear) q = q.AsNoTracking();
        q = q.Where(o => o.Status != StatusOrdemServico.Concluida && o.Status != StatusOrdemServico.Cancelada);

        if (v.MunicipioId is { } m) return await q.Where(o => o.MunicipioId == m).ToListAsync(ct);

        var agora = relogio.Agora;
        var comVersaoPropria = await db.VersoesPontuacao
            .Where(x => x.MunicipioId != null && x.Status == StatusVersaoPontuacao.Publicada && x.VigenciaInicio <= agora)
            .Select(x => x.MunicipioId!.Value).Distinct().ToListAsync(ct);
        return await q.Where(o => !comVersaoPropria.Contains(o.MunicipioId)).ToListAsync(ct);
    }

    public async Task<List<VersaoListaDto>> VersoesAsync(int? municipioId, CancellationToken ct = default)
    {
        await ExigirEscopoAsync(municipioId, municipioId is { } id ? id : "global", ct);
        var agora = relogio.Agora;
        var vigenteId = municipioId is { } m ? (await classificacao.VersaoVigenteAsync(m, agora, ct)).Id : (await VigenteGlobalAsync(agora, ct)).Id;
        var lista = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Municipio)
            .Where(v => v.MunicipioId == municipioId).OrderByDescending(v => v.Numero).ToListAsync(ct);
        return lista.Select(v => Lista(v, v.Id == vigenteId)).ToList();
    }

    public async Task<ComparacaoDto> CompararAsync(int a, int b, CancellationToken ct = default)
    {
        var va = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Municipio).Include(v => v.Pontos).Include(v => v.Faixas).Include(v => v.Criterios).FirstOrDefaultAsync(v => v.Id == a, ct)
                 ?? throw new NaoEncontradoException("Versão A não encontrada.");
        var vb = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Municipio).Include(v => v.Pontos).Include(v => v.Faixas).Include(v => v.Criterios).FirstOrDefaultAsync(v => v.Id == b, ct)
                 ?? throw new NaoEncontradoException("Versão B não encontrada.");
        var opcoes = await db.OpcoesCriterio.AsNoTracking().Include(o => o.Criterio).ToDictionaryAsync(o => o.Id, ct);
        var criterios = await db.Criterios.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Nome, ct);
        var prioridades = await db.Prioridades.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Nome, ct);

        var dif = new List<DiferencaDto>();
        var pa = va.Pontos.ToDictionary(p => p.OpcaoId);
        var pb = vb.Pontos.ToDictionary(p => p.OpcaoId);
        foreach (var id in pa.Keys.Union(pb.Keys).OrderBy(id => opcoes.TryGetValue(id, out var o) ? o.Criterio!.Ordem : 0).ThenBy(id => id))
        {
            var x = pa.GetValueOrDefault(id)?.Pontos;
            var y = pb.GetValueOrDefault(id)?.Pontos;
            if (x != y && opcoes.TryGetValue(id, out var o))
                dif.Add(new DiferencaDto("PONTOS", $"{o.Criterio!.Nome} › {o.Rotulo}", x?.ToString() ?? "sem pontos", y?.ToString() ?? "sem pontos"));
        }
        var ca = va.Criterios.ToDictionary(c => c.CriterioId, c => c.Habilitado);
        var cb = vb.Criterios.ToDictionary(c => c.CriterioId, c => c.Habilitado);
        foreach (var id in ca.Keys.Union(cb.Keys))
            if (ca.GetValueOrDefault(id) != cb.GetValueOrDefault(id))
                dif.Add(new DiferencaDto("CRITERIO", criterios.GetValueOrDefault(id, $"#{id}"),
                    ca.GetValueOrDefault(id) ? "habilitado" : "desabilitado", cb.GetValueOrDefault(id) ? "habilitado" : "desabilitado"));
        var fa = va.Faixas.ToDictionary(f => f.PrioridadeId);
        var fb = vb.Faixas.ToDictionary(f => f.PrioridadeId);
        foreach (var id in fa.Keys.Union(fb.Keys))
        {
            string? Fx(FaixaPrioridade? f) => f is null ? null : $"{f.Minimo}–{(f.Maximo?.ToString() ?? "∞")}";
            var x = Fx(fa.GetValueOrDefault(id));
            var y = Fx(fb.GetValueOrDefault(id));
            if (x != y) dif.Add(new DiferencaDto("FAIXA", prioridades.GetValueOrDefault(id, $"#{id}"), x, y));
        }
        return new ComparacaoDto(Lista(va, false), Lista(vb, false), dif);
    }

    public async Task<VersaoDetalheDto> DetalheAsync(int versaoId, CancellationToken ct = default)
    {
        var v = await db.VersoesPontuacao.AsNoTracking().Include(x => x.Municipio).Include(x => x.Pontos).Include(x => x.Faixas).Include(x => x.Criterios)
            .FirstOrDefaultAsync(x => x.Id == versaoId, ct) ?? throw new NaoEncontradoException("Versão não encontrada.");
        await ExigirEscopoAsync(v.MunicipioId, versaoId, ct);
        var criterios = await db.Criterios.AsNoTracking().Include(c => c.Opcoes).Include(c => c.Municipios).Where(c => c.Ativo)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Id).ToListAsync(ct);
        var prioridades = await db.Prioridades.AsNoTracking().Where(p => p.Ativo).OrderBy(p => p.Rank).ToListAsync(ct);
        var regras = await db.RegrasPrecedencia.AsNoTracking().Where(r => r.Ativa).ToListAsync(ct);
        var config = ServicoClassificacao.Montar(v, criterios, regras, prioridades, v.MunicipioId);

        var pontos = v.Pontos.ToDictionary(p => p.OpcaoId);
        var habilitados = v.Criterios.ToDictionary(c => c.CriterioId, c => c.Habilitado);
        var criteriosDto = criterios
            .Where(c => v.MunicipioId is null || c.Municipios.Count == 0 || c.Municipios.Any(m => m.MunicipioId == v.MunicipioId))
            .Select(c =>
            {
                var ops = c.Opcoes.Where(o => o.Ativa).OrderBy(o => o.Ordem).Select(o =>
                {
                    var p = pontos.GetValueOrDefault(o.Id);
                    return new OpcaoPontosDto(o.Id, o.Codigo, o.Rotulo, o.RepresentaDesconhecido, p?.Pontos, p?.RequerConfirmacao ?? false,
                        p?.Origem, p?.AlteradoPorNome, p?.AlteradoEm);
                }).ToList();
                var (min, max) = Limites(c, ops.Select(o => (o.Pontos, o.RepresentaDesconhecido, o.Codigo)).ToList());
                return new CriterioPontosDto(c.Id, c.Codigo, c.Nome, c.Descricao, c.RegraAplicacao, c.Tipo, c.Agregacao, c.MultiplaEscolha,
                    habilitados.GetValueOrDefault(c.Id), min, max, ops);
            }).ToList();

        var faixas = prioridades.Select(p =>
        {
            var f = v.Faixas.FirstOrDefault(x => x.PrioridadeId == p.Id);
            return new FaixaDto(p.Id, p.Codigo, p.Nome, p.Cor, p.Rank, f?.Minimo, f?.Maximo);
        }).ToList();

        var habilitadosDto = criteriosDto.Where(c => c.Habilitado).ToList();
        return new VersaoDetalheDto(v.Id, v.Numero, v.Status, v.Demonstrativa, v.MunicipioId, v.Municipio?.Nome, v.VigenciaInicio, v.Aplicacao,
            v.Justificativa, v.AutorNome, v.CriadaEm, v.AlteradaEm, v.PublicadaPorNome, v.PublicadaEm, v.AprovadaPorNome, v.AprovadaEm,
            v.Versao.ToString(), criteriosDto, faixas,
            habilitadosDto.Sum(c => c.MinimoCriterio ?? 0), habilitadosDto.Sum(c => c.MaximoCriterio ?? 0),
            Pendencias(config), Alertas(config));
    }

    private static (int? Min, int? Max) Limites(Criterio c, List<(int? Pontos, bool Desconhecido, string Codigo)> ops)
    {
        var definidos = ops.Where(o => o.Pontos is not null).ToList();
        if (definidos.Count == 0) return (null, null);
        var min = definidos.Min(o => o.Pontos!.Value);
        var max = definidos.Max(o => o.Pontos!.Value);
        if (c.Agregacao == AgregacaoCriterio.Soma && c.MultiplaEscolha)
        {
            var combinaveis = definidos.Where(o => !o.Desconhecido && !Criterios.Seguranca.Exclusivas.Contains(o.Codigo)).Sum(o => o.Pontos!.Value);
            max = Math.Max(max, combinaveis);
        }
        return (min, max);
    }

    /// <summary>Pendências que impedem publicar: opções sem pontos e faixas com lacuna ou sobreposição.</summary>
    public static List<string> Pendencias(ConfiguracaoClassificacao config)
    {
        var pendencias = new List<string>();
        foreach (var c in config.Criterios.Where(c => c.Habilitado))
            foreach (var o in c.Opcoes.Where(o => o.Pontos is null))
                pendencias.Add($"{c.Nome} › {o.Rotulo}: sem pontos definidos.");

        pendencias.AddRange(ValidarFaixas(config.Faixas, config.Prioridades));
        return pendencias;
    }

    public static List<string> ValidarFaixas(IReadOnlyList<FaixaConfig> faixas, IReadOnlyDictionary<int, PrioridadeConfig> prioridades)
    {
        var erros = new List<string>();
        if (faixas.Count == 0) return ["Defina ao menos uma faixa de prioridade."];
        if (faixas.GroupBy(f => f.PrioridadeId).Any(g => g.Count() > 1)) erros.Add("Uma prioridade tem mais de uma faixa.");

        string Nome(FaixaConfig f) => prioridades.TryGetValue(f.PrioridadeId, out var p) ? p.Nome : $"#{f.PrioridadeId}";
        var ordenadas = faixas.OrderBy(f => f.Minimo).ToList();
        if (ordenadas[0].Minimo != 0) erros.Add($"Lacuna: nenhuma faixa cobre de 0 a {ordenadas[0].Minimo - 1} pontos.");
        for (var i = 0; i < ordenadas.Count; i++)
        {
            var f = ordenadas[i];
            if (f.Maximo is { } max && max < f.Minimo) erros.Add($"Faixa {Nome(f)}: máximo menor que o mínimo.");
            if (i == ordenadas.Count - 1)
            {
                if (f.Maximo is not null) erros.Add($"Lacuna: nenhuma faixa cobre acima de {f.Maximo} pontos (deixe o máximo da última faixa em aberto).");
                break;
            }
            var proxima = ordenadas[i + 1];
            if (f.Maximo is null) erros.Add($"Sobreposição: a faixa {Nome(f)} não tem máximo e há faixas acima dela.");
            else if (proxima.Minimo <= f.Maximo) erros.Add($"Sobreposição entre {Nome(f)} e {Nome(proxima)} ({proxima.Minimo}–{f.Maximo}).");
            else if (proxima.Minimo > f.Maximo + 1) erros.Add($"Lacuna entre {Nome(f)} e {Nome(proxima)}: {f.Maximo + 1}–{proxima.Minimo - 1} pontos.");
        }
        return erros;
    }

    /// <summary>Alertas de coerência: avisam, não bloqueiam (seção 7A.3).</summary>
    public static List<string> Alertas(ConfiguracaoClassificacao config)
    {
        var alertas = new List<string>();
        int? P(string criterio, string opcao) =>
            config.Criterios.FirstOrDefault(c => c.Codigo == criterio && c.Habilitado)?.Opcoes.FirstOrDefault(o => o.Codigo == opcao)?.Pontos;
        void SeMenor(string criterio, string maior, string menor, string texto)
        {
            if (P(criterio, maior) is { } a && P(criterio, menor) is { } b && a < b) alertas.Add(texto);
        }

        SeMenor(Criterios.ClasseCliente, "ESSENCIAL", "RESIDENCIAL", "A classe Essencial vale menos pontos que a Residencial.");
        SeMenor(Criterios.ClasseCliente, "PODER_PUBLICO", "RESIDENCIAL", "A classe Poder público vale menos pontos que a Residencial.");
        SeMenor(Criterios.ServicoEssencial, Criterios.Essencial.Hospital, Criterios.Essencial.NaoIdentificado, "Hospital vale menos que \"serviço essencial não identificado\".");
        SeMenor(Criterios.RiscoSeguranca, Criterios.Seguranca.RiscoChoque, Criterios.Seguranca.SemRiscoAdicional, "Risco de choque vale menos que \"sem risco adicional\".");
        SeMenor(Criterios.RiscoSeguranca, Criterios.Seguranca.CaboEnergizado, Criterios.Seguranca.SemRiscoAdicional, "Cabo energizado vale menos que \"sem risco adicional\".");
        SeMenor(Criterios.CondicaoFornecimento, Criterios.Fornecimento.Total, Criterios.Fornecimento.Parcial, "Interrupção total vale menos que parcial.");
        SeMenor(Criterios.PessoasAfetadas, Criterios.Faixa.MilOuMais, Criterios.Faixa.Ate10, "1.000 ou mais pessoas vale menos que até 10 pessoas.");

        foreach (var c in config.Criterios.Where(c => c.Habilitado))
            foreach (var o in c.Opcoes.Where(o => o.RepresentaDesconhecido && o.Pontos == 0 && !o.RequerConfirmacao))
                alertas.Add($"{c.Nome} › {o.Rotulo} vale 0 sem \"requer confirmação\": a falta de informação será tratada como risco zero.");

        var porRank = config.Faixas.Select(f => (Faixa: f, Rank: config.Prioridades.TryGetValue(f.PrioridadeId, out var p) ? p.Rank : 99))
            .OrderBy(x => x.Rank).ToList();
        for (var i = 1; i < porRank.Count; i++)
            if (porRank[i].Faixa.Minimo > porRank[i - 1].Faixa.Minimo)
                alertas.Add("Uma prioridade menos urgente exige mais pontos que uma mais urgente.");
        return alertas.Distinct().ToList();
    }

    private async Task<List<RegraPrecedenciaDto>> RegrasAsync(CancellationToken ct)
    {
        var regras = await db.RegrasPrecedencia.AsNoTracking().Include(r => r.PrioridadeMinima).OrderByDescending(r => r.NivelPrecedencia).ThenBy(r => r.Codigo).ToListAsync(ct);
        var criterios = await db.Criterios.AsNoTracking().Include(c => c.Opcoes).ToDictionaryAsync(c => c.Codigo, ct);
        return regras.Select(r => MapearRegra(r, criterios)).ToList();
    }

    public static RegraPrecedenciaDto MapearRegra(RegraPrecedencia r, IReadOnlyDictionary<string, Criterio> criterios) =>
        new(r.Id, r.Codigo, r.Nome, r.Descricao,
            r.Condicoes.Select(c => new CondicaoDto(c.Criterio, criterios.TryGetValue(c.Criterio, out var cr) ? cr.Nome : c.Criterio, c.Opcoes.ToList(),
                c.Opcoes.Select(o => cr?.Opcoes.FirstOrDefault(x => x.Codigo == o)?.Rotulo ?? o).ToList())).ToList(),
            r.NivelPrecedencia, r.PrioridadeMinimaId, r.PrioridadeMinima?.Nome ?? "", r.Ativa, r.Demonstrativa, r.Versao, r.AprovadaEm);

    private Task ExigirEscopoAsync(int? municipioId, object entidadeId, CancellationToken ct) =>
        municipioId is { } id
            ? escopo.ExigirAsync(id, "VersaoPontuacao", entidadeId, ct)
            : Task.CompletedTask;

    private async Task<VersaoPontuacao> VigenteGlobalAsync(DateTimeOffset agora, CancellationToken ct) =>
        await db.VersoesPontuacao.AsNoTracking()
            .Where(v => v.MunicipioId == null && (v.Status == StatusVersaoPontuacao.Publicada || v.Status == StatusVersaoPontuacao.Substituida) && v.VigenciaInicio <= agora)
            .OrderByDescending(v => v.VigenciaInicio).ThenByDescending(v => v.Numero).FirstOrDefaultAsync(ct)
        ?? throw new InvalidOperationException("Nenhuma versão global de pontuação publicada.");

    private static VersaoListaDto Lista(VersaoPontuacao v, bool emVigor) =>
        new(v.Id, v.Numero, v.Status, v.Demonstrativa, v.MunicipioId, v.Municipio?.Nome, v.VigenciaInicio, v.Aplicacao, v.Justificativa,
            v.AutorNome, v.CriadaEm, v.PublicadaPorNome, v.PublicadaEm, v.AprovadaPorNome, v.RestauradaDeId, emVigor);
}
