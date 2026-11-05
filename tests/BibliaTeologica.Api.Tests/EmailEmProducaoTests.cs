using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class EmailEmProducaoTests
{
    private sealed class FabricaProducao(string sink, string? urlPublica = "https://site.teste") : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Default", "Data Source=:memory:");
            builder.UseSetting("Jwt:SigningKey", "chave-de-teste-com-mais-de-32-caracteres-para-hs256!!");
            builder.UseSetting("Email:Sink", sink);
            if (sink == "Smtp")
            {
                builder.UseSetting("Email:Smtp:Host", "smtp.teste");
                builder.UseSetting("Email:Smtp:User", "u");
                builder.UseSetting("Email:Smtp:Password", "p");
                builder.UseSetting("Email:Smtp:From", "a@teste");
            }
            if (urlPublica is not null) builder.UseSetting("Site:UrlPublica", urlPublica);
        }
    }

    [Fact]
    public void Api_RecusaSubir_QuandoProducaoComSinkDev()
    {
        using var fabrica = new FabricaProducao("Dev");

        var subir = () => fabrica.Services;

        subir.Should().Throw<InvalidOperationException>().WithMessage("*Email:Sink*");
    }

    [Fact]
    public void Producao_FalhaAoIniciar_QuandoSiteUrlPublicaAusente()
    {
        using var fabrica = new FabricaProducao("Smtp", urlPublica: null);

        var subir = () => fabrica.Services;

        subir.Should().Throw<InvalidOperationException>().WithMessage("*Site:UrlPublica*");
    }
}
