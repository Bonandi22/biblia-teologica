using System.Text.RegularExpressions;
using BibliaTeologica.Domain.Slugs;

namespace BibliaTeologica.Domain.Biblia;

/// <summary>
/// Lê referências no formato "Livro cap[:vers][–fim]" (travessão ou hífen) e devolve o slug do livro.
/// Reconhece os 66 livros do cânon protestante pelo slug do nome, mais dois apelidos fixos.
/// </summary>
public static partial class ReferenciaBiblica
{
    private static readonly IReadOnlyDictionary<string, string> Apelidos = new Dictionary<string, string>
    {
        ["cantares"] = "cantares-de-salomao",
        ["salmo"] = "salmos",
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

        var slug = Slug.From(m.Groups["livro"].Value);
        if (Apelidos.TryGetValue(slug, out var canonico))
            slug = canonico;

        if (!SlugsCanonicos.Contains(slug))
            return false;

        slugLivro = slug;
        return true;
    }

    // Livro (pode começar com número) + capítulo, opcionalmente ":versículo", opcionalmente "–fim".
    [GeneratedRegex(@"^(?<livro>.+?)\s+\d+(?::\d+)?(?:\s?[–-]\s?\d+)?$")]
    private static partial Regex Formato();
}
