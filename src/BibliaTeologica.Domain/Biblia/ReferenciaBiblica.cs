using System.Text.RegularExpressions;
using BibliaTeologica.Domain.Slugs;

namespace BibliaTeologica.Domain.Biblia;

/// <summary>
/// Lê referências no formato "Livro cap[:vers][–fim]" (travessão ou hífen) e devolve o slug do livro.
/// Reconhece os 66 livros do cânon protestante pelo slug do nome, mais apelidos fixos.
/// </summary>
public static partial class ReferenciaBiblica
{
    private static readonly IReadOnlyDictionary<string, string> Apelidos = new Dictionary<string, string>
    {
        ["cantares"] = "cantares-de-salomao",
        ["salmo"] = "salmos",
        ["atos"] = "atos-dos-apostolos",
    };

    private static readonly HashSet<string> SlugsCanonicos = new(StringComparer.Ordinal)
    {
        "genesis", "exodo", "levitico", "numeros", "deuteronomio", "josue", "juizes", "rute",
        "1-samuel", "2-samuel", "1-reis", "2-reis", "1-cronicas", "2-cronicas", "esdras", "neemias",
        "ester", "jo", "salmos", "proverbios", "eclesiastes", "cantares-de-salomao", "isaias",
        "jeremias", "lamentacoes", "ezequiel", "daniel", "oseias", "joel", "amos", "obadias", "jonas",
        "miqueias", "naum", "habacuque", "sofonias", "ageu", "zacarias", "malaquias", "mateus",
        "marcos", "lucas", "joao", "atos-dos-apostolos", "romanos", "1-corintios", "2-corintios",
        "galatas", "efesios", "filipenses", "colossenses", "1-tessalonicenses", "2-tessalonicenses",
        "1-timoteo", "2-timoteo", "tito", "filemom", "hebreus", "tiago", "1-pedro", "2-pedro",
        "1-joao", "2-joao", "3-joao", "judas", "apocalipse",
    };

    public static bool TentarLer(string texto, out string slugLivro)
    {
        slugLivro = string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var m = Formato().Match(texto.Trim());
        if (!m.Success)
            return false;

        if (!IntervaloCoerente(m))
            return false;

        var slug = Slug.From(m.Groups["livro"].Value);
        if (Apelidos.TryGetValue(slug, out var canonico))
            slug = canonico;

        if (!SlugsCanonicos.Contains(slug))
            return false;

        slugLivro = slug;
        return true;
    }

    // Rejeita intervalo invertido e capítulo final com ":" sem versículo inicial.
    private static bool IntervaloCoerente(Match m)
    {
        if (!int.TryParse(m.Groups["c1"].Value, out var c1)) return false;
        var temV1 = m.Groups["v1"].Success;
        var temC2 = m.Groups["c2"].Success;
        if (!m.Groups["f"].Success) return true;
        if (!int.TryParse(m.Groups["f"].Value, out var f)) return false;

        if (!temV1) return !temC2 && f > c1;                       // "36–37"
        if (!int.TryParse(m.Groups["v1"].Value, out var v1)) return false;
        if (!temC2) return f >= v1;                                // "2:22–25"
        if (!int.TryParse(m.Groups["c2"].Value, out var c2)) return false;
        return c2 > c1 || (c2 == c1 && f >= v1);                   // "52:13–53:12"
    }

    // Livro (pode começar com número) + cap[:vers] + "–fim" opcional (versículo, capítulo ou cap:vers).
    // Só dígitos ASCII, sem zero à esquerda: capítulo e versículo 0 não existem.
    [GeneratedRegex(@"^(?<livro>.+?)\s+(?<c1>[1-9][0-9]*)(?::(?<v1>[1-9][0-9]*))?(?:\s?[–-]\s?(?:(?<c2>[1-9][0-9]*):)?(?<f>[1-9][0-9]*))?$")]
    private static partial Regex Formato();
}
