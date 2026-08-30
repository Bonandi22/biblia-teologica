namespace BibliaTeologica.Application.Common.Resultados;

public record IdentityResultDto(bool Sucesso, IReadOnlyList<string> Erros)
{
    public static IdentityResultDto Ok() => new(true, Array.Empty<string>());
    public static IdentityResultDto Falha(params string[] erros) => new(false, erros);
}
