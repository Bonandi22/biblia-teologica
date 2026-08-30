namespace BibliaTeologica.Infrastructure.Configuracao;

public class JwtOptions
{
    public const string Secao = "Jwt";
    public string Issuer { get; set; } = "BibliaTeologica";
    public string Audience { get; set; } = "BibliaTeologica";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}
