using Farol.Domain.Enums;

namespace Farol.Application.Prioritization;

/// <summary>Chave de ordenação de uma OS na fila.</summary>
public sealed record ChaveFila(
    Guid Id,
    int MunicipioId,
    int NivelPrecedencia,
    int RankPrioridade,
    TipoManutencao TipoManutencao,
    int Pontuacao,
    DateTimeOffset? ProximoPrazo,
    DateTimeOffset AbertaEm,
    string Numero);

/// <summary>
/// Política explícita de ordenação da fila (Key decision 3, seção 7.4):
/// precedência ↓, rank da prioridade ↑, corretiva antes de preventiva, pontuação ↓,
/// próximo prazo ↑, abertura ↑, número ↑. A posição é sempre calculada dentro da cidade.
/// </summary>
public sealed class PoliticaOrdenacao : IComparer<ChaveFila>
{
    public static readonly PoliticaOrdenacao Instancia = new();

    public static readonly string[] Descricao =
    [
        "1. Nível de precedência (regras de segurança da camada A), do maior para o menor.",
        "2. Código de prioridade, do mais urgente para o menos urgente.",
        "3. Em prioridade equivalente, manutenção corretiva antes da preventiva.",
        "4. Pontuação total, da maior para a menor.",
        "5. Próximo prazo, do mais próximo (ou vencido) para o mais distante.",
        "6. Data e hora de abertura, da mais antiga para a mais recente.",
        "7. Número da OS, em ordem crescente (desempate final determinístico).",
    ];

    public int Compare(ChaveFila? a, ChaveFila? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a is null) return 1;
        if (b is null) return -1;

        var c = b.NivelPrecedencia.CompareTo(a.NivelPrecedencia);
        if (c != 0) return c;
        c = a.RankPrioridade.CompareTo(b.RankPrioridade);
        if (c != 0) return c;
        c = Peso(a.TipoManutencao).CompareTo(Peso(b.TipoManutencao));
        if (c != 0) return c;
        c = b.Pontuacao.CompareTo(a.Pontuacao);
        if (c != 0) return c;
        c = (a.ProximoPrazo ?? DateTimeOffset.MaxValue).CompareTo(b.ProximoPrazo ?? DateTimeOffset.MaxValue);
        if (c != 0) return c;
        c = a.AbertaEm.CompareTo(b.AbertaEm);
        if (c != 0) return c;
        return string.CompareOrdinal(a.Numero, b.Numero);
    }

    private static int Peso(TipoManutencao t) => t == TipoManutencao.Corretiva ? 0 : 1;

    /// <summary>Posição (1..n) de cada OS dentro da própria cidade.</summary>
    public static Dictionary<Guid, int> Posicoes(IEnumerable<ChaveFila> chaves) =>
        chaves.GroupBy(c => c.MunicipioId)
            .SelectMany(g => g.Order(Instancia).Select((c, i) => (c.Id, Posicao: i + 1)))
            .ToDictionary(x => x.Id, x => x.Posicao);
}
