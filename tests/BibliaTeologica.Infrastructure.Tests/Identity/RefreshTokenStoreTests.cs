using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public sealed class RefreshTokenStoreTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public RefreshTokenStoreTests()
    {
        // Foreign Keys=False: estes testes exercitam o RefreshTokenStore isoladamente,
        // com UserIds aleatorios sem linha correspondente em AspNetUsers.
        _conexao = new SqliteConnection("Filename=:memory:;Foreign Keys=False");
        _conexao.Open();
        using var ctx = Novo();
        ctx.Database.EnsureCreated();
    }

    private AppDbContext Novo()
        => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);

    [Fact]
    public async Task GuardarEObter_DevolveInfoValida()
    {
        var uid = Guid.NewGuid();
        await new RefreshTokenStore(Novo()).GuardarAsync(uid, "h1", DateTime.UtcNow.AddDays(1), "ip", default);

        var info = await new RefreshTokenStore(Novo()).ObterAsync("h1", default);
        info.Should().NotBeNull();
        info!.UserId.Should().Be(uid);
        info.Valido.Should().BeTrue();
    }

    [Fact]
    public async Task ObterAsync_TokenExpirado_NaoEValido()
    {
        await new RefreshTokenStore(Novo()).GuardarAsync(Guid.NewGuid(), "h2", DateTime.UtcNow.AddMinutes(-1), null, default);
        (await new RefreshTokenStore(Novo()).ObterAsync("h2", default))!.Valido.Should().BeFalse();
    }

    [Fact]
    public async Task Rotacionar_RevogaAntigoEEncadeia()
    {
        var uid = Guid.NewGuid();
        await new RefreshTokenStore(Novo()).GuardarAsync(uid, "antigo", DateTime.UtcNow.AddDays(1), null, default);
        await new RefreshTokenStore(Novo()).RotacionarAsync("antigo", uid, "novo", DateTime.UtcNow.AddDays(1), null, default);

        var antigo = await new RefreshTokenStore(Novo()).ObterAsync("antigo", default);
        antigo!.Valido.Should().BeFalse();
        antigo.RevogadoEm.Should().NotBeNull();
        (await new RefreshTokenStore(Novo()).ObterAsync("novo", default))!.Valido.Should().BeTrue();
    }

    [Fact]
    public async Task RevogarCadeia_RevogaTodosDoUser()
    {
        var uid = Guid.NewGuid();
        var store = new RefreshTokenStore(Novo());
        await store.GuardarAsync(uid, "a", DateTime.UtcNow.AddDays(1), null, default);
        await store.GuardarAsync(uid, "b", DateTime.UtcNow.AddDays(1), null, default);

        await new RefreshTokenStore(Novo()).RevogarCadeiaAsync(uid, default);

        (await new RefreshTokenStore(Novo()).ObterAsync("a", default))!.Valido.Should().BeFalse();
        (await new RefreshTokenStore(Novo()).ObterAsync("b", default))!.Valido.Should().BeFalse();
    }

    public void Dispose() => _conexao.Dispose();
}
