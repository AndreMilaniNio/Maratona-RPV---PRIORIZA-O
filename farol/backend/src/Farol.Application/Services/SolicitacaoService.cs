using System.Text.Json;
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

namespace Farol.Application.Services;

/// <summary>Abertura de solicitação e geração da OS (seção 4.8), em uma única transação.</summary>
public class SolicitacaoService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    IGeradorNumero gerador,
    INotificadorFila notificador,
    EscopoMunicipio escopo,
    Auditor auditor,
    MontadorOrdemServico montador,
    ServicoClassificacao classificacao,
    DuplicidadeService duplicidades)
{
    public async Task<NovaSolicitacaoResponse> CriarAsync(NovaSolicitacaoRequest r, CancellationToken ct = default)
    {
        await escopo.ExigirAsync(r.MunicipioId, "Solicitacao", r.ChaveIdempotencia, ct);

        var existente = await RespostaIdempotenteAsync(r.ChaveIdempotencia, ct);
        if (existente is not null) return existente;

        var agora = relogio.Agora;
        var montada = await montador.MontarAsync(r, agora, ct);
        if (r.OsExistenteId is { } osExistenteId)
            return await VincularAsync(r, montada, osExistenteId, ct);

        var os = montada.Os;
        var municipio = await db.Municipios.FirstAsync(m => m.Id == r.MunicipioId, ct);
        var versao = await classificacao.VersaoVigenteAsync(r.MunicipioId, agora, ct);
        os.VersaoPontuacaoId = versao.Id;
        os.Status = os.LocalizacaoPendente ? StatusOrdemServico.EmTriagem : StatusOrdemServico.AguardandoDespacho;

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var solicitacao = NovaSolicitacao(r, montada, agora);
            solicitacao.Numero = await gerador.ProximoAsync(municipio, "SOL", agora.Year, ct);
            os.Numero = await gerador.ProximoAsync(municipio, "OS", agora.Year, ct);
            os.SolicitacaoId = solicitacao.Id;
            solicitacao.OrdemServicoId = os.Id;

            db.OrdensServico.Add(os);
            db.Solicitacoes.Add(solicitacao);
            db.HistoricosOs.Add(new HistoricoOs
            {
                OrdemServicoId = os.Id,
                OcorridoEm = agora,
                UsuarioId = usuario.Id,
                UsuarioNome = usuario.Nome,
                Tipo = "CRIACAO",
                Descricao = $"OS gerada a partir da solicitação {solicitacao.Numero} ({r.Canal}).",
                StatusNovo = os.Status.ToString(),
            });
            auditor.Registrar(AcoesAuditoria.Criar, "OrdemServico", os.Id, os.MunicipioId,
                novos: new { os.Numero, solicitacao = solicitacao.Numero, os.Status, os.TipoManutencao, os.TipoOcorrenciaId });
            await db.SaveChangesAsync(ct);

            await classificacao.ReclassificarAsync(os, MotivoClassificacao.Criacao, usuario.Id, ct: ct);
            await tx.CommitAsync(ct);

            await notificador.FilaAlteradaAsync(os.MunicipioId, "nova-os", ct);
            return await RespostaAsync(solicitacao, os, false, montada.Alertas, ct);
        }
        catch (DbUpdateException)
        {
            // Envio duplicado concorrente: a outra transação venceu; devolve o resultado dela.
            await tx.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
            return await RespostaIdempotenteAsync(r.ChaveIdempotencia, ct) ?? throw new ConflitoException(
                "Não foi possível registrar a solicitação por um conflito de gravação. Tente novamente.");
        }
    }

    /// <summary>Calcula a classificação que a solicitação receberia, sem gravar nada.</summary>
    public async Task<ClassificacaoDto> PreviaAsync(NovaSolicitacaoRequest r, CancellationToken ct = default)
    {
        await escopo.ExigirAsync(r.MunicipioId, "Solicitacao", "previa", ct);
        var agora = relogio.Agora;
        var montada = await montador.MontarAsync(r, agora, ct);
        var versaoId = r.OsExistenteId is { } osId
            ? await db.OrdensServico.Where(o => o.Id == osId).Select(o => o.VersaoPontuacaoId).FirstAsync(ct)
            : (await classificacao.VersaoVigenteAsync(r.MunicipioId, agora, ct)).Id;
        var versao = await db.VersoesPontuacao.AsNoTracking().Include(v => v.Municipio).FirstAsync(v => v.Id == versaoId, ct);
        var config = await classificacao.CarregarAsync(versao.Id, r.MunicipioId, ct);
        var calculo = await classificacao.CalcularAsync(montada.Os, config, agora, ct);
        var prioridades = await db.Prioridades.AsNoTracking().ToDictionaryAsync(p => p.Id, ct);
        return MapeamentoClassificacao.DeCalculo(calculo, prioridades, MapeamentoClassificacao.Versao(versao), agora, montada.Alertas);
    }

    public async Task<SolicitacaoDto> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var s = await db.Solicitacoes.AsNoTracking()
            .Include(x => x.Municipio).Include(x => x.Ucs).Include(x => x.OrdemServico)
            .FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NaoEncontradoException("Solicitação não encontrada.");
        await escopo.ExigirAsync(s.MunicipioId, "Solicitacao", id, ct);
        var autor = await db.Users.Where(u => u.Id == s.RegistradaPorId).Select(u => u.Nome).FirstOrDefaultAsync(ct) ?? "—";
        return new SolicitacaoDto(s.Id, s.Numero, s.MunicipioId, s.Municipio!.Nome, s.Canal, s.Origem, s.ProtocoloExterno,
            s.RegistradaEm, autor, s.Descricao, s.UcNaoInformada, s.MotivoUcNaoInformada,
            s.Ucs.Select(u => new SolicitacaoUcDto(u.Numero, u.ValidadaNoCadastro)).ToList(),
            s.OrdemServicoId, s.OrdemServico!.Numero, s.DadosInformados);
    }

    /// <summary>
    /// Vincula a solicitação a uma OS existente escolhida pelo atendente: nenhuma OS nova; os fatos
    /// conhecidos completam os desconhecidos, as faixas de impacto ficam com a maior, e a OS é reclassificada.
    /// </summary>
    private async Task<NovaSolicitacaoResponse> VincularAsync(
        NovaSolicitacaoRequest r, OsMontada montada, Guid osId, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var os = await CarregarOsCompletaAsync(osId, ct) ?? throw new NaoEncontradoException("OS para vínculo não encontrada.");
        if (os.MunicipioId != r.MunicipioId)
            throw new ConflitoException("A OS escolhida pertence a outro município.");
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status))
            throw new ConflitoException("A OS escolhida já está encerrada.");

        var municipio = await db.Municipios.FirstAsync(m => m.Id == r.MunicipioId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var solicitacao = NovaSolicitacao(r, montada, agora);
        solicitacao.Numero = await gerador.ProximoAsync(municipio, "SOL", agora.Year, ct);
        solicitacao.OrdemServicoId = os.Id;
        db.Solicitacoes.Add(solicitacao);

        FundirFatos(os, montada.Os);
        db.VinculosSolicitacao.Add(new VinculoSolicitacao
        {
            SolicitacaoId = solicitacao.Id,
            OrdemServicoId = os.Id,
            VinculadoPorId = usuario.Id,
            VinculadoEm = agora,
            Justificativa = "Vinculada pelo atendente na abertura: mesma ocorrência.",
        });
        db.HistoricosOs.Add(new HistoricoOs
        {
            OrdemServicoId = os.Id,
            OcorridoEm = agora,
            UsuarioId = usuario.Id,
            UsuarioNome = usuario.Nome,
            Tipo = "VINCULO",
            Descricao = $"Solicitação {solicitacao.Numero} vinculada a esta OS.",
        });
        auditor.Registrar(AcoesAuditoria.Vincular, "OrdemServico", os.Id, os.MunicipioId, novos: new { solicitacao = solicitacao.Numero });
        await db.SaveChangesAsync(ct);

        await classificacao.ReclassificarAsync(os, MotivoClassificacao.Vinculo, usuario.Id, ct: ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(os.MunicipioId, "vinculo", ct);
        return await RespostaAsync(solicitacao, os, true, montada.Alertas, ct);
    }

    /// <summary>Completa a OS com os fatos de um novo relato da mesma ocorrência.</summary>
    public static void FundirFatos(OrdemServico destino, OrdemServico relato)
    {
        static string Conhecido(string atual, string novo, string desconhecido) => atual == desconhecido ? novo : atual;

        destino.PessoasAfetadas = MaiorFaixa(destino.PessoasAfetadas, relato.PessoasAfetadas);
        destino.UcsAfetadas = MaiorFaixa(destino.UcsAfetadas, relato.UcsAfetadas);
        destino.QuantidadePessoas = Max(destino.QuantidadePessoas, relato.QuantidadePessoas);
        destino.QuantidadeUcs = Max(destino.QuantidadeUcs, relato.QuantidadeUcs);
        destino.ServicoEssencial = Conhecido(destino.ServicoEssencial, relato.ServicoEssencial, Criterios.Essencial.NaoIdentificado);
        destino.SituacaoCliente = Conhecido(destino.SituacaoCliente, relato.SituacaoCliente, Criterios.Situacao.NaoInformada);
        destino.CondicaoFornecimento = Conhecido(destino.CondicaoFornecimento, relato.CondicaoFornecimento, Criterios.Fornecimento.Desconhecida);
        destino.Redundancia = Conhecido(destino.Redundancia, relato.Redundancia, Criterios.Confirmacao.Desconhecida);
        destino.FonteReserva = Conhecido(destino.FonteReserva, relato.FonteReserva, Criterios.Confirmacao.Desconhecida);
        destino.EquipeEspecializada = Conhecido(destino.EquipeEspecializada, relato.EquipeEspecializada, Criterios.Confirmacao.Desconhecida);
        destino.EquipamentoAfetado = Conhecido(destino.EquipamentoAfetado, relato.EquipamentoAfetado, Criterios.Equipamento.NaoIdentificado);
        destino.Abrangencia = Conhecido(destino.Abrangencia, relato.Abrangencia, Criterios.Abrange.Desconhecida);
        destino.NivelRede = Conhecido(destino.NivelRede, relato.NivelRede, Criterios.Rede.NaoIdentificado);
        destino.DuracaoEstimadaMin ??= relato.DuracaoEstimadaMin;

        var condicoes = destino.CondicoesSeguranca.Select(c => c.Codigo)
            .Concat(relato.CondicoesSeguranca.Select(c => c.Codigo))
            .Distinct().ToList();
        var riscosReais = condicoes.Where(c => !Criterios.Seguranca.Exclusivas.Contains(c)).ToList();
        var final = riscosReais.Count > 0 ? riscosReais
            : condicoes.Contains(Criterios.Seguranca.SemRiscoAdicional) ? [Criterios.Seguranca.SemRiscoAdicional]
            : condicoes.Take(1).ToList();
        destino.CondicoesSeguranca.RemoveAll(c => !final.Contains(c.Codigo));
        foreach (var codigo in final.Where(c => destino.CondicoesSeguranca.All(x => x.Codigo != c)))
            destino.CondicoesSeguranca.Add(new OsCondicaoSeguranca { OrdemServicoId = destino.Id, Codigo = codigo });

        foreach (var classe in relato.Classes.Where(c => destino.Classes.All(x => x.ClasseClienteId != c.ClasseClienteId)))
            destino.Classes.Add(new OsClasseCliente { OrdemServicoId = destino.Id, ClasseClienteId = classe.ClasseClienteId });

        if (destino.Latitude is null && relato.Latitude is not null)
        {
            destino.Latitude = relato.Latitude;
            destino.Longitude = relato.Longitude;
            destino.OrigemCoordenada = relato.OrigemCoordenada;
            destino.PrecisaoMetros = relato.PrecisaoMetros;
            destino.CoordenadaRegistradaEm = relato.CoordenadaRegistradaEm;
        }
        destino.LocalizacaoPendente = destino.Latitude is null && destino.Logradouro is null;
    }

    private static string MaiorFaixa(string a, string b)
    {
        var ordem = Criterios.Faixa.Todas;
        if (a == Criterios.Faixa.Desconhecida) return b;
        if (b == Criterios.Faixa.Desconhecida) return a;
        return Array.IndexOf(ordem, a) >= Array.IndexOf(ordem, b) ? a : b;
    }

    private static int? Max(int? a, int? b) => a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);

    private Solicitacao NovaSolicitacao(NovaSolicitacaoRequest r, OsMontada montada, DateTimeOffset agora)
    {
        var id = Guid.NewGuid();
        foreach (var uc in montada.Ucs) uc.SolicitacaoId = id;
        return new Solicitacao
        {
            Id = id,
            Numero = string.Empty,
            ChaveIdempotencia = r.ChaveIdempotencia,
            MunicipioId = r.MunicipioId,
            Canal = r.Canal,
            Origem = r.Origem,
            ProtocoloExterno = r.ProtocoloExterno,
            RegistradaEm = agora,
            RegistradaPorId = usuario.Id,
            Descricao = r.Descricao.Trim(),
            UcNaoInformada = r.UcNaoInformada,
            MotivoUcNaoInformada = r.UcNaoInformada ? r.MotivoUcNaoInformada?.Trim() : null,
            DadosInformados = JsonSerializer.Serialize(r, Auditor.Json),
            Ucs = montada.Ucs,
        };
    }

    private Task<OrdemServico?> CarregarOsCompletaAsync(Guid id, CancellationToken ct) =>
        db.OrdensServico
            .Include(o => o.CondicoesSeguranca).Include(o => o.Classes)
            .Include(o => o.RecursosNecessarios).Include(o => o.RespostasPersonalizadas)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    private async Task<NovaSolicitacaoResponse?> RespostaIdempotenteAsync(Guid chave, CancellationToken ct)
    {
        var s = await db.Solicitacoes.AsNoTracking().FirstOrDefaultAsync(x => x.ChaveIdempotencia == chave, ct);
        if (s is null) return null;
        var os = await db.OrdensServico.AsNoTracking().FirstAsync(o => o.Id == s.OrdemServicoId, ct);
        return await RespostaAsync(s, os, os.SolicitacaoId != s.Id, [], ct);
    }

    private async Task<NovaSolicitacaoResponse> RespostaAsync(
        Solicitacao s, OrdemServico os, bool vinculada, List<string> alertas, CancellationToken ct)
    {
        var prioridade = await db.Prioridades.AsNoTracking().FirstAsync(p => p.Id == os.PrioridadeId, ct);
        var demonstrativa = await db.VersoesPontuacao.Where(v => v.Id == os.VersaoPontuacaoId).Select(v => v.Demonstrativa).FirstAsync(ct);
        var ucs = await db.Solicitacoes.Where(x => x.Id == s.Id).SelectMany(x => x.Ucs.Select(u => u.Numero)).ToListAsync(ct);
        var sugestoes = vinculada ? [] : await duplicidades.SugerirAsync(new CriterioDuplicidade(
            os.MunicipioId, os.Latitude, os.Longitude, os.TransformadorNumero, ucs, os.ConjuntoId, os.TipoOcorrenciaId, os.Id), ct);

        return new NovaSolicitacaoResponse(s.Id, s.Numero, os.Id, os.Numero, vinculada,
            MapeamentoClassificacao.Resumo(prioridade), os.Pontuacao, demonstrativa, alertas, sugestoes);
    }
}
