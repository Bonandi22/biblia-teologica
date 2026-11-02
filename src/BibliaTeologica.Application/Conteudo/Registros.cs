namespace BibliaTeologica.Application.Conteudo;

public sealed record LivroCanonico(string Slug, int Ordem);

public sealed class RegistroObra
{
    public string? Id { get; set; }
    public string? Autor { get; set; }
    public string? Titulo { get; set; }
    public string? Detalhes { get; set; }
    public string? Url { get; set; }
}

public sealed class RegistroNota
{
    public int Numero { get; set; }
    public string? Obra { get; set; }
    public string? Local { get; set; }
}

public sealed class RegistroItem
{
    public string? Nome { get; set; }
    public string? Quando { get; set; }
    public int? Ano { get; set; }
    public string? Texto { get; set; }
    public string? SaberMais { get; set; }
    public string? Estudo { get; set; }
    private List<string> _Referencias = new();
    public List<string> Referencias { get => _Referencias; set => _Referencias = value ?? new(); }
}

public sealed class RegistroBloco
{
    public string? Explicacao { get; set; }
    private List<RegistroItem> _Personagens = new();
    public List<RegistroItem> Personagens { get => _Personagens; set => _Personagens = value ?? new(); }
    private List<RegistroItem> _Acontecimentos = new();
    public List<RegistroItem> Acontecimentos { get => _Acontecimentos; set => _Acontecimentos = value ?? new(); }
    private List<RegistroNota> _Notas = new();
    public List<RegistroNota> Notas { get => _Notas; set => _Notas = value ?? new(); }
}

public sealed class RegistroProfecia
{
    public string? Referencia { get; set; }
    public string? Anuncia { get; set; }
    /// <summary>cumpridaEmCristo | cumpridaNaHistoria | promessaAguardada</summary>
    public string? Categoria { get; set; }
    private List<string> _Cumprimento = new();
    public List<string> Cumprimento { get => _Cumprimento; set => _Cumprimento = value ?? new(); }
    public string? FatoHistorico { get; set; }
    private List<RegistroNota> _Notas = new();
    public List<RegistroNota> Notas { get => _Notas; set => _Notas = value ?? new(); }
}

public sealed class RegistroCitacao
{
    public string? Trecho { get; set; }
    private List<string> _CitadoEm = new();
    public List<string> CitadoEm { get => _CitadoEm; set => _CitadoEm = value ?? new(); }
    public string? Comentario { get; set; }
    private List<RegistroNota> _Notas = new();
    public List<RegistroNota> Notas { get => _Notas; set => _Notas = value ?? new(); }
}

public sealed class RegistroLivro
{
    /// <summary>Caminho do arquivo de origem, usado nas mensagens de erro.</summary>
    public string Arquivo { get; set; } = string.Empty;
    /// <summary>rascunho | revisado</summary>
    public string Estado { get; set; } = string.Empty;
    public string? Livro { get; set; }
    public string? Resumo { get; set; }
    public string? Quem { get; set; }
    public string? ParaQuem { get; set; }
    public string? Porque { get; set; }
    public RegistroBloco? Panorama { get; set; }
    public RegistroBloco? Contexto { get; set; }
    public RegistroBloco? Mundo { get; set; }
    private List<RegistroProfecia> _Profecias = new();
    public List<RegistroProfecia> Profecias { get => _Profecias; set => _Profecias = value ?? new(); }
    private List<RegistroCitacao> _Citacoes = new();
    public List<RegistroCitacao> Citacoes { get => _Citacoes; set => _Citacoes = value ?? new(); }
}

public sealed class RegistroEstudo
{
    /// <summary>Caminho do arquivo de origem; o nome do arquivo (sem extensão) é o slug do estudo.</summary>
    public string Arquivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public string? Resumo { get; set; }
    private List<string> _Livros = new();
    public List<string> Livros { get => _Livros; set => _Livros = value ?? new(); }
    private List<string> _Relacionados = new();
    public List<string> Relacionados { get => _Relacionados; set => _Relacionados = value ?? new(); }
    private List<RegistroNota> _Notas = new();
    public List<RegistroNota> Notas { get => _Notas; set => _Notas = value ?? new(); }
    public string? CorpoMarkdown { get; set; }
}

public sealed record ConjuntoRegistros(
    IReadOnlyList<RegistroObra> Obras,
    IReadOnlyList<RegistroLivro> Livros,
    IReadOnlyList<RegistroEstudo> Estudos);
