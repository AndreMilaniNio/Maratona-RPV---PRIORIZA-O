namespace Farol.Domain.ValueObjects;

/// <summary>
/// Número do transformador: os 3 primeiros dígitos identificam a localidade e os demais
/// o equipamento na localidade. Sempre texto, para preservar zeros à esquerda.
/// </summary>
public sealed record NumeroTransformador
{
    public const int DigitosLocalidade = 3;
    public const int TamanhoMinimo = DigitosLocalidade + 1;

    public string Completo { get; }
    public string CodigoLocalidade { get; }
    public string NumeroLocal { get; }

    private NumeroTransformador(string completo)
    {
        Completo = completo;
        CodigoLocalidade = completo[..DigitosLocalidade];
        NumeroLocal = completo[DigitosLocalidade..];
    }

    /// <summary>Interpreta o número; devolve o motivo da recusa quando inválido.</summary>
    public static bool TentarCriar(string? texto, int tamanhoMaximo, out NumeroTransformador? numero, out string? erro)
    {
        numero = null;
        var valor = (texto ?? string.Empty).Trim();
        if (valor.Length == 0)
        {
            erro = "Informe o número do transformador.";
            return false;
        }
        if (!valor.All(char.IsAsciiDigit))
        {
            erro = "O número do transformador aceita somente dígitos.";
            return false;
        }
        if (valor.Length < TamanhoMinimo)
        {
            erro = $"O número do transformador precisa de ao menos {TamanhoMinimo} dígitos (3 da localidade e 1 do equipamento).";
            return false;
        }
        if (valor.Length > tamanhoMaximo)
        {
            erro = $"O número do transformador aceita no máximo {tamanhoMaximo} dígitos.";
            return false;
        }
        numero = new NumeroTransformador(valor);
        erro = null;
        return true;
    }

    public override string ToString() => Completo;
}
