using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta;
using BibliaTeologica.Application.Conta.Dtos;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Conta;

public class ContaServiceTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<IEmissorTokens> _emissor = new();
    private static readonly Guid Uid = Guid.NewGuid();

    private readonly Mock<IRefreshTokenStore> _store = new();

    private readonly Mock<IFilaEmail> _fila = new();
    private readonly Mock<ILimiteEnvio> _limite = new();

    private ContaService Criar()
    {
        _limite.Setup(l => l.TentarConsumir(It.IsAny<string>(), It.IsAny<FinalidadeEnvio>())).Returns(true);
        return new(_identity.Object, _emissor.Object, _store.Object, _fila.Object, _limite.Object);
    }

    [Fact]
    public async Task SolicitarTrocaEmailAsync_NaoEnvia_QuandoSenhaErrada()
    {
        _identity.Setup(i => i.ConferirSenhaAsync(Uid, "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, false, null));

        var r = await Criar().SolicitarTrocaEmailAsync(Uid, new SolicitarTrocaEmailDto("n@b.c", "x"), default);

        r.Sucesso.Should().BeFalse();
        r.Erros.Should().Contain("Senha incorreta.");
        _fila.Verify(f => f.Enfileirar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarTrocaEmailAsync_SinalizaLimite_QuandoEsgotado()
    {
        _identity.Setup(i => i.ConferirSenhaAsync(Uid, "s", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(true, false, Uid));
        var svc = Criar();
        _limite.Setup(l => l.TentarConsumir("n@b.c", FinalidadeEnvio.TrocaEmail)).Returns(false);

        var r = await svc.SolicitarTrocaEmailAsync(Uid, new SolicitarTrocaEmailDto("n@b.c", "s"), default);

        r.Limite.Should().BeTrue();
        _fila.Verify(f => f.Enfileirar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmarTrocaEmailAsync_NaoRevogaNemEmite_QuandoCodigoInvalido()
    {
        _identity.Setup(i => i.ConfirmarTrocaEmailAsync(Uid, "n@b.c", "1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Falha("Código inválido ou vencido."));

        var r = await Criar().ConfirmarTrocaEmailAsync(Uid, new ConfirmarTrocaEmailDto("n@b.c", "1"), null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _store.Verify(s => s.RevogarCadeiaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _emissor.Verify(e => e.EmitirAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrocarSenhaAsync_NaoRevogaSessoes_QuandoIdentityFalha()
    {
        _identity.Setup(i => i.TrocarSenhaAsync(Uid, "errada", "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Falha("Senha atual incorreta."));

        await Criar().TrocarSenhaAsync(Uid, new TrocarSenhaDto("errada", "x"), null, default);

        _store.Verify(s => s.RevogarCadeiaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrocarSenhaAsync_ReemiteTokens_QuandoSucesso()
    {
        _identity.Setup(i => i.TrocarSenhaAsync(Uid, "atual", "NovaSenha!1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());
        _emissor.Setup(e => e.EmitirAsync(Uid, "ip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoLoginDto(EstadoLogin.Autenticado,
                Usuario: new UsuarioDto("a@b.c", "A", new[] { "Membro" }, false),
                Tokens: new TokensGerados("acc", DateTime.UtcNow, "ref", DateTime.UtcNow, "csrf")));

        var r = await Criar().TrocarSenhaAsync(Uid, new TrocarSenhaDto("atual", "NovaSenha!1"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("acc");
    }

    [Fact]
    public async Task TrocarSenhaAsync_DevolveErros_QuandoIdentityFalha()
    {
        _identity.Setup(i => i.TrocarSenhaAsync(Uid, "errada", "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Falha("Senha atual incorreta."));

        var r = await Criar().TrocarSenhaAsync(Uid, new TrocarSenhaDto("errada", "x"), null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        r.Erros.Should().Contain("Senha atual incorreta.");
        _emissor.Verify(e => e.EmitirAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterAsync_MapeiaPerfil()
    {
        _identity.Setup(i => i.ObterUsuarioAsync(Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Membro" }, false));

        var p = await Criar().ObterAsync(Uid, default);
        p!.Email.Should().Be("a@b.c");
        p.NomeExibicao.Should().Be("Ana");
    }
}
