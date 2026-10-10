using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Mapping;
using Farol.Domain.Enums;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Farol.Application.Services;

/// <summary>Dados do mapa operacional (seção 6). Só desenha o que tem coordenada; nada é inventado.</summary>
public class MapaService(IAppDbContext db, IUsuarioAtual usuario, EscopoMunicipio escopo, IProvedorRoteamento roteamento)
{
    public async Task<MapaDto> OperacoesAsync(int? municipioId, CancellationToken ct = default)
    {
        var municipios = (await escopo.ResolverAsync(municipioId, ct))?.ToArray();

        var ordens = await db.OrdensServico.AsNoTracking()
            .Where(o => (municipios == null || municipios.Contains(o.MunicipioId))
                        && o.Status != StatusOrdemServico.Concluida && o.Status != StatusOrdemServico.Cancelada)
            .Select(o => new
            {
                o.Id, o.Numero, o.Latitude, o.Longitude, o.Prioridade, o.Status, Tipo = o.TipoOcorrencia!.Nome,
                o.EnderecoCompleto, o.OrigemCoordenada, o.MunicipioId,
            })
            .ToListAsync(ct);

        var podeVerEquipes = usuario.Tem(Permissoes.LocalizacaoConsultar);
        var equipes = !podeVerEquipes ? [] : await db.Equipes.AsNoTracking()
            .Where(e => e.Ativa && e.Latitude != null && (municipios == null || municipios.Contains(e.MunicipioBaseId)))
            .Select(e => new
            {
                e.Id, e.Codigo, e.Nome, e.Status, e.Capacidade, e.Latitude, e.Longitude, e.LocalizacaoAtualizadaEm,
                e.OrigemLocalizacao, e.MunicipioBaseId, e.Ativa, Ativos = db.Despachos.Count(d => d.EquipeId == e.Id && d.Ativo),
            })
            .ToListAsync(ct);

        var subestacoes = await db.Subestacoes.AsNoTracking()
            .Where(s => s.Latitude != null && (municipios == null || municipios.Contains(s.MunicipioId)))
            .Select(s => new MapaSubestacaoDto(s.Id, s.Codigo, s.Nome, s.Latitude!.Value, s.Longitude!.Value, s.Demonstrativo))
            .ToListAsync(ct);

        var centro = municipios is { Length: 1 }
            ? await db.Municipios.Where(m => m.Id == municipios[0]).Select(m => new { m.Latitude, m.Longitude }).FirstOrDefaultAsync(ct)
            : null;

        return new MapaDto(
            ordens.Where(o => o.Latitude is not null).Select(o => new MapaOsDto(o.Id, o.Numero, o.Latitude!.Value, o.Longitude!.Value,
                o.Prioridade is null ? null : MapeamentoClassificacao.Resumo(o.Prioridade), o.Status, o.Tipo, o.EnderecoCompleto,
                o.Prioridade?.Critica ?? false, o.OrigemCoordenada, o.MunicipioId)).ToList(),
            equipes.Select(e => new MapaEquipeDto(e.Id, e.Codigo, e.Nome, e.Status,
                e.Status == StatusEquipe.Disponivel && e.Ativos < e.Capacidade, e.Latitude!.Value, e.Longitude!.Value,
                e.LocalizacaoAtualizadaEm, e.OrigemLocalizacao, e.MunicipioBaseId)).ToList(),
            subestacoes,
            ordens.Count(o => o.Latitude is null),
            centro?.Latitude, centro?.Longitude);
    }

    /// <summary>Rota entre dois pontos. Sem serviço de rotas, devolve só a distância em linha reta, sem tempo.</summary>
    public async Task<RotaDto> RotaAsync(double deLat, double deLng, double paraLat, double paraLng, CancellationToken ct = default)
    {
        var reta = new Coordenadas(deLat, deLng).DistanciaKm(new Coordenadas(paraLat, paraLng));
        if (roteamento.Disponivel)
        {
            var rota = await roteamento.CalcularAsync(deLat, deLng, paraLat, paraLng, ct);
            if (rota is not null)
                return new RotaDto(true, Math.Round(rota.DistanciaKm, 2), Math.Round(rota.TempoMin, 1), "ROTEAMENTO", rota.Geometria.ToList(), null);
        }
        return new RotaDto(false, Math.Round(reta, 2), null, "LINHA_RETA", [[deLat, deLng], [paraLat, paraLng]],
            "Distância em linha reta — sem estimativa de tempo (serviço de rotas não configurado ou indisponível).");
    }
}
