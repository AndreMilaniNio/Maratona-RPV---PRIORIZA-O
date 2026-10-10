using System.Globalization;
using System.Text.RegularExpressions;

namespace Farol.Domain.ValueObjects;

/// <summary>Par latitude/longitude em graus decimais, WGS84.</summary>
public readonly record struct Coordenadas(double Latitude, double Longitude)
{
    // Retângulo envolvente do território brasileiro, usado apenas para alerta.
    private const double BrasilLatMin = -33.8, BrasilLatMax = 5.3, BrasilLngMin = -74.0, BrasilLngMax = -34.7;

    public bool DentroDosLimites => Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;

    public bool DentroDoBrasil =>
        Latitude is >= BrasilLatMin and <= BrasilLatMax && Longitude is >= BrasilLngMin and <= BrasilLngMax;

    /// <summary>Distância em linha reta (haversine), em quilômetros.</summary>
    public double DistanciaKm(Coordenadas outra)
    {
        const double raioTerraKm = 6371.0088;
        var dLat = Radianos(outra.Latitude - Latitude);
        var dLng = Radianos(outra.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(Radianos(Latitude)) * Math.Cos(Radianos(outra.Latitude)) * Math.Pow(Math.Sin(dLng / 2), 2);
        return 2 * raioTerraKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double Radianos(double graus) => graus * Math.PI / 180;

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Latitude:0.######},{Longitude:0.######}");
}

/// <summary>
/// Interpreta coordenadas coladas pelo atendente: par decimal, link de mapa
/// ou graus/minutos/segundos (como 21°31'49.594"S 42°38'20.207"W).
/// </summary>
public static partial class InterpretadorCoordenadas
{
    public static bool TentarInterpretar(string? texto, out Coordenadas coordenadas)
    {
        coordenadas = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        var valor = Uri.UnescapeDataString(texto.Trim());

        var gms = Gms().Matches(valor);
        if (gms.Count == 2)
        {
            var a = DeGms(gms[0]);
            var b = DeGms(gms[1]);
            var aEhLatitude = gms[0].Groups["h"].Value.ToUpperInvariant() is "N" or "S";
            coordenadas = aEhLatitude ? new Coordenadas(a, b) : new Coordenadas(b, a);
            return true;
        }

        // Links de mapa: "@lat,lng", "q=lat,lng", "ll=lat,lng", "query=lat,lng", ou o par solto.
        var par = ParDecimal().Match(valor);
        if (!par.Success) return false;
        coordenadas = new Coordenadas(
            double.Parse(par.Groups["lat"].Value, CultureInfo.InvariantCulture),
            double.Parse(par.Groups["lng"].Value, CultureInfo.InvariantCulture));
        return true;
    }

    private static double DeGms(Match m)
    {
        var graus = double.Parse(m.Groups["g"].Value, CultureInfo.InvariantCulture);
        var minutos = m.Groups["m"].Success ? double.Parse(m.Groups["m"].Value, CultureInfo.InvariantCulture) : 0;
        var segundos = m.Groups["s"].Success ? double.Parse(m.Groups["s"].Value.Replace(',', '.'), CultureInfo.InvariantCulture) : 0;
        var decimais = graus + minutos / 60 + segundos / 3600;
        return m.Groups["h"].Value.ToUpperInvariant() is "S" or "W" or "O" ? -decimais : decimais;
    }

    [GeneratedRegex(@"(?<g>\d{1,3})\s*°\s*(?:(?<m>\d{1,2})\s*['′]\s*)?(?:(?<s>\d{1,2}(?:[.,]\d+)?)\s*(?:""|″|''))?\s*(?<h>[NSEWOnsewo])")]
    private static partial Regex Gms();

    [GeneratedRegex(@"(?<lat>[-+]?\d{1,2}(?:\.\d+)?)\s*[,;\s]\s*(?<lng>[-+]?\d{1,3}(?:\.\d+)?)")]
    private static partial Regex ParDecimal();
}
