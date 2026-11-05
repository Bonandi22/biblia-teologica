using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class SegurancaApiTests
{
    private const string Membro = AutenticacaoWebFactory.MembroEmail;
    private const string Senha = AutenticacaoWebFactory.Senha;
    private const string NovaSenha = "Outra!Forte2";

    /// <summary>Simula a conexão chegando de um IP específico, como o do nginx no Docker.</summary>
    private sealed class FabricaComConexao : AppFactory
    {
        private readonly string _ipConexao;

        public FabricaComConexao(string ipConexao)
        {
            _ipConexao = ipConexao;
            ConfiguracaoExtra["Proxy:RedesConfiaveis:0"] = "172.16.0.0/12";
            ConfiguracaoExtra["RateLimiting:PermitLimit"] = "2";
            ConfiguracaoExtra["RateLimiting:WindowSeconds"] = "60";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(s => s.AddSingleton<IStartupFilter>(new FiltroConexao(_ipConexao)));
        }

        private sealed class FiltroConexao : IStartupFilter
        {
            private readonly IPAddress _ip;

            public FiltroConexao(string ip) => _ip = IPAddress.Parse(ip);

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use((contexto, proximo) =>
                {
                    contexto.Connection.RemoteIpAddress = _ip;
                    return proximo();
                });
                next(app);
            };
        }
    }

    private static Task<HttpResponseMessage> GetDoCliente(HttpClient cliente, string ipCliente)
    {
        var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/livros");
        pedido.Headers.TryAddWithoutValidation("X-Forwarded-For", ipCliente);
        return cliente.SendAsync(pedido);
    }

    [Fact]
    public async Task RateLimit_SeparaClientes_QuandoConexaoVemDoProxyConfiavel()
    {
        await using var fabrica = new FabricaComConexao("172.18.0.5");
        var cliente = fabrica.CreateClient();

        await GetDoCliente(cliente, "203.0.113.1");
        await GetDoCliente(cliente, "203.0.113.1");
        var esgotado = await GetDoCliente(cliente, "203.0.113.1");
        var outroCliente = await GetDoCliente(cliente, "203.0.113.2");

        esgotado.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        outroCliente.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateLimit_IgnoraCabecalhoForjado_QuandoConexaoNaoVemDoProxy()
    {
        await using var fabrica = new FabricaComConexao("198.51.100.7");
        var cliente = fabrica.CreateClient();

        await GetDoCliente(cliente, "203.0.113.1");
        await GetDoCliente(cliente, "203.0.113.2");
        var terceiro = await GetDoCliente(cliente, "203.0.113.3");

        terceiro.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task TrocarSenha_EncerraOutrasSessoes_SemDerrubarASessaoAtual()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var atual = await fabrica.LoginAsync(Membro, Senha);
        var outra = await fabrica.LoginAsync(Membro, Senha);

        var troca = await atual.PostAsJsonAsync("/api/conta/senha", new { senhaAtual = Senha, novaSenha = NovaSenha });
        troca.StatusCode.Should().Be(HttpStatusCode.OK);

        (await outra.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await atual.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<HttpClient> LoginComSenhaPendenteAsync(AutenticacaoWebFactory fabrica)
    {
        const string email = "pendente@teste.local";
        await fabrica.SemearUsuariosAsync();
        using (var escopo = fabrica.Services.CreateScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var usuario = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NomeExibicao = "Pendente",
                DeveTrocarSenha = true
            };
            await usuarios.CreateAsync(usuario, Senha);
            await usuarios.AddToRoleAsync(usuario, "Membro");
        }
        return await fabrica.LoginAsync(email, Senha);
    }

    [Fact]
    public async Task EndpointAutenticado_Retorna403_QuandoSenhaInicialPendente()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = await LoginComSenhaPendenteAsync(fabrica);

        var r = await cliente.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Novo" });

        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuthEu_Retorna200_QuandoSenhaInicialPendente()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = await LoginComSenhaPendenteAsync(fabrica);

        (await cliente.GetAsync("/api/auth/eu")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointAutenticado_VoltaAFuncionar_QuandoSenhaInicialFoiTrocada()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = await LoginComSenhaPendenteAsync(fabrica);

        var troca = await cliente.PostAsJsonAsync("/api/conta/senha", new { senhaAtual = Senha, novaSenha = NovaSenha });
        var r = await cliente.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Novo" });

        troca.StatusCode.Should().Be(HttpStatusCode.OK);
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Login_Retorna400_QuandoSenhaExcedeLimite()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = fabrica.CreateClient();

        var r = await cliente.PostAsJsonAsync("/api/auth/login", new { email = Membro, senha = new string('a', 129) });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registar_Retorna400_QuandoSenhaExcedeLimite()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = fabrica.CreateClient();

        var r = await cliente.PostAsJsonAsync("/api/auth/registar", new
        {
            email = "novo@teste.local",
            senha = "Aa1!" + new string('a', 125),
            nomeExibicao = "Novo",
            aceitouTermo = true
        });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login2fa_Retorna400_QuandoCodigoExcedeLimite()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = fabrica.CreateClient();

        var r = await cliente.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = "x", codigo = new string('1', 11) });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TrocarSenha_Retorna400_QuandoNovaSenhaExcedeLimite()
    {
        await using var fabrica = new AutenticacaoWebFactory();
        var cliente = await fabrica.LoginAsync(Membro, Senha);

        var r = await cliente.PostAsJsonAsync("/api/conta/senha", new
        {
            senhaAtual = Senha,
            novaSenha = "Aa1!" + new string('a', 125)
        });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
