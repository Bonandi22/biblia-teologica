namespace BibliaTeologica.Application.Auth.Dtos;

public record ResultadoCadastro(string? TokenDesafio, IReadOnlyList<string> Erros);
