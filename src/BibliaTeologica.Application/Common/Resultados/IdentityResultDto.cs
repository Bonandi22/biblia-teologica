namespace BibliaTeologica.Application.Common.Resultados;

public record IdentityResultDto(bool Sucesso, IReadOnlyList<string> Erros, bool Limite = false)
{
    public static IdentityResultDto Ok() => new(true, Array.Empty<string>());
    public static IdentityResultDto Falha(params string[] erros) => new(false, erros);
    /// <summary>Limite de envio de e-mail esgotado: o controller responde 429.</summary>
    public static IdentityResultDto LimiteEsgotado() => new(false, Array.Empty<string>(), true);
}
