using System.Net;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class InfraestruturaTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public InfraestruturaTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_Retorna200()
    {
        var resposta = await _factory.CreateClient().GetAsync("/health");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Resposta_ContemSecurityHeaders()
    {
        var resposta = await _factory.CreateClient().GetAsync("/health");
        resposta.Headers.TryGetValues("X-Content-Type-Options", out var valores).Should().BeTrue();
        valores!.Should().Contain("nosniff");
    }

    [Fact]
    public async Task Startup_SemeiaOs66Livros()
    {
        _ = _factory.CreateClient();
        await using var db = await _factory.AbrirContextoAsync();
        db.Livros.Count().Should().Be(66);
    }
}
