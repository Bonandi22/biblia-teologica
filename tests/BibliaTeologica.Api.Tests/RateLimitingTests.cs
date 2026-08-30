using System.Net;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task GET_ApiLivros_Retorna429_QuandoExcedeLimite()
    {
        await using var factory = new AppFactory();
        factory.ConfiguracaoExtra["RateLimiting:PermitLimit"] = "3";
        factory.ConfiguracaoExtra["RateLimiting:WindowSeconds"] = "60";

        var cliente = factory.CreateClient();

        HttpStatusCode ultimo = HttpStatusCode.OK;
        for (var i = 0; i < 6; i++)
            ultimo = (await cliente.GetAsync("/api/livros")).StatusCode;

        ultimo.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
