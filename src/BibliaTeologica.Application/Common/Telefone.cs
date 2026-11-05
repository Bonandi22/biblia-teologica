using System.Text;

namespace BibliaTeologica.Application.Common;

public static class Telefone
{
    private const int MinimoDigitos = 8;
    private const int MaximoDigitos = 15;

    /// <summary>
    /// Vazio é válido e resulta em null. Aceita um "+" inicial e separadores (espaço, hífen, parênteses);
    /// exige de 8 a 15 dígitos.
    /// </summary>
    public static bool TentarNormalizar(string? valor, out string? normalizado)
    {
        normalizado = null;
        if (string.IsNullOrWhiteSpace(valor)) return true;

        var texto = valor.Trim();
        var mais = texto.StartsWith('+');
        var resto = mais ? texto[1..] : texto;

        var digitos = new StringBuilder();
        foreach (var c in resto)
        {
            if (c is >= '0' and <= '9') digitos.Append(c);
            else if (c is ' ' or '-' or '(' or ')') continue;
            else return false;
        }

        if (digitos.Length is < MinimoDigitos or > MaximoDigitos) return false;

        normalizado = (mais ? "+" : "") + digitos;
        return true;
    }
}
