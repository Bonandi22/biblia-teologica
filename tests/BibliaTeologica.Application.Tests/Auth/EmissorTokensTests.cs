using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Auth;

public class EmissorTokensTests
{
    [Fact]
    public async Task EmitirAsync_GeraTokensEGuardaRefresh()
    {
        var uid = Guid.NewGuid();
        var identity = new Mock<IIdentityService>();
        var token = new Mock<ITokenService>();
        var store = new Mock<IRefreshTokenStore>();

        identity.Setup(i => i.ObterUsuarioAsync(uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false));
        token.Setup(t => t.GerarAccessToken(uid, "a@b.c", "Ana", It.IsAny<IEnumerable<string>>(), false)).Returns("acc");
        token.Setup(t => t.GerarRefreshToken()).Returns(("cru", "hash", DateTime.UtcNow.AddDays(14)));

        var r = await new EmissorTokens(identity.Object, token.Object, store.Object).EmitirAsync(uid, "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("acc");
        r.Tokens.Csrf.Should().NotBeNullOrWhiteSpace();
        store.Verify(s => s.GuardarAsync(uid, "hash", It.IsAny<DateTime>(), "ip", It.IsAny<CancellationToken>()), Times.Once);
    }
}
