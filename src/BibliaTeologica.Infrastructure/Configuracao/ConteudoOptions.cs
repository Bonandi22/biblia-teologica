namespace BibliaTeologica.Infrastructure.Configuracao;

public class ConteudoOptions
{
    public const string Secao = "Conteudo";
    /// <summary>Relativa resolve contra AppContext.BaseDirectory.</summary>
    public string Pasta { get; set; } = "content";
    public bool IncluirRascunhos { get; set; } = false;
}
