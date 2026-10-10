using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Enums;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Services;

public sealed record CriterioDuplicidade(
    int MunicipioId,
    double? Latitude,
    double? Longitude,
    string? TransformadorNumero,
    IReadOnlyCollection<string> Ucs,
    int? ConjuntoId,
    int? TipoOcorrenciaId,
    Guid? IgnorarOsId = null);

/// <summary>
/// Sugere possíveis duplicidades (seção 13). Nunca une sozinho, e mesma rua não basta.
/// </summary>
public class DuplicidadeService(IAppDbContext db, IRelogio relogio, IOptions<FarolOptions> opcoes)
{
    public async Task<List<DuplicidadeDto>> SugerirAsync(CriterioDuplicidade c, CancellationToken ct = default)
    {
        var cfg = opcoes.Value.Duplicidade;
        var desde = relogio.Agora.AddHours(-cfg.JanelaHoras);

        var candidatas = await db.OrdensServico.AsNoTracking()
            .Where(o => o.MunicipioId == c.MunicipioId
                        && o.AbertaEm >= desde
                        && o.Status != StatusOrdemServico.Concluida && o.Status != StatusOrdemServico.Cancelada
                        && o.Id != c.IgnorarOsId)
            .Select(o => new
            {
                o.Id, o.Numero, o.Status, Tipo = o.TipoOcorrencia!.Nome, o.TipoOcorrenciaId, o.EnderecoCompleto,
                o.Latitude, o.Longitude, o.TransformadorNumero, o.ConjuntoId, o.AbertaEm,
            })
            .ToListAsync(ct);
        if (candidatas.Count == 0) return [];

        var ids = candidatas.Select(x => x.Id).ToList();
        var ucsPorOs = (await db.Solicitacoes.AsNoTracking()
                .Where(s => ids.Contains(s.OrdemServicoId))
                .SelectMany(s => s.Ucs.Select(u => new { s.OrdemServicoId, u.Numero }))
                .ToListAsync(ct))
            .GroupBy(x => x.OrdemServicoId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Numero).ToHashSet());

        var resultado = new List<DuplicidadeDto>();
        foreach (var o in candidatas)
        {
            var motivos = new List<string>();
            double? distanciaM = null;
            if (c.Latitude is { } lat && c.Longitude is { } lng && o.Latitude is { } olat && o.Longitude is { } olng)
            {
                distanciaM = new Coordenadas(lat, lng).DistanciaKm(new Coordenadas(olat, olng)) * 1000;
                if (distanciaM <= cfg.RaioMetros) motivos.Add($"A {distanciaM:0} m de distância");
            }
            if (c.TransformadorNumero is not null && c.TransformadorNumero == o.TransformadorNumero)
                motivos.Add($"Mesmo transformador ({o.TransformadorNumero})");
            if (ucsPorOs.TryGetValue(o.Id, out var ucs) && c.Ucs.Any(ucs.Contains))
                motivos.Add("Mesma UC");
            if (c.ConjuntoId is not null && c.ConjuntoId == o.ConjuntoId && c.TipoOcorrenciaId == o.TipoOcorrenciaId)
                motivos.Add("Mesmo conjunto elétrico e mesmo tipo de ocorrência");

            if (motivos.Count > 0)
                resultado.Add(new DuplicidadeDto(o.Id, o.Numero, o.Status.ToString(), o.Tipo, o.EnderecoCompleto, distanciaM, motivos, o.AbertaEm));
        }

        return resultado.OrderByDescending(d => d.Motivos.Count).ThenBy(d => d.DistanciaM ?? double.MaxValue).ToList();
    }
}
