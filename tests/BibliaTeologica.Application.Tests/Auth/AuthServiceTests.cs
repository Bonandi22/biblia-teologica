using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<ITokenService> _token = new();
    private readonly Mock<IRefreshTokenStore> _store = new();
    private readonly Mock<IEmailSender> _email = new();
    private readonly Mock<IEmissorTokens> _emissor = new();

    private AuthService Criar() => new(_identity.Object, _token.Object, _store.Object,
        _email.Object, _emissor.Object, NullLogger<AuthService>.Instance);

    private static readonly Guid Uid = Guid.NewGuid();

    private void ArranjarEmissorOk() =>
        _emissor.Setup(e => e.EmitirAsync(Uid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoLoginDto(EstadoLogin.Autenticado,
                Usuario: new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false),
                Tokens: new TokensGerados("access-jwt", DateTime.UtcNow.AddMinutes(15),
                    "refresh-cru", DateTime.UtcNow.AddDays(14), "csrf")));

    private void ArranjarEmissaoLocalOk()
    {
        _identity.Setup(i => i.ObterUsuarioAsync(Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false));
        _token.Setup(t => t.GerarAccessToken(Uid, "a@b.c", "Ana", It.IsAny<IEnumerable<string>>(), false))
            .Returns("access-jwt");
        _token.Setup(t => t.GerarRefreshToken())
            .Returns(("refresh-cru", "refresh-hash", DateTime.UtcNow.AddDays(14)));
    }

    [Fact]
    public async Task LoginAsync_EnviaOtpEDevolveDesafio_QuandoCredenciaisValidas()
    {
        _identity.Setup(i => i.VerificarSenhaAsync("a@b.c", "senha", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(true, false, Uid));
        _identity.Setup(i => i.GerarTokenEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("123456");
        _token.Setup(t => t.GerarDesafioLogin(Uid)).Returns("token-desafio");

        var r = await Criar().LoginAsync(new LoginDto("a@b.c", "senha"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Desafio2fa);
        r.TokenDesafio.Should().Be("token-desafio");
        _email.Verify(e => e.EnviarAsync("a@b.c", It.IsAny<string>(),
            It.Is<string>(c => c.Contains("123456")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DevolveBloqueado_QuandoLockout()
    {
        _identity.Setup(i => i.VerificarSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, true, null));

        (await Criar().LoginAsync(new LoginDto("a@b.c", "x"), null, default)).Estado
            .Should().Be(EstadoLogin.Bloqueado);
    }

    [Fact]
    public async Task LoginAsync_DevolveCredenciaisInvalidas_QuandoSenhaErrada()
    {
        _identity.Setup(i => i.VerificarSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, false, null));

        (await Criar().LoginAsync(new LoginDto("a@b.c", "x"), null, default)).Estado
            .Should().Be(EstadoLogin.CredenciaisInvalidas);
    }

    [Fact]
    public async Task Verificar2faAsync_EmiteTokens_QuandoCodigoCorreto()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.VerificarTokenEmailAsync(Uid, "123456", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ArranjarEmissorOk();

        var r = await Criar().Verificar2faAsync(new Verificar2faDto("td", "123456"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("access-jwt");
        _emissor.Verify(e => e.EmitirAsync(Uid, "ip", It.IsAny<CancellationToken>()), Times.Once);
        _identity.Verify(i => i.ResetarFalhasAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Verificar2faAsync_RegistaFalha_QuandoCodigoErrado()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.VerificarTokenEmailAsync(Uid, "000000", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var r = await Criar().Verificar2faAsync(new Verificar2faDto("td", "000000"), null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _identity.Verify(i => i.RegistarFalhaAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_RotacionaEEmite_QuandoRefreshValido()
    {
        _token.Setup(t => t.Hash("cru")).Returns("hash-antigo");
        _store.Setup(s => s.ObterAsync("hash-antigo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "hash-antigo", DateTime.UtcNow.AddDays(1), null));
        ArranjarEmissaoLocalOk();

        var r = await Criar().RefreshAsync("cru", "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        _store.Verify(s => s.RotacionarAsync("hash-antigo", Uid, "refresh-hash", It.IsAny<DateTime>(), "ip", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_RevogaCadeia_QuandoTokenJaRevogado()
    {
        _token.Setup(t => t.Hash("cru")).Returns("h");
        _store.Setup(s => s.ObterAsync("h", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "h", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMinutes(-1)));

        var r = await Criar().RefreshAsync("cru", null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _store.Verify(s => s.RevogarCadeiaAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistarMembroAsync_DelegaNoIdentity()
    {
        _identity.Setup(i => i.CriarMembroAsync("m@b.c", "Senha!Forte1", "Marta", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());

        (await Criar().RegistarMembroAsync(new RegistarMembroDto("m@b.c", "Senha!Forte1", "Marta"), default))
            .Sucesso.Should().BeTrue();
    }
}
