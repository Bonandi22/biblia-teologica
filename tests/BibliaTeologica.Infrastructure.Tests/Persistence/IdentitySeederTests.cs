using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class IdentitySeederTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly ServiceProvider _sp;

    public IdentitySeederTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        _sp = services.BuildServiceProvider();

        _sp.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    private IdentitySeeder CriarSeeder(params AdminSeedEntrada[] admins)
        => new(_sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
               _sp.GetRequiredService<UserManager<ApplicationUser>>(),
               admins, NullLogger<IdentitySeeder>.Instance);

    [Fact]
    public async Task SeedAsync_CriaRolesEAdmin()
    {
        await CriarSeeder(new AdminSeedEntrada { Email = "admin@teste.local", SenhaInicial = "Senha!Forte1" })
            .SeedAsync();

        var roles = _sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        (await roles.RoleExistsAsync("Admin")).Should().BeTrue();
        (await roles.RoleExistsAsync("Membro")).Should().BeTrue();

        var users = _sp.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync("admin@teste.local");
        admin.Should().NotBeNull();
        admin!.DeveTrocarSenha.Should().BeTrue();
        (await users.IsInRoleAsync(admin, "Admin")).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_Idempotente_NumaSegundaChamada()
    {
        var entrada = new AdminSeedEntrada { Email = "admin@teste.local", SenhaInicial = "Senha!Forte1" };
        await CriarSeeder(entrada).SeedAsync();
        await CriarSeeder(entrada).SeedAsync();

        var users = _sp.GetRequiredService<UserManager<ApplicationUser>>();
        users.Users.Count().Should().Be(1);
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conexao.Dispose();
    }
}
