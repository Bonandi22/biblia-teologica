namespace BibliaTeologica.Application.Auth.Dtos;

public record TokensGerados(
    string AccessToken, DateTime AccessExpiraEm,
    string RefreshToken, DateTime RefreshExpiraEm,
    string Csrf);
