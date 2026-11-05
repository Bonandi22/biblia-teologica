using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using Microsoft.Extensions.Logging;
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

    private readonly Mock<IFilaEmail> _fila = new();
    private readonly Mock<ILimiteEnvio> _limite = new();

    private AuthService Criar() => new(_identity.Object, _token.Object, _store.Object,
        _email.Object, _emissor.Object, _fila.Object, _limite.Object, Mock.Of<IConfiguracaoSite>(), NullLogger<AuthService>.Instance);

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

    private sealed class LoggerCapturador : ILogger<AuthService>
    {
        public List<(LogLevel Nivel, string Mensagem)> Registros { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Registros.Add((logLevel, formatter(state, exception)));
    }

    private void ArranjarSenhaOk()
    {
        _identity.Setup(i => i.VerificarSenhaAsync("a@b.c", "senha", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(true, false, Uid));
        _identity.Setup(i => i.GerarTokenEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("123456");
        _token.Setup(t => t.GerarDesafioLogin(Uid)).Returns("token-desafio");
    }

    [Fact]
    public async Task LoginAsync_DevolveFalhaEnvioELogaSemCodigo_QuandoEnvioLanca()
    {
        ArranjarSenhaOk();
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(true);
        _email.Setup(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp fora"));
        var log = new LoggerCapturador();
        var svc = new AuthService(_identity.Object, _token.Object, _store.Object, _email.Object,
            _emissor.Object, _fila.Object, _limite.Object, Mock.Of<IConfiguracaoSite>(), log);

        var r = await svc.LoginAsync(new LoginDto("a@b.c", "senha"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.FalhaEnvio);
        r.TokenDesafio.Should().BeNull();
        log.Registros.Should().ContainSingle(x => x.Nivel == LogLevel.Error);
        log.Registros.Should().OnlyContain(x => !x.Mensagem.Contains("123456"));
    }

    [Fact]
    public async Task LoginAsync_DevolveLimiteEnvioSemEnviar_QuandoLimiteEsgotado()
    {
        ArranjarSenhaOk();
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(false);

        var r = await Criar().LoginAsync(new LoginDto("a@b.c", "senha"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.LimiteEnvio);
        _email.Verify(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_NaoConsomeLimite_QuandoSenhaErrada()
    {
        _identity.Setup(i => i.VerificarSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, false, null));

        await Criar().LoginAsync(new LoginDto("a@b.c", "x"), null, default);

        _limite.Verify(l => l.TentarConsumir(It.IsAny<string>(), It.IsAny<FinalidadeEnvio>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarCodigoAsync_Enfileira_QuandoContaExisteELimiteLivre()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.ObterEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("a@b.c");
        _identity.Setup(i => i.EstaBloqueadoAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _identity.Setup(i => i.GerarTokenEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("654321");
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(true);

        (await Criar().ReenviarCodigoAsync("td", default)).Should().Be(ResultadoReenvio.Aceito);

        _fila.Verify(f => f.Enfileirar("a@b.c", It.IsAny<string>(), It.Is<string>(c => c.Contains("654321"))), Times.Once);
    }

    [Fact]
    public async Task ReenviarCodigoAsync_UsaChaveDoDesafio_QuandoContaNaoExiste()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.ObterEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _limite.Setup(l => l.TentarConsumir($"desafio:{Uid}", FinalidadeEnvio.Login)).Returns(true);

        (await Criar().ReenviarCodigoAsync("td", default)).Should().Be(ResultadoReenvio.Aceito);

        _fila.Verify(f => f.Enfileirar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarCodigoAsync_NaoEnfileira_QuandoContaBloqueada()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.ObterEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("a@b.c");
        _identity.Setup(i => i.EstaBloqueadoAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(true);

        (await Criar().ReenviarCodigoAsync("td", default)).Should().Be(ResultadoReenvio.Aceito);

        _fila.Verify(f => f.Enfileirar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarCodigoAsync_DevolveLimite_QuandoLimiteEsgotado()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.ObterEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("a@b.c");
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(false);

        (await Criar().ReenviarCodigoAsync("td", default)).Should().Be(ResultadoReenvio.Limite);
    }

    [Fact]
    public async Task ReenviarCodigoAsync_DevolveDesafioInvalido_QuandoTokenInvalido()
    {
        _token.Setup(t => t.ValidarDesafioLogin("ruim")).Returns((Guid?)null);

        (await Criar().ReenviarCodigoAsync("ruim", default)).Should().Be(ResultadoReenvio.DesafioInvalido);
    }

    [Fact]
    public async Task LoginAsync_EnviaOtpEDevolveDesafio_QuandoCredenciaisValidas()
    {
        _limite.Setup(l => l.TentarConsumir("a@b.c", FinalidadeEnvio.Login)).Returns(true);
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
    public async Task RefreshAsync_RevogaCadeia_QuandoTokenRotacionadoEReutilizado()
    {
        _token.Setup(t => t.Hash("cru")).Returns("h");
        _store.Setup(s => s.ObterAsync("h", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "h", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMinutes(-1), Rotacionado: true));

        var r = await Criar().RefreshAsync("cru", null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _store.Verify(s => s.RevogarCadeiaAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_NaoRevogaCadeia_QuandoTokenFoiEncerradoSemRotacao()
    {
        _token.Setup(t => t.Hash("cru")).Returns("h");
        _store.Setup(s => s.ObterAsync("h", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "h", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMinutes(-1), Rotacionado: false));

        var r = await Criar().RefreshAsync("cru", null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _store.Verify(s => s.RevogarCadeiaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static RegistarMembroDto Cadastro(string email = "m@b.c", bool termo = true,
        string? telefone = null, string? site = null)
        => new(email, "Senha!Forte1", "Marta", termo, telefone, site);

    [Fact]
    public async Task RegistarMembroAsync_CriaContaEEnfileiraCodigo_QuandoContaNova()
    {
        var novo = Guid.NewGuid();
        _identity.Setup(i => i.ValidarSenhaAsync("Senha!Forte1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());
        _identity.Setup(i => i.CriarMembroAsync(
                It.Is<NovoMembro>(n => n.Email == "m@b.c" && n.Telefone == "+5511987654321"
                    && n.TermoVersao == TermoPrivacidade.VersaoAtual),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoCriacao(novo, false, Array.Empty<string>()));
        _identity.Setup(i => i.GerarTokenEmailAsync(novo, It.IsAny<CancellationToken>())).ReturnsAsync("123456");
        _token.Setup(t => t.GerarDesafioLogin(novo)).Returns("td");
        _limite.Setup(l => l.TentarConsumir("m@b.c", FinalidadeEnvio.Cadastro)).Returns(true);

        var r = await Criar().RegistarMembroAsync(Cadastro(" m@b.c ", telefone: "+55 (11) 98765-4321"), default);

        r.TokenDesafio.Should().Be("td");
        r.Erros.Should().BeEmpty();
        _fila.Verify(f => f.Enfileirar("m@b.c", It.IsAny<string>(), It.Is<string>(c => c.Contains("123456"))), Times.Once);
    }

    [Fact]
    public async Task RegistarMembroAsync_NaoEnviaNada_QuandoEmailJaExiste()
    {
        _identity.Setup(i => i.ValidarSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());
        _identity.Setup(i => i.CriarMembroAsync(It.IsAny<NovoMembro>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoCriacao(null, true, Array.Empty<string>()));
        _token.Setup(t => t.GerarDesafioLogin(It.IsAny<Guid>())).Returns("falso");

        var r = await Criar().RegistarMembroAsync(Cadastro(), default);

        r.TokenDesafio.Should().Be("falso");
        _fila.Verify(f => f.Enfileirar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _limite.Verify(l => l.TentarConsumir(It.IsAny<string>(), It.IsAny<FinalidadeEnvio>()), Times.Never);
        _identity.Verify(i => i.GerarTokenEmailAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistarMembroAsync_NaoFazNada_QuandoSiteVemPreenchido()
    {
        _token.Setup(t => t.GerarDesafioLogin(It.IsAny<Guid>())).Returns("falso");

        var r = await Criar().RegistarMembroAsync(Cadastro(site: "x"), default);

        r.TokenDesafio.Should().Be("falso");
        _identity.VerifyNoOtherCalls();
        _fila.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistarMembroAsync_RejeitaSemConsultarEmail_QuandoTermoNaoAceito()
    {
        var r = await Criar().RegistarMembroAsync(Cadastro(termo: false), default);

        r.TokenDesafio.Should().BeNull();
        r.Erros.Should().NotBeEmpty();
        _identity.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistarMembroAsync_RejeitaSemConsultarEmail_QuandoSenhaFraca()
    {
        _identity.Setup(i => i.ValidarSenhaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Falha("fraca"));

        var r = await Criar().RegistarMembroAsync(Cadastro(), default);

        r.Erros.Should().Equal("fraca");
        _identity.Verify(i => i.CriarMembroAsync(It.IsAny<NovoMembro>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
