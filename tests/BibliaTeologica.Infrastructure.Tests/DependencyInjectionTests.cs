using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static IServiceProvider Construir(string sink = "Dev")
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["Jwt:SigningKey"] = "chave-de-teste-com-pelo-menos-32-caracteres!!",
            ["Email:Sink"] = sink
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(config);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(typeof(IIdentityService))]
    [InlineData(typeof(ITokenService))]
    [InlineData(typeof(IRefreshTokenStore))]
    [InlineData(typeof(IEmailSender))]
    [InlineData(typeof(ILivroRepository))]
    public void AddInfrastructure_ResolveOsServicos_QuandoConfigValida(Type servico)
    {
        using var scope = Construir().CreateScope();
        scope.ServiceProvider.GetService(servico).Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_Lanca_QuandoSigningKeyCurta()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["Jwt:SigningKey"] = "curta"
        }).Build();

        var acao = () =>
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddInfrastructure(config);
        };

        acao.Should().Throw<InvalidOperationException>();
    }
}
