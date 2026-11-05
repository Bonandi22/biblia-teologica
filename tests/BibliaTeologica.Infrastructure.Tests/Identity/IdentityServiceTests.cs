using AwesomeAssertions;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using BibliaTeologica.Infrastructure.Tests.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

    private static Task<ResultadoCriacao> CriarAsync(IdentityService svc, string email, string nome)
        => svc.CriarMembroAsync(new NovoMembro(email, "Senha!Forte1", nome, null, "v1", DateTime.UtcNow), default);

    private IdentityService Criar()
        => new(_sp.GetRequiredService<UserManager<ApplicationUser>>());

    [Fact]
    public async Task CriarMembro_E_VerificarSenha()
    {
        var svc = Criar();
        (await CriarAsync(svc, "m@teste.local", "Marta")).UserId.Should().NotBeNull();

        var ok = await svc.VerificarSenhaAsync("m@teste.local", "Senha!Forte1", false, default);
        ok.Sucesso.Should().BeTrue();
        ok.UserId.Should().NotBeNull();

        (await svc.VerificarSenhaAsync("m@teste.local", "errada", false, default)).Sucesso.Should().BeFalse();
    }

    private sealed class HasherEspiao : IPasswordHasher<ApplicationUser>
    {
        private readonly PasswordHasher<ApplicationUser> _real = new();
        public int Verificacoes { get; private set; }
        public string HashPassword(ApplicationUser user, string password) => _real.HashPassword(user, password);
        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            Verificacoes++;
            return _real.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }

    [Fact]
    public async Task Login_CalculaHash_QuandoEmailDesconhecido()
    {
        var espiao = new HasherEspiao();
        var svc = new IdentityService(new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser, IdentityRole<Guid>, AppDbContext, Guid>(_sp.GetRequiredService<AppDbContext>()),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()), espiao,
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), _sp,
            _sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>()));

        var r = await svc.VerificarSenhaAsync("ninguem@teste.local", "qualquer", true, default);

        r.Sucesso.Should().BeFalse();
        r.Bloqueado.Should().BeFalse();
        espiao.Verificacoes.Should().Be(1);
    }

    [Fact]
    public async Task EstaBloqueado_DevolveFalso_QuandoContaNaoExiste()
    {
        (await Criar().EstaBloqueadoAsync(Guid.NewGuid(), default)).Should().BeFalse();
    }

    [Fact]
    public async Task ConferirSenha_ContaFalha_QuandoSenhaErrada()
    {
        var svc = Criar();
        await CriarAsync(svc, "c@teste.local", "C");
        var id = (await svc.ResolverIdPorEmailAsync("c@teste.local", default))!.Value;

        (await svc.ConferirSenhaAsync(id, "Senha!Forte1", default)).Sucesso.Should().BeTrue();
        for (var i = 0; i < 4; i++)
            (await svc.ConferirSenhaAsync(id, "errada", default)).Sucesso.Should().BeFalse();
        var ultima = await svc.ConferirSenhaAsync(id, "errada", default);

        ultima.Bloqueado.Should().BeTrue();
        (await svc.EstaBloqueadoAsync(id, default)).Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmarTrocaEmail_Falha_QuandoCodigoEraDeOutroEndereco()
    {
        var svc = Criar();
        await CriarAsync(svc, "d@teste.local", "D");
        var id = (await svc.ResolverIdPorEmailAsync("d@teste.local", default))!.Value;
        var codigo = await svc.GerarCodigoTrocaEmailAsync(id, "a@teste.local", default);

        (await svc.ConfirmarTrocaEmailAsync(id, "b@teste.local", codigo, default)).Sucesso.Should().BeFalse();
        (await svc.ConfirmarTrocaEmailAsync(id, "a@teste.local", codigo, default)).Sucesso.Should().BeTrue();
        (await svc.ObterEmailAsync(id, default)).Should().Be("a@teste.local");
    }

    [Fact]
    public async Task TokenEmail_RoundTrip()
    {
        var svc = Criar();
        await CriarAsync(svc, "t@teste.local", "T");
        var id = (await svc.ResolverIdPorEmailAsync("t@teste.local", default))!.Value;

        var codigo = await svc.GerarTokenEmailAsync(id, default);
        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeTrue();
        (await svc.VerificarTokenEmailAsync(id, "000000", default)).Should().BeFalse();
    }

    private async Task<(IdentityService Svc, Guid Id)> CriarContaAsync()
    {
        var svc = Criar();
        await CriarAsync(svc, "c@teste.local", "C");
        return (svc, (await svc.ResolverIdPorEmailAsync("c@teste.local", default))!.Value);
    }

    [Fact]
    public async Task VerificarTokenEmail_RetornaFalse_QuandoCodigoJaFoiUsado()
    {
        var (svc, id) = await CriarContaAsync();
        var codigo = await svc.GerarTokenEmailAsync(id, default);

        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeTrue();

        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeFalse();
    }

    [Fact]
    public async Task VerificarTokenEmail_RetornaFalse_QuandoContaBloqueada()
    {
        var (svc, id) = await CriarContaAsync();
        var codigo = await svc.GerarTokenEmailAsync(id, default);
        for (var i = 0; i < 5; i++) await svc.RegistarFalhaAsync(id, default);

        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeFalse();
    }

    [Fact]
    public async Task VerificarSenha_MantemFalhasDe2fa_QuandoSenhaCorreta()
    {
        var (svc, id) = await CriarContaAsync();
        await svc.RegistarFalhaAsync(id, default);

        (await svc.VerificarSenhaAsync("c@teste.local", "Senha!Forte1", true, default)).Sucesso.Should().BeTrue();

        var usuarios = _sp.GetRequiredService<UserManager<ApplicationUser>>();
        var usuario = await usuarios.FindByIdAsync(id.ToString());
        (await usuarios.GetAccessFailedCountAsync(usuario!)).Should().Be(1);
    }

    [Theory]
    [InlineData("Senha!Forte1")]
    [InlineData("fraca")]
    public async Task IdentitySeeder_NaoRegistraEmail_QuandoCriaOuFalha(string senha)
    {
        var logger = new LoggerEspiao<IdentitySeeder>();
        var seeder = new IdentitySeeder(
            _sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
            _sp.GetRequiredService<UserManager<ApplicationUser>>(),
            new[] { new AdminSeedEntrada { Email = "dono@teste.local", SenhaInicial = senha } },
            logger);

        await seeder.SeedAsync();

        logger.Mensagens.Should().NotBeEmpty();
        logger.Mensagens.Should().NotContain(m => m.Contains("dono@teste.local"));
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conexao.Dispose();
    }
}
