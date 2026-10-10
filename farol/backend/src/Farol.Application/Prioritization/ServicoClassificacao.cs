using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Prioritization;

/// <summary>
/// Liga o motor puro ao banco: escolhe a versão, monta o contexto, grava o resultado imutável
/// e mantém na OS a classificação corrente e os prazos.
/// </summary>
public class ServicoClassificacao(IAppDbContext db, IRelogio relogio, IOptions<FarolOptions> opcoes)
{
    private readonly TimeZoneInfo _fuso = TimeZoneInfo.FindSystemTimeZoneById(opcoes.Value.FusoHorario);

    public TimeZoneInfo Fuso => _fuso;

    /// <summary>
    /// Versão em vigor para o município no instante: a da cidade prevalece sobre a global (seção 3A.7).
    /// Sem nenhuma publicada, vale a demonstrativa.
    /// </summary>
    public async Task<VersaoPontuacao> VersaoVigenteAsync(int municipioId, DateTimeOffset instante, CancellationToken ct = default)
    {
        var candidatas = await db.VersoesPontuacao.AsNoTracking()
            .Where(v => (v.Status == StatusVersaoPontuacao.Publicada || v.Status == StatusVersaoPontuacao.Substituida)
                        && v.VigenciaInicio <= instante
                        && (v.MunicipioId == null || v.MunicipioId == municipioId))
            .ToListAsync(ct);

        var vigente = candidatas.Where(v => v.MunicipioId == municipioId).OrderByDescending(v => v.VigenciaInicio).ThenByDescending(v => v.Numero).FirstOrDefault()
                      ?? candidatas.Where(v => v.MunicipioId == null).OrderByDescending(v => v.VigenciaInicio).ThenByDescending(v => v.Numero).FirstOrDefault();

        return vigente ?? throw new InvalidOperationException(
            "Nenhuma versão de pontuação publicada. Carregue o conjunto demonstrativo ou publique uma versão.");
    }

    /// <summary>Carrega a configuração de uma versão (publicada ou rascunho) para um município.</summary>
    public async Task<ConfiguracaoClassificacao> CarregarAsync(int versaoId, int? municipioId, CancellationToken ct = default)
    {
        var versao = await db.VersoesPontuacao.AsNoTracking()
            .Include(v => v.Pontos)
            .Include(v => v.Faixas)
            .Include(v => v.Criterios)
            .FirstAsync(v => v.Id == versaoId, ct);

        var criterios = await db.Criterios.AsNoTracking()
            .Include(c => c.Opcoes)
            .Include(c => c.Municipios)
            .Where(c => c.Ativo)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Id)
            .ToListAsync(ct);

        var regras = await db.RegrasPrecedencia.AsNoTracking().Where(r => r.Ativa).ToListAsync(ct);
        var prioridades = await db.Prioridades.AsNoTracking().Where(p => p.Ativo).ToListAsync(ct);

        return Montar(versao, criterios, regras, prioridades, municipioId);
    }

    public static ConfiguracaoClassificacao Montar(
        VersaoPontuacao versao, IEnumerable<Criterio> criterios, IEnumerable<RegraPrecedencia> regras,
        IEnumerable<Prioridade> prioridades, int? municipioId)
    {
        var pontos = versao.Pontos.ToDictionary(p => p.OpcaoId);
        var habilitados = versao.Criterios.ToDictionary(c => c.CriterioId, c => c.Habilitado);

        var configCriterios = criterios
            .Where(c => municipioId is null || c.Municipios.Count == 0 || c.Municipios.Any(m => m.MunicipioId == municipioId))
            .Select(c => new CriterioConfig(
                c.Codigo,
                c.Nome,
                c.Agregacao,
                habilitados.TryGetValue(c.Id, out var h) && h,
                c.Opcoes.Where(o => o.Ativa).OrderBy(o => o.Ordem).Select(o => pontos.TryGetValue(o.Id, out var p)
                    ? new OpcaoConfig(o.Codigo, o.Rotulo, p.Pontos, p.RequerConfirmacao, o.RepresentaDesconhecido)
                    : new OpcaoConfig(o.Codigo, o.Rotulo, null, true, o.RepresentaDesconhecido)).ToList()))
            .ToList();

        var mapaPrioridades = prioridades.ToDictionary(p => p.Id, p => new PrioridadeConfig(p.Id, p.Codigo, p.Nome, p.Rank));

        return new ConfiguracaoClassificacao(
            versao.Id,
            versao.Numero,
            versao.MunicipioId,
            versao.Demonstrativa,
            configCriterios,
            versao.Faixas.Where(f => mapaPrioridades.ContainsKey(f.PrioridadeId)).Select(f => new FaixaConfig(f.PrioridadeId, f.Minimo, f.Maximo)).ToList(),
            regras.Where(r => mapaPrioridades.ContainsKey(r.PrioridadeMinimaId))
                .Select(r => new RegraConfig(r.Codigo, r.Nome, r.Condicoes, r.NivelPrecedencia, r.PrioridadeMinimaId)).ToList(),
            mapaPrioridades);
    }

    /// <summary>Monta o contexto de cadastro de uma OS (carregada ou ainda não gravada).</summary>
    public async Task<ContextoFatos> ContextoAsync(OrdemServico os, CancellationToken ct = default)
    {
        var tipoCodigo = await db.TiposOcorrencia.Where(t => t.Id == os.TipoOcorrenciaId).Select(t => t.Codigo).FirstAsync(ct);

        var classesIds = os.Classes.Select(c => c.ClasseClienteId).ToList();
        var classes = await db.ClassesCliente.Where(c => classesIds.Contains(c.Id)).Select(c => c.Codigo).ToListAsync(ct);

        int? ucsNoTransformador = os.TransformadorId is { } tid
            ? await db.UnidadesConsumidoras.CountAsync(u => u.TransformadorId == tid, ct)
            : null;

        var respostas = new Dictionary<string, IReadOnlyList<string>>();
        if (os.RespostasPersonalizadas.Count > 0)
        {
            var opcoesIds = os.RespostasPersonalizadas.Select(r => r.OpcaoId).ToList();
            var opcoes = await db.OpcoesCriterio.Include(o => o.Criterio).Where(o => opcoesIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var grupo in opcoes.GroupBy(o => o.Criterio!.Codigo))
                respostas[grupo.Key] = grupo.Select(o => o.Codigo).ToList();
        }

        return new ContextoFatos(tipoCodigo, classes, ucsNoTransformador, respostas);
    }

    public async Task<ResultadoClassificacao> CalcularAsync(
        OrdemServico os, ConfiguracaoClassificacao config, DateTimeOffset agora, CancellationToken ct = default)
    {
        var contexto = await ContextoAsync(os, ct);
        return MotorPriorizacao.Classificar(ExtratorFatos.Extrair(os, contexto, agora), config);
    }

    /// <summary>
    /// Classifica a OS com a versão a que ela está vinculada e grava um novo resultado.
    /// Com <paramref name="somenteSeMudou"/>, nada é gravado quando código, pontos e precedência não mudam.
    /// A OS precisa estar gravada (o resultado referencia o id dela).
    /// </summary>
    public async Task<ResultadoPrioridade?> ReclassificarAsync(
        OrdemServico os, MotivoClassificacao motivo, Guid? usuarioId = null, string? justificativa = null,
        bool somenteSeMudou = false, ConfiguracaoClassificacao? config = null, CancellationToken ct = default)
    {
        var agora = relogio.Agora;
        config ??= await CarregarAsync(os.VersaoPontuacaoId, os.MunicipioId, ct);
        var calculo = await CalcularAsync(os, config, agora, ct);

        var prioridadeFinalId = os.PrioridadeManualId ?? calculo.Prioridade.Id;
        if (somenteSeMudou &&
            os.PrioridadeCalculadaId == calculo.Prioridade.Id &&
            os.PrioridadeId == prioridadeFinalId &&
            os.Pontuacao == calculo.Pontuacao &&
            os.NivelPrecedencia == calculo.NivelPrecedencia)
            return null;

        var resultado = new ResultadoPrioridade
        {
            OrdemServicoId = os.Id,
            VersaoPontuacaoId = config.VersaoId,
            CalculadoEm = agora,
            Motivo = motivo,
            Pontuacao = calculo.Pontuacao,
            NivelPrecedencia = calculo.NivelPrecedencia,
            PrioridadeCalculadaId = calculo.Prioridade.Id,
            PrioridadeId = prioridadeFinalId,
            RegrasAplicadas = calculo.RegrasAplicadas.ToList(),
            MotivoPrincipal = os.PrioridadeManualId is null
                ? calculo.MotivoPrincipal
                : $"Prioridade revista manualmente. Cálculo: {calculo.Prioridade.Nome} — {calculo.MotivoPrincipal}",
            UsuarioId = usuarioId,
            Justificativa = justificativa,
            Itens = calculo.Itens.Select(i => new ResultadoPrioridadeItem
            {
                CriterioCodigo = i.CriterioCodigo,
                CriterioNome = i.CriterioNome,
                OpcoesCodigos = string.Join(",", i.OpcoesCodigos),
                OpcoesRotulos = string.Join(", ", i.OpcoesRotulos),
                Pontos = i.Pontos,
                SemPontosDefinidos = i.SemPontosDefinidos,
                RequerConfirmacao = i.RequerConfirmacao,
            }).ToList(),
        };

        db.ResultadosPrioridade.Add(resultado);
        await db.SaveChangesAsync(ct);

        var prioridadeMudou = os.PrioridadeId != prioridadeFinalId;
        os.ResultadoAtualId = resultado.Id;
        os.PrioridadeCalculadaId = calculo.Prioridade.Id;
        os.PrioridadeId = prioridadeFinalId;
        os.Pontuacao = calculo.Pontuacao;
        os.NivelPrecedencia = calculo.NivelPrecedencia;
        os.AtualizadaEm = agora;
        if (prioridadeMudou || os.PrazoConclusao is null)
            await AplicarPrazosAsync(os, ct);

        if (prioridadeMudou && motivo != MotivoClassificacao.Criacao)
        {
            var nova = await db.Prioridades.Where(p => p.Id == prioridadeFinalId).Select(p => p.Nome).FirstAsync(ct);
            db.HistoricosOs.Add(new HistoricoOs
            {
                OrdemServicoId = os.Id,
                OcorridoEm = agora,
                UsuarioId = usuarioId,
                Tipo = "CLASSIFICACAO",
                Descricao = $"Prioridade alterada para {nova} ({calculo.Pontuacao} pontos). Motivo: {Descrever(motivo)}.",
                Justificativa = justificativa,
            });
        }

        await db.SaveChangesAsync(ct);
        return resultado;
    }

    /// <summary>Recalcula os cinco prazos a partir da abertura, com a prioridade em vigor.</summary>
    public async Task AplicarPrazosAsync(OrdemServico os, CancellationToken ct = default)
    {
        if (os.PrioridadeId is not { } prioridadeId) return;
        var prioridade = await db.Prioridades.AsNoTracking().FirstAsync(p => p.Id == prioridadeId, ct);
        var feriados = (await db.Feriados.AsNoTracking()
                .Where(f => f.MunicipioId == null || f.MunicipioId == os.MunicipioId)
                .Select(f => f.Data).ToListAsync(ct))
            .ToHashSet();

        var prazos = CalculadoraPrazos.Calcular(prioridade, os.AbertaEm, _fuso, feriados);
        os.PrazoTriagem = prazos.Triagem;
        os.PrazoDespacho = prazos.Despacho;
        os.PrazoInicio = prazos.Inicio;
        os.PrazoRestabelecimento = prazos.Restabelecimento;
        os.PrazoConclusao = prazos.Conclusao;
    }

    public static string Descrever(MotivoClassificacao motivo) => motivo switch
    {
        MotivoClassificacao.Criacao => "abertura da OS",
        MotivoClassificacao.AtualizacaoDados => "atualização de dados da ocorrência",
        MotivoClassificacao.Tempo => "passagem do tempo (espera ou prazo)",
        MotivoClassificacao.NovaVersao => "nova versão de pontuação",
        MotivoClassificacao.Manual => "revisão manual",
        MotivoClassificacao.Vinculo => "nova solicitação vinculada",
        MotivoClassificacao.TrocaMunicipio => "troca de município",
        _ => motivo.ToString(),
    };
}
