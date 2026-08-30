namespace BibliaTeologica.Application.Auth.Dtos;

public record UsuarioDto(string Email, string NomeExibicao, IReadOnlyList<string> Roles, bool DeveTrocarSenha);
