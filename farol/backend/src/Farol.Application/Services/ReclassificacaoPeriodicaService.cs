using Farol.Application.Abstractions;
using Farol.Application.Prioritization;
using Farol.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Farol.Application.Services;

/// <summary>
/// Reclassificação automática (seção 7.5): aplica versões cuja vigência chegou e recalcula os critérios
/// que dependem do tempo (espera, proximidade do prazo). Só grava quando algo muda.
/// </summary>
public class ReclassificacaoPeriodicaService(
    IAppDbContext db,
    IRelogio relogio,
    ServicoClassificacao classificacao,
    PontuacaoService pontuacao,
    INotificadorFila notificador,
    ILogger<ReclassificacaoPeriodicaService> logger)
{
    public async Task<(int VersoesAplicadas, int Reclassificadas)> ExecutarAsync(CancellationToken ct = default)
    {
        var agora = relogio.Agora;

        var pendentes = await db.VersoesPontuacao
            .Where(v => v.Status == StatusVersaoPontuacao.Publicada && v.Aplicacao == AplicacaoVersao.ReclassificarAbertas
                        && v.VigenciaInicio <= agora && v.ReclassificacaoAplicadaEm == null)
            .OrderBy(v => v.VigenciaInicio).ToListAsync(ct);
        foreach (var v in pendentes)
        {
            var n = await pontuacao.AplicarAsync(v, ct);
            logger.LogInformation("Versão {Numero} de pontuação aplicada a {Quantidade} OS abertas.", v.Numero, n);
        }

        var abertas = await db.OrdensServico
            .Include(o => o.CondicoesSeguranca).Include(o => o.Classes).Include(o => o.RecursosNecessarios).Include(o => o.RespostasPersonalizadas)
            .Where(o => o.Status != StatusOrdemServico.Concluida && o.Status != StatusOrdemServico.Cancelada)
            .AsSplitQuery()
            .ToListAsync(ct);

        var configs = new Dictionary<(int, int), ConfiguracaoClassificacao>();
        var alteradas = new HashSet<int>();
        var total = 0;
        foreach (var os in abertas)
        {
            var chave = (os.VersaoPontuacaoId, os.MunicipioId);
            if (!configs.TryGetValue(chave, out var config))
                configs[chave] = config = await classificacao.CarregarAsync(os.VersaoPontuacaoId, os.MunicipioId, ct);
            var resultado = await classificacao.ReclassificarAsync(os, MotivoClassificacao.Tempo, somenteSeMudou: true, config: config, ct: ct);
            if (resultado is null) continue;
            total++;
            alteradas.Add(os.MunicipioId);
        }

        foreach (var m in alteradas) await notificador.FilaAlteradaAsync(m, "reclassificacao-automatica", ct);
        return (pendentes.Count, total);
    }
}
