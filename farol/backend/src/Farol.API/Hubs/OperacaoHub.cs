using Farol.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Farol.API.Hubs;

/// <summary>Notificações em tempo real por cidade. A atualização periódica continua como reconciliação.</summary>
[Authorize]
public class OperacaoHub(IUsuarioAtual usuario) : Hub
{
    public const string GrupoTodas = "municipio-todas";
    public static string Grupo(int municipioId) => $"municipio-{municipioId}";

    /// <summary>Entra no grupo da cidade (nulo = todas). O escopo é verificado no servidor.</summary>
    public async Task Acompanhar(int? municipioId)
    {
        var permitidos = await usuario.MunicipiosPermitidosAsync(Context.ConnectionAborted);
        if (municipioId is null)
        {
            if (permitidos is not null) throw new HubException("Visão de todas as cidades não autorizada.");
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoTodas);
            return;
        }
        if (permitidos is not null && !permitidos.Contains(municipioId.Value)) throw new HubException("Município não autorizado.");
        await Groups.AddToGroupAsync(Context.ConnectionId, Grupo(municipioId.Value));
    }

    public Task Deixar(int? municipioId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, municipioId is null ? GrupoTodas : Grupo(municipioId.Value));
}

public class NotificadorFilaSignalR(IHubContext<OperacaoHub> hub) : INotificadorFila
{
    public Task FilaAlteradaAsync(int municipioId, string motivo, CancellationToken ct = default) =>
        hub.Clients.Groups(OperacaoHub.Grupo(municipioId), OperacaoHub.GrupoTodas)
            .SendAsync("filaAlterada", new { municipioId, motivo, em = DateTimeOffset.UtcNow }, ct);
}
