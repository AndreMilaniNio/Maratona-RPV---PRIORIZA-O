using Farol.Application.DTOs;
using Farol.Application.Prioritization;
using Farol.Domain.Entities;

namespace Farol.Application.Mapping;

public static class MapeamentoClassificacao
{
    public static PrioridadeResumoDto Resumo(Prioridade p) => new(p.Id, p.Codigo, p.Nome, p.Cor, p.Rank, p.Critica);

    public static ClassificacaoDto DeCalculo(
        ResultadoClassificacao r, IReadOnlyDictionary<int, Prioridade> prioridades, VersaoResumoDto versao,
        DateTimeOffset calculadoEm, List<string>? alertas = null) =>
        new(
            r.Pontuacao,
            Resumo(prioridades[r.Prioridade.Id]),
            Resumo(prioridades[r.PrioridadePelaFaixa.Id]),
            r.NivelPrecedencia,
            r.MotivoPrincipal,
            r.Itens.Select(i => new ItemPontuacaoDto(i.CriterioCodigo, i.CriterioNome, i.OpcoesRotulos.ToList(),
                i.OpcoesCodigos.ToList(), i.Pontos, i.SemPontosDefinidos, i.RequerConfirmacao)).ToList(),
            r.RegrasAplicadas.Select(Regra).ToList(),
            versao,
            calculadoEm,
            alertas ?? []);

    public static ClassificacaoDto DeResultado(
        ResultadoPrioridade r, IReadOnlyDictionary<int, Prioridade> prioridades, VersaoResumoDto versao) =>
        new(
            r.Pontuacao,
            Resumo(prioridades[r.PrioridadeId]),
            Resumo(prioridades[r.PrioridadeCalculadaId]),
            r.NivelPrecedencia,
            r.MotivoPrincipal,
            r.Itens.OrderBy(i => i.Id).Select(i => new ItemPontuacaoDto(i.CriterioCodigo, i.CriterioNome,
                i.OpcoesRotulos.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
                i.OpcoesCodigos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                i.Pontos, i.SemPontosDefinidos, i.RequerConfirmacao)).ToList(),
            r.RegrasAplicadas.Select(Regra).ToList(),
            versao,
            r.CalculadoEm,
            []);

    public static VersaoResumoDto Versao(VersaoPontuacao v) =>
        new(v.Id, v.Numero, v.MunicipioId, v.Municipio?.Nome, v.Demonstrativa, v.VigenciaInicio);

    private static RegraAplicadaDto Regra(RegraAplicada r) => new(r.Codigo, r.Nome, r.NivelPrecedencia, r.PrioridadeMinima);
}
