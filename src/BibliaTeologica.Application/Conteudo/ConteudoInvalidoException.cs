namespace BibliaTeologica.Application.Conteudo;

/// <summary>Arquivo de conteúdo ilegível ou inválido; cada erro traz o nome do arquivo.</summary>
public class ConteudoInvalidoException : Exception
{
    public IReadOnlyList<string> Erros { get; }

    public ConteudoInvalidoException(IReadOnlyList<string> erros)
        : base("Conteúdo inválido: " + string.Join(" | ", erros))
    {
        Erros = erros;
    }

    public ConteudoInvalidoException(string erro) : this(new[] { erro }) { }
}
