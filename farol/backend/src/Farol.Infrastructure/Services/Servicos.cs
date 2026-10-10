using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Farol.Infrastructure.Services;

public class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}

/// <summary>
/// Numeração por município/ano/tipo com upsert atômico: a linha do contador fica bloqueada até o fim
/// da transação, então duas criações simultâneas na mesma cidade nunca recebem o mesmo número.
/// </summary>
public class GeradorNumero(FarolDbContext db, IOptions<FarolOptions> opcoes) : IGeradorNumero
{
    public async Task<string> ProximoAsync(Municipio municipio, string tipo, int ano, CancellationToken ct = default)
    {
        var seq = await db.Database.SqlQuery<long>($"""
            INSERT INTO numeracao_sequencias (municipio_id, ano, tipo, ultimo)
            VALUES ({municipio.Id}, {ano}, {tipo}, 1)
            ON CONFLICT (municipio_id, ano, tipo) DO UPDATE SET ultimo = numeracao_sequencias.ultimo + 1
            RETURNING ultimo AS "Value"
            """).ToListAsync(ct);

        var cfg = opcoes.Value.Numeracao;
        var formato = tipo == "OS" ? cfg.FormatoOs : cfg.FormatoSolicitacao;
        return formato
            .Replace("{PREFIXO}", municipio.Prefixo)
            .Replace("{ANO}", ano.ToString(CultureInfo.InvariantCulture))
            .Replace("{TIPO}", tipo)
            .Replace("{SEQ}", seq[0].ToString(new string('0', cfg.DigitosSequencia), CultureInfo.InvariantCulture));
    }
}

public class CepOptions
{
    /// <summary>"Demonstrativo" (base local de CEPs) ou "Http" (provedor externo configurável).</summary>
    public string Provedor { get; set; } = "Demonstrativo";
    /// <summary>URL com o marcador {cep}. O JSON deve trazer logradouro, bairro e localidade (padrão comum a provedores de CEP).</summary>
    public string? UrlModelo { get; set; }
    public int TimeoutSegundos { get; set; } = 4;
}

/// <summary>Consulta a base local de CEPs cadastrados (demonstrativos ou carregados pela operação).</summary>
public class ProvedorCepLocal(FarolDbContext db) : IProvedorCep
{
    public async Task<ResultadoCep> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        var r = await db.Ceps.AsNoTracking().FirstOrDefaultAsync(c => c.Cep == cep, ct);
        if (r is not null)
        {
            var nome = await db.Municipios.Where(m => m.Id == r.MunicipioId).Select(m => m.Nome).FirstAsync(ct);
            return new ResultadoCep(true, cep, r.Logradouro, r.Bairro, r.MunicipioId, nome, r.Demonstrativo ? "BASE_DEMONSTRATIVA" : "BASE_LOCAL");
        }
        var unico = await db.Municipios.AsNoTracking().FirstOrDefaultAsync(m => m.CepUnico == cep, ct);
        return unico is null
            ? new ResultadoCep(false, cep, null, null, null, null, "BASE_LOCAL")
            : new ResultadoCep(true, cep, null, null, unico.Id, unico.Nome, "CEP_UNICO_MUNICIPIO");
    }
}

/// <summary>Provedor HTTP configurável. Falhas viram "não encontrado" e o atendente preenche à mão.</summary>
public class ProvedorCepHttp(HttpClient http, IOptions<CepOptions> opcoes, ProvedorCepLocal local, FarolDbContext db, ILogger<ProvedorCepHttp> logger) : IProvedorCep
{
    public async Task<ResultadoCep> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        var url = opcoes.Value.UrlModelo;
        if (string.IsNullOrWhiteSpace(url)) return await local.ConsultarAsync(cep, ct);
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(opcoes.Value.TimeoutSegundos));
            var json = await http.GetFromJsonAsync<JsonElement>(url.Replace("{cep}", cep), cts.Token);
            if (json.ValueKind != JsonValueKind.Object || json.TryGetProperty("erro", out _))
                return await local.ConsultarAsync(cep, ct);
            string? Campo(string nome) => json.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
            var cidade = Campo("localidade") ?? Campo("cidade");
            var municipio = cidade is null ? null : await db.Municipios.AsNoTracking().FirstOrDefaultAsync(m => m.Nome == cidade, ct);
            return new ResultadoCep(true, cep, Campo("logradouro"), Campo("bairro"), municipio?.Id, municipio?.Nome ?? cidade, "PROVEDOR_HTTP");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning("Consulta de CEP falhou ({Mensagem}); usando a base local.", ex.Message);
            return await local.ConsultarAsync(cep, ct);
        }
    }
}

public class RoteamentoOptions
{
    /// <summary>URL base de um serviço compatível com OSRM (ex.: http://localhost:5000). Vazio = desabilitado.</summary>
    public string? UrlBase { get; set; }
    public string Perfil { get; set; } = "driving";
    public int TimeoutSegundos { get; set; } = 5;
}

/// <summary>Rotas por serviço compatível com OSRM. Sem URL configurada, não há tempo estimado — nunca inventado.</summary>
public class ProvedorRoteamentoOsrm(HttpClient http, IOptions<RoteamentoOptions> opcoes, ILogger<ProvedorRoteamentoOsrm> logger) : IProvedorRoteamento
{
    public bool Disponivel => !string.IsNullOrWhiteSpace(opcoes.Value.UrlBase);

    public async Task<Rota?> CalcularAsync(double deLat, double deLng, double paraLat, double paraLng, CancellationToken ct = default)
    {
        if (!Disponivel) return null;
        var o = opcoes.Value;
        var url = string.Create(CultureInfo.InvariantCulture,
            $"{o.UrlBase!.TrimEnd('/')}/route/v1/{o.Perfil}/{deLng},{deLat};{paraLng},{paraLat}?overview=simplified&geometries=geojson");
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(o.TimeoutSegundos));
            var json = await http.GetFromJsonAsync<JsonElement>(url, cts.Token);
            if (json.GetProperty("code").GetString() != "Ok") return null;
            var rota = json.GetProperty("routes")[0];
            var geometria = rota.GetProperty("geometry").GetProperty("coordinates").EnumerateArray()
                .Select(p => new[] { p[1].GetDouble(), p[0].GetDouble() }).ToList();
            return new Rota(rota.GetProperty("distance").GetDouble() / 1000, rota.GetProperty("duration").GetDouble() / 60, geometria);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            logger.LogWarning("Serviço de rotas indisponível ({Mensagem}).", ex.Message);
            return null;
        }
    }
}
