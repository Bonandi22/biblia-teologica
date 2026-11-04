using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class EmailEmProducaoTests
{
    private sealed class FabricaProducao : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
            builder.UseSetting("Jwt:SigningKey", "chave-de-teste-com-mais-de-32-caracteres-para-hs256!!");
            builder.UseSetting("Email:Sink", "Dev");
        }
    }

    [Fact]
    public void Api_RecusaSubir_QuandoProducaoComSinkDev()
    {
        using var fabrica = new FabricaProducao();

        var subir = () => fabrica.Services;

        subir.Should().Throw<InvalidOperationException>().WithMessage("*Email:Sink*");
    }
}
