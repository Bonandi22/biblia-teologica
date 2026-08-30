using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;

namespace BibliaTeologica.Domain.Entities;

public class Livro
{
    private readonly List<string> _ideias = new();

    private Livro() { }

    private Livro(string nome, SecaoCanonica secao, int ordem, string quem, string porque,
        string paraQuem, string estrutura, string tema, IEnumerable<string> ideias, string versiculo)
    {
        Nome = nome;
        Slug = Slugs.Slug.From(nome);
        Secao = secao;
        Ordem = ordem;
        Quem = quem;
        Porque = porque;
        ParaQuem = paraQuem;
        Estrutura = estrutura;
        Tema = tema;
        _ideias.AddRange(ideias);
        Versiculo = versiculo;
        Publicado = true;
        CriadoEm = AtualizadoEm = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public SecaoCanonica Secao { get; private set; }
    public int Ordem { get; private set; }
    public string Quem { get; private set; } = null!;
    public string Porque { get; private set; } = null!;
    public string ParaQuem { get; private set; } = null!;
    public string Estrutura { get; private set; } = null!;
    public string Tema { get; private set; } = null!;
    public IReadOnlyList<string> Ideias => _ideias.AsReadOnly();
    public string Versiculo { get; private set; } = null!;
    public bool Publicado { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public Testamento Testamento => Secao.Testamento();

    public static Livro Create(string nome, SecaoCanonica secao, int ordem, string quem,
        string porque, string paraQuem, string estrutura, string tema,
        IEnumerable<string> ideias, string versiculo)
    {
        var ideiasValidas = (ideias ?? Enumerable.Empty<string>())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToList();

        Requer(nome, nameof(nome));
        if (!Enum.IsDefined(secao)) throw new DomainException("Seção canônica inválida.");
        if (ordem is < 1 or > 66) throw new DomainException("Ordem deve estar entre 1 e 66.");
        Requer(quem, nameof(quem));
        Requer(porque, nameof(porque));
        Requer(paraQuem, nameof(paraQuem));
        Requer(estrutura, nameof(estrutura));
        Requer(tema, nameof(tema));
        Requer(versiculo, nameof(versiculo));
        if (ideiasValidas.Count == 0)
            throw new DomainException("Livro precisa de ao menos uma ideia-chave.");

        return new Livro(nome, secao, ordem, quem, porque, paraQuem, estrutura, tema,
            ideiasValidas, versiculo);

        static void Requer(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new DomainException($"Campo obrigatório ausente: {campo}.");
        }
    }
}
