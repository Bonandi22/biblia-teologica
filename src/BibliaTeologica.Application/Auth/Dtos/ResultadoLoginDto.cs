namespace BibliaTeologica.Application.Auth.Dtos;

public enum EstadoLogin { CredenciaisInvalidas, Bloqueado, Desafio2fa, Autenticado }

public record ResultadoLoginDto(
    EstadoLogin Estado,
    string? TokenDesafio = null,
    UsuarioDto? Usuario = null,
    TokensGerados? Tokens = null,
    IReadOnlyList<string>? Erros = null);
