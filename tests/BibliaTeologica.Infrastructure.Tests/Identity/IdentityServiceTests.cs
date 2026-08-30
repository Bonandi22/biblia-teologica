using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public sealed class IdentityServiceTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly ServiceProvider _sp;

    public IdentityServiceTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireNonAlphanumeric = false;
                o.SignIn.RequireConfirmedAccount = false;
                o.Tokens.ProviderMap["Email"] =
                    new TokenProviderDescriptor(typeof(EmailTokenProvider<ApplicationUser>));
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddHttpContextAccessor();
        _sp = services.BuildServiceProvider();
        _sp.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        _sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>()
            .CreateAsync(new IdentityRole<Guid>("Membro")).GetAwaiter().GetResult();
    }

    private IdentityService Criar()
        => new(_sp.GetRequiredService<UserManager<ApplicationUser>>(),
               _sp.GetRequiredService<SignInManager<ApplicationUser>>());

    [Fact]
    public async Task CriarMembro_E_VerificarSenha()
    {
        var svc = Criar();
        (await svc.CriarMembroAsync("m@teste.local", "Senha!Forte1", "Marta", default)).Sucesso.Should().BeTrue();

        var ok = await svc.VerificarSenhaAsync("m@teste.local", "Senha!Forte1", false, default);
        ok.Sucesso.Should().BeTrue();
        ok.UserId.Should().NotBeNull();

        (await svc.VerificarSenhaAsync("m@teste.local", "errada", false, default)).Sucesso.Should().BeFalse();
    }

    [Fact]
    public async Task TokenEmail_RoundTrip()
    {
        var svc = Criar();
        await svc.CriarMembroAsync("t@teste.local", "Senha!Forte1", "T", default);
        var id = (await svc.ResolverIdPorEmailAsync("t@teste.local", default))!.Value;

        var codigo = await svc.GerarTokenEmailAsync(id, default);
        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeTrue();
        (await svc.VerificarTokenEmailAsync(id, "000000", default)).Should().BeFalse();
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conexao.Dispose();
    }
}
