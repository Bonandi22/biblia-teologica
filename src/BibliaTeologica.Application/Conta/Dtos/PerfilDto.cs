namespace BibliaTeologica.Application.Conta.Dtos;

public record PerfilDto(string Email, string NomeExibicao, IReadOnlyList<string> Roles, DateTime CriadoEm, string? Telefone);
