using Farol.Domain.Entities;
using Farol.Domain.Enums;

namespace Farol.Application.Prioritization;

/// <summary>Respostas da OS por critério: código do critério → códigos das opções escolhidas.</summary>
public sealed record FatosOcorrencia(IReadOnlyDictionary<string, IReadOnlyList<string>> Respostas);

public sealed record OpcaoConfig(string Codigo, string Rotulo, int? Pontos, bool RequerConfirmacao, bool RepresentaDesconhecido);

public sealed record CriterioConfig(
    string Codigo,
    string Nome,
    AgregacaoCriterio Agregacao,
    bool Habilitado,
    IReadOnlyList<OpcaoConfig> Opcoes);

public sealed record PrioridadeConfig(int Id, string Codigo, string Nome, int Rank);

public sealed record FaixaConfig(int PrioridadeId, int Minimo, int? Maximo);

public sealed record RegraConfig(string Codigo, string Nome, IReadOnlyList<CondicaoRegra> Condicoes, int NivelPrecedencia, int PrioridadeMinimaId);

/// <summary>Tudo de que o motor precisa, já resolvido para uma versão e um município.</summary>
public sealed record ConfiguracaoClassificacao(
    int VersaoId,
    int VersaoNumero,
    int? VersaoMunicipioId,
    bool Demonstrativa,
    IReadOnlyList<CriterioConfig> Criterios,
    IReadOnlyList<FaixaConfig> Faixas,
    IReadOnlyList<RegraConfig> Regras,
    IReadOnlyDictionary<int, PrioridadeConfig> Prioridades);

public sealed record ItemClassificacao(
    string CriterioCodigo,
    string CriterioNome,
    IReadOnlyList<string> OpcoesCodigos,
    IReadOnlyList<string> OpcoesRotulos,
    int Pontos,
    bool SemPontosDefinidos,
    bool RequerConfirmacao);

public sealed record ResultadoClassificacao(
    int Pontuacao,
    int NivelPrecedencia,
    PrioridadeConfig Prioridade,
    PrioridadeConfig PrioridadePelaFaixa,
    IReadOnlyList<ItemClassificacao> Itens,
    IReadOnlyList<RegraAplicada> RegrasAplicadas,
    string MotivoPrincipal);

/// <summary>
/// Classificação em duas camadas (seção 7.3): A — regras de precedência e segurança;
/// B — soma dos pontos configurados. Função pura: mesma entrada e mesma versão, mesmo resultado.
/// É o único lugar onde a classificação é calculada — produção, prévia, simulador e impacto usam este motor.
/// </summary>
public static class MotorPriorizacao
{
    public static ResultadoClassificacao Classificar(FatosOcorrencia fatos, ConfiguracaoClassificacao config)
    {
        if (config.Faixas.Count == 0)
            throw new InvalidOperationException($"A versão de pontuação {config.VersaoNumero} não possui faixas de prioridade.");

        // Camada B: pontos por critério habilitado
        var itens = new List<ItemClassificacao>();
        foreach (var criterio in config.Criterios.Where(c => c.Habilitado))
            itens.Add(PontuarCriterio(criterio, fatos));

        var pontuacao = itens.Sum(i => i.Pontos);
        var pelaFaixa = PrioridadePorFaixa(pontuacao, config);

        // Camada A: regras de precedência
        var aplicadas = config.Regras
            .Where(r => Atende(r, fatos))
            .OrderByDescending(r => r.NivelPrecedencia)
            .ThenBy(r => config.Prioridades[r.PrioridadeMinimaId].Rank)
            .ThenBy(r => r.Codigo, StringComparer.Ordinal)
            .ToList();

        var prioridade = pelaFaixa;
        foreach (var regra in aplicadas)
        {
            var minima = config.Prioridades[regra.PrioridadeMinimaId];
            if (minima.Rank < prioridade.Rank) prioridade = minima;
        }

        var nivel = aplicadas.Count == 0 ? 0 : aplicadas.Max(r => r.NivelPrecedencia);
        var regras = aplicadas
            .Select(r => new RegraAplicada(r.Codigo, r.Nome, r.NivelPrecedencia, config.Prioridades[r.PrioridadeMinimaId].Codigo))
            .ToList();

        return new ResultadoClassificacao(pontuacao, nivel, prioridade, pelaFaixa, itens, regras,
            MotivoPrincipal(aplicadas, itens, prioridade, pelaFaixa));
    }

    private static ItemClassificacao PontuarCriterio(CriterioConfig criterio, FatosOcorrencia fatos)
    {
        var codigos = fatos.Respostas.TryGetValue(criterio.Codigo, out var r) && r.Count > 0
            ? r
            : criterio.Opcoes.Where(o => o.RepresentaDesconhecido).Select(o => o.Codigo).Take(1).ToList();

        var opcoes = codigos
            .Select(c => criterio.Opcoes.FirstOrDefault(o => o.Codigo == c))
            .Where(o => o is not null)
            .Cast<OpcaoConfig>()
            .ToList();

        if (opcoes.Count == 0)
        {
            // Sem resposta e sem opção "desconhecida" cadastrada: registrado e sinalizado, nunca silencioso.
            return new ItemClassificacao(criterio.Codigo, criterio.Nome, codigos.ToList(), ["Sem resposta"], 0, true, true);
        }

        var semPontos = opcoes.Any(o => o.Pontos is null);
        var valores = opcoes.Select(o => o.Pontos ?? 0).ToList();
        var pontos = criterio.Agregacao == AgregacaoCriterio.Soma ? valores.Sum() : valores.Max();

        return new ItemClassificacao(
            criterio.Codigo,
            criterio.Nome,
            opcoes.Select(o => o.Codigo).ToList(),
            opcoes.Select(o => o.Rotulo).ToList(),
            pontos,
            semPontos,
            semPontos || opcoes.Any(o => o.RequerConfirmacao));
    }

    private static PrioridadeConfig PrioridadePorFaixa(int pontuacao, ConfiguracaoClassificacao config)
    {
        var faixa = config.Faixas.FirstOrDefault(f => pontuacao >= f.Minimo && (f.Maximo is null || pontuacao <= f.Maximo));
        if (faixa is not null) return config.Prioridades[faixa.PrioridadeId];

        // Fora de todas as faixas (só ocorre com faixas incompletas): abaixo do mínimo → menos urgente; acima → mais urgente.
        var ordenadas = config.Faixas.OrderBy(f => f.Minimo).ToList();
        return pontuacao < ordenadas[0].Minimo
            ? config.Prioridades[ordenadas[0].PrioridadeId]
            : config.Prioridades[ordenadas[^1].PrioridadeId];
    }

    private static bool Atende(RegraConfig regra, FatosOcorrencia fatos) =>
        regra.Condicoes.Count > 0 &&
        regra.Condicoes.All(c => fatos.Respostas.TryGetValue(c.Criterio, out var respostas) && respostas.Any(c.Opcoes.Contains));

    private static string MotivoPrincipal(
        List<RegraConfig> aplicadas, List<ItemClassificacao> itens, PrioridadeConfig final, PrioridadeConfig pelaFaixa)
    {
        if (aplicadas.Count > 0 && final.Rank < pelaFaixa.Rank)
            return $"Regra de precedência \"{aplicadas[0].Nome}\" elevou a prioridade para {final.Nome}.";
        if (aplicadas.Count > 0)
            return $"Regra de precedência \"{aplicadas[0].Nome}\" aplicada; prioridade {final.Nome} pela pontuação.";

        var maior = itens.Where(i => i.Pontos > 0).OrderByDescending(i => i.Pontos).ThenBy(i => i.CriterioCodigo, StringComparer.Ordinal).FirstOrDefault();
        return maior is null
            ? $"Prioridade {final.Nome} pela pontuação total."
            : $"Maior contribuição: {maior.CriterioNome} — {string.Join(", ", maior.OpcoesRotulos)} (+{maior.Pontos}).";
    }
}
