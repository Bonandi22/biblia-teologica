using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BibliaTeologica.Api.Tests;

public class AppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _arquivoDb = Path.Combine(Path.GetTempPath(), $"biblia-teste-{Guid.NewGuid():N}.db");
    private readonly List<IServiceScope> _escopos = new();

    public Dictionary<string, string?> ConfiguracaoExtra { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // No modelo de hosting mínimo, fontes adicionadas via ConfigureAppConfiguration
        // do WebApplicationFactory ficam ABAIXO do appsettings.json; UseSetting sobrepõe.
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_arquivoDb}");
        // Chave de assinatura só-de-teste: o guard de AddInfrastructure (Task 12) exige >= 32 chars.
        // Definida ANTES do loop para que um teste a possa sobrepor via ConfiguracaoExtra.
        builder.UseSetting("Jwt:SigningKey", "chave-de-teste-com-mais-de-32-caracteres-para-hs256!!");
        foreach (var (chave, valor) in ConfiguracaoExtra)
            builder.UseSetting(chave, valor);
    }

    public async Task<AppDbContext> AbrirContextoAsync()
    {
        var scope = Services.CreateScope();
        _escopos.Add(scope);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        return db;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        foreach (var escopo in _escopos)
            escopo.Dispose();

        await DisposeAsync();

        // O pool de conexões do SQLite mantém o arquivo aberto após o dispose do host.
        SqliteConnection.ClearAllPools();
        try
        {
            if (File.Exists(_arquivoDb)) File.Delete(_arquivoDb);
        }
        catch (IOException)
        {
            // arquivo temporário; o SO limpa depois se ainda estiver travado.
        }
    }
}
