using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BibliaTeologica.Domain.Slugs;

public static partial class Slug
{
    public static string From(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto para slug não pode ser vazio.", nameof(texto));

        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);

        foreach (var ch in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        return HifensRepetidos().Replace(sb.ToString(), "-").Trim('-');
    }

    [GeneratedRegex("-{2,}")]
    private static partial Regex HifensRepetidos();
}
