using System.IdentityModel.Tokens.Jwt;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public class TokenServiceTests
{
    private static TokenService Criar()
    {
        var opts = Options.Create(new JwtOptions
        {
            Issuer = "BT", Audience = "BT",
            SigningKey = "chave-de-teste-com-pelo-menos-32-caracteres-!!",
            AccessTokenMinutes = 15, RefreshTokenDays = 14
        });
        return new TokenService(opts, DataProtectionProvider.Create("BibliaTeologica.Tests"));
    }

    [Fact]
    public void GerarAccessToken_IncluiSubEmailRolesEDeveTrocarSenha()
    {
        var svc = Criar();
        var id = Guid.NewGuid();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(
            svc.GerarAccessToken(id, "a@b.c", "Ana", new[] { "Admin", "Membro" }, deveTrocarSenha: true));

        jwt.Subject.Should().Be(id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "a@b.c");
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().BeEquivalentTo("Admin", "Membro");
        jwt.Claims.Should().Contain(c => c.Type == "deve_trocar_senha" && c.Value == "true");
    }

    [Fact]
    public void GerarRefreshToken_ProduzHashEExpiracaoNoFuturo()
    {
        var (cru, hash, expira) = Criar().GerarRefreshToken();
        cru.Should().NotBeNullOrWhiteSpace();
        hash.Should().HaveLength(64);
        expira.Should().BeAfter(DateTime.UtcNow.AddDays(13));
    }

    [Fact]
    public void Hash_EhDeterministico()
    {
        var svc = Criar();
        svc.Hash("abc").Should().Be(svc.Hash("abc"));
        svc.Hash("abc").Should().NotBe(svc.Hash("abd"));
    }

    [Fact]
    public void DesafioLogin_RoundTrip()
    {
        var svc = Criar();
        var id = Guid.NewGuid();
        svc.ValidarDesafioLogin(svc.GerarDesafioLogin(id)).Should().Be(id);
    }

    [Fact]
    public void ValidarDesafioLogin_DevolveNull_QuandoTokenLixo()
    {
        Criar().ValidarDesafioLogin("nao-e-um-token").Should().BeNull();
    }
}
