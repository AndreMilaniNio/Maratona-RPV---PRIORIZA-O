using Farol.Domain.Enums;

namespace Farol.Domain.Rules;

/// <summary>
/// Transições válidas do ciclo de vida da OS (seção 12). Transições feitas pelo despacho
/// (designação, aceite, início, conclusão, remoção de equipe) usam <see cref="PorDespacho"/>;
/// as manuais, pelo endpoint de status, usam <see cref="Manuais"/>.
/// </summary>
public static class MaquinaEstadosOrdemServico
{
    public static readonly IReadOnlySet<StatusOrdemServico> Despachaveis = new HashSet<StatusOrdemServico>
    {
        StatusOrdemServico.Aberta, StatusOrdemServico.EmTriagem, StatusOrdemServico.AguardandoDespacho,
    };

    public static readonly IReadOnlySet<StatusOrdemServico> Encerrados = new HashSet<StatusOrdemServico>
    {
        StatusOrdemServico.Concluida, StatusOrdemServico.Cancelada,
    };

    /// <summary>Status em que existe um despacho ativo.</summary>
    public static readonly IReadOnlySet<StatusOrdemServico> ComEquipe = new HashSet<StatusOrdemServico>
    {
        StatusOrdemServico.EquipeDesignada, StatusOrdemServico.EquipeACaminho,
        StatusOrdemServico.EmExecucao, StatusOrdemServico.AguardandoRecurso,
    };

    public static readonly IReadOnlyDictionary<StatusOrdemServico, StatusOrdemServico[]> Manuais =
        new Dictionary<StatusOrdemServico, StatusOrdemServico[]>
        {
            [StatusOrdemServico.Aberta] = [StatusOrdemServico.EmTriagem, StatusOrdemServico.AguardandoDespacho, StatusOrdemServico.Cancelada],
            [StatusOrdemServico.EmTriagem] = [StatusOrdemServico.AguardandoDespacho, StatusOrdemServico.Cancelada],
            [StatusOrdemServico.AguardandoDespacho] = [StatusOrdemServico.Suspensa, StatusOrdemServico.Cancelada, StatusOrdemServico.EmTriagem],
            [StatusOrdemServico.Suspensa] = [StatusOrdemServico.AguardandoDespacho, StatusOrdemServico.Cancelada],
            [StatusOrdemServico.EmExecucao] = [StatusOrdemServico.AguardandoRecurso],
            [StatusOrdemServico.AguardandoRecurso] = [StatusOrdemServico.EmExecucao],
            [StatusOrdemServico.Concluida] = [StatusOrdemServico.AguardandoDespacho],
        };

    public static bool PodeTransitarManualmente(StatusOrdemServico de, StatusOrdemServico para) =>
        Manuais.TryGetValue(de, out var destinos) && destinos.Contains(para);

    /// <summary>Transições que exigem justificativa obrigatória.</summary>
    public static bool ExigeJustificativa(StatusOrdemServico de, StatusOrdemServico para) =>
        para is StatusOrdemServico.Cancelada or StatusOrdemServico.Suspensa ||
        de == StatusOrdemServico.Concluida;

    /// <summary>Reabrir uma OS concluída é uma ação autorizada à parte (permissão os.reabrir).</summary>
    public static bool EhReabertura(StatusOrdemServico de, StatusOrdemServico para) =>
        de == StatusOrdemServico.Concluida && para == StatusOrdemServico.AguardandoDespacho;

    public static class PorDespacho
    {
        public static bool PodeAceitar(StatusOrdemServico s) => s == StatusOrdemServico.EquipeDesignada;
        public static bool PodeIniciar(StatusOrdemServico s) => s == StatusOrdemServico.EquipeACaminho;
        public static bool PodeConcluir(StatusOrdemServico s) => s is StatusOrdemServico.EmExecucao or StatusOrdemServico.AguardandoRecurso;
        public static bool PodeRemoverEquipe(StatusOrdemServico s) => s is StatusOrdemServico.EquipeDesignada or StatusOrdemServico.EquipeACaminho;
    }

    /// <summary>Qual prazo está correndo em cada status (decisão "próximo prazo" do design).</summary>
    public static TipoPrazo? PrazoCorrente(StatusOrdemServico status) => status switch
    {
        StatusOrdemServico.Aberta or StatusOrdemServico.EmTriagem => TipoPrazo.Triagem,
        StatusOrdemServico.AguardandoDespacho or StatusOrdemServico.Suspensa => TipoPrazo.Despacho,
        StatusOrdemServico.EquipeDesignada or StatusOrdemServico.EquipeACaminho => TipoPrazo.Inicio,
        StatusOrdemServico.EmExecucao or StatusOrdemServico.AguardandoRecurso => TipoPrazo.Conclusao,
        _ => null,
    };
}
