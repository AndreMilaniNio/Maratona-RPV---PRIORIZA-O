using Farol.Application.Common;
using Farol.Application.Services;
using Microsoft.Extensions.Options;

namespace Farol.API.Extensions;

/// <summary>Dispara a reclassificação automática no intervalo configurado (padrão: 60 s).</summary>
public class ReclassificacaoHostedService(IServiceScopeFactory escopos, IOptions<FarolOptions> opcoes, ILogger<ReclassificacaoHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = opcoes.Value.Reclassificacao;
        if (!cfg.Habilitada) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(10, cfg.IntervaloSegundos)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var escopo = escopos.CreateScope();
                var (versoes, os) = await escopo.ServiceProvider.GetRequiredService<ReclassificacaoPeriodicaService>().ExecutarAsync(stoppingToken);
                if (versoes + os > 0) logger.LogInformation("Reclassificação automática: {Versoes} versão(ões) aplicada(s), {Os} OS reclassificada(s).", versoes, os);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha na reclassificação automática; nova tentativa no próximo ciclo.");
            }
        }
    }
}
