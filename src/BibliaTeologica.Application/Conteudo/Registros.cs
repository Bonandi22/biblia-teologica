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
    public List<string> Referencias { get; set; } = new();
}

public sealed class RegistroBloco
{
    public string? Explicacao { get; set; }
    public List<RegistroItem> Personagens { get; set; } = new();
    public List<RegistroItem> Acontecimentos { get; set; } = new();
    public List<RegistroNota> Notas { get; set; } = new();
}

public sealed class RegistroProfecia
{
    public string? Referencia { get; set; }
    public string? Anuncia { get; set; }
    /// <summary>cumpridaEmCristo | cumpridaNaHistoria | promessaAguardada</summary>
    public string? Categoria { get; set; }
    public List<string> Cumprimento { get; set; } = new();
    public string? FatoHistorico { get; set; }
    public List<RegistroNota> Notas { get; set; } = new();
}

public sealed class RegistroCitacao
{
    public string? Trecho { get; set; }
    public List<string> CitadoEm { get; set; } = new();
    public string? Comentario { get; set; }
    public List<RegistroNota> Notas { get; set; } = new();
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
    public List<RegistroProfecia> Profecias { get; set; } = new();
    public List<RegistroCitacao> Citacoes { get; set; } = new();
}

public sealed class RegistroEstudo
{
    /// <summary>Caminho do arquivo de origem; o nome do arquivo (sem extensão) é o slug do estudo.</summary>
    public string Arquivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public string? Resumo { get; set; }
    public List<string> Livros { get; set; } = new();
    public List<string> Relacionados { get; set; } = new();
    public List<RegistroNota> Notas { get; set; } = new();
    public string? CorpoMarkdown { get; set; }
}

public sealed record ConjuntoRegistros(
    IReadOnlyList<RegistroObra> Obras,
    IReadOnlyList<RegistroLivro> Livros,
    IReadOnlyList<RegistroEstudo> Estudos);
