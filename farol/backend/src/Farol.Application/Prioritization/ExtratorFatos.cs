using Farol.Domain.Entities;
using Farol.Domain.Rules;

namespace Farol.Application.Prioritization;

/// <summary>Dados de cadastro que a OS referencia por id e o motor precisa por código.</summary>
public sealed record ContextoFatos(
    string TipoOcorrenciaCodigo,
    IReadOnlyList<string> ClassesCodigos,
    int? UcsNoTransformador,
    IReadOnlyDictionary<string, IReadOnlyList<string>> RespostasPersonalizadas);

/// <summary>
/// Traduz a OS em respostas por critério. Fato ausente vira a opção "desconhecida" do critério,
/// nunca ausência de pontos (Key decision 4).
/// </summary>
public static class ExtratorFatos
{
    public static FatosOcorrencia Extrair(OrdemServico os, ContextoFatos contexto, DateTimeOffset agora)
    {
        static IReadOnlyList<string> Um(string valor) => [valor];

        var respostas = new Dictionary<string, IReadOnlyList<string>>
        {
            [Criterios.RiscoSeguranca] = os.CondicoesSeguranca.Count > 0
                ? os.CondicoesSeguranca.Select(c => c.Codigo).Order(StringComparer.Ordinal).ToList()
                : Um(Criterios.Seguranca.Desconhecida),
            [Criterios.ServicoEssencial] = Um(os.ServicoEssencial),
            [Criterios.FonteReserva] = Um(os.FonteReserva),
            [Criterios.PessoasAfetadas] = Um(os.PessoasAfetadas),
            [Criterios.UcsAfetadas] = Um(os.UcsAfetadas),
            [Criterios.CondicaoFornecimento] = Um(os.CondicaoFornecimento),
            [Criterios.DuracaoInterrupcao] = Um(Criterios.Duracao.DeMinutos(os.DuracaoEstimadaMin)),
            [Criterios.TipoOcorrencia] = Um(contexto.TipoOcorrenciaCodigo),
            [Criterios.EquipamentoAfetado] = Um(os.EquipamentoAfetado),
            [Criterios.Abrangencia] = Um(os.Abrangencia),
            [Criterios.NivelRede] = Um(os.NivelRede),
            [Criterios.UcsTransformador] = Um(Criterios.UcsNoTransformador.DeQuantidade(contexto.UcsNoTransformador)),
            [Criterios.TempoEspera] = Um(Criterios.Espera.DeDuracao(agora - os.AbertaEm)),
            [Criterios.ProximidadePrazo] = Um(Criterios.Prazo.DeLimite(os.DataLimite, agora)),
            [Criterios.TipoManutencao] = Um(os.TipoManutencao == Domain.Enums.TipoManutencao.Corretiva
                ? Criterios.Manutencao.Corretiva
                : Criterios.Manutencao.Preventiva),
            [Criterios.Redundancia] = Um(os.Redundancia),
            [Criterios.EquipeEspecializada] = Um(os.EquipeEspecializada),
            [Criterios.ClasseCliente] = contexto.ClassesCodigos.Count > 0
                ? contexto.ClassesCodigos.Order(StringComparer.Ordinal).ToList()
                : Um(Criterios.ClasseNaoIdentificada),
            [Criterios.SituacaoCliente] = Um(os.SituacaoCliente),
        };

        foreach (var (criterio, opcoes) in contexto.RespostasPersonalizadas)
            respostas[criterio] = opcoes;

        return new FatosOcorrencia(respostas);
    }
}
