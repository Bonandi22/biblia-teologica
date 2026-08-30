using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BibliaTeologica.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opts;
    private readonly IDataProtector _protetorDesafio;

    public TokenService(IOptions<JwtOptions> opts, IDataProtectionProvider dp)
    {
        _opts = opts.Value;
        _protetorDesafio = dp.CreateProtector("login-2fa");
    }

    public string GerarAccessToken(Guid userId, string email, string nome,
        IEnumerable<string> roles, bool deveTrocarSenha)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("email", email),
            new("name", nome),
            new("deve_trocar_senha", deveTrocarSenha ? "true" : "false"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opts.AccessTokenMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string tokenCru, string hash, DateTime expiraEm) GerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var cru = Base64UrlEncoder.Encode(bytes);
        return (cru, Hash(cru), DateTime.UtcNow.AddDays(_opts.RefreshTokenDays));
    }

    public string Hash(string tokenCru)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenCru));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string GerarDesafioLogin(Guid userId)
    {
        var validade = DateTime.UtcNow.AddMinutes(5).Ticks;
        return _protetorDesafio.Protect($"{userId:D}|{validade}");
    }

    public Guid? ValidarDesafioLogin(string token)
    {
        try
        {
            var partes = _protetorDesafio.Unprotect(token).Split('|');
            if (partes.Length != 2) return null;
            if (new DateTime(long.Parse(partes[1]), DateTimeKind.Utc) < DateTime.UtcNow) return null;
            return Guid.Parse(partes[0]);
        }
        catch
        {
            return null; // token adulterado / expirado / de outra purpose
        }
    }
}
