using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Api.Tests.Conta;

public class TrocaEmailTests
{
    private const string Novo = "novo@teste.local";
    private const string Membro = AutenticacaoWebFactory.MembroEmail;
    private const string Senha = AutenticacaoWebFactory.Senha;

    private static async Task<AutenticacaoWebFactory> NovaFabricaAsync(int limite = 100000)
    {
        var f = new AutenticacaoWebFactory();
        f.ConfiguracaoExtra["EnvioEmail:MaxPorJanela"] = limite.ToString();
        await f.SemearUsuariosAsync();
        return f;
    }

    private static Task<HttpResponseMessage> Solicitar(HttpClient c, string novoEmail = Novo, string senha = Senha)
        => c.PostAsJsonAsync("/api/conta/email/solicitar", new { novoEmail, senha });

    private static Task<HttpResponseMessage> Confirmar(HttpClient c, string novoEmail, string? codigo)
        => c.PostAsJsonAsync("/api/conta/email/confirmar", new { novoEmail, codigo });

    private static async Task<string?> EmailDaContaAsync(AutenticacaoWebFactory f, string email)
    {
        using var scope = f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await users.FindByEmailAsync(email))?.Email;
    }

    [Fact]
    public async Task Solicitar_EnviaCodigoAoNovoEndereco_SemTrocarAinda()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);

        var res = await Solicitar(client);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        f.Otp.UltimoPara.Should().Be(Novo);
        f.Otp.UltimoCodigo.Should().MatchRegex(@"^\d{6}$");
        (await EmailDaContaAsync(f, Membro)).Should().Be(Membro);
        (await EmailDaContaAsync(f, Novo)).Should().BeNull();
    }

    [Fact]
    public async Task Solicitar_RespondeIgual_QuandoEnderecoJaUsado()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        var antes = f.Otp.Enviados.Count;

        var res = await Solicitar(client, AutenticacaoWebFactory.AdminEmail);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await res.Content.ReadAsStringAsync()).Should().BeEmpty();
        f.Otp.Enviados.Count.Should().Be(antes + 1);
        f.Otp.UltimoPara.Should().Be(AutenticacaoWebFactory.AdminEmail);
        f.Otp.UltimoCorpo.Should().NotContain("<strong>");
    }

    [Fact]
    public async Task Solicitar_Responde400EContaFalha_QuandoSenhaErrada()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        var antes = f.Otp.Enviados.Count;

        for (var i = 0; i < 5; i++)
        {
            var res = await Solicitar(client, senha: "Errada!123");
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await res.Content.ReadAsStringAsync()).Should().Contain("Senha incorreta.");
        }

        // Bloqueada pelas falhas: nem a senha certa passa mais.
        (await Solicitar(client)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        f.Otp.Enviados.Count.Should().Be(antes);
    }

    [Fact]
    public async Task Solicitar_Responde429_QuandoLimiteEsgotado()
    {
        await using var f = await NovaFabricaAsync(limite: 2);
        var client = await f.LoginAsync(Membro, Senha);

        (await Solicitar(client)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Solicitar(client)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var livre = await Solicitar(client);
        (await Solicitar(client, AutenticacaoWebFactory.AdminEmail)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Solicitar(client, AutenticacaoWebFactory.AdminEmail)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var usado = await Solicitar(client, AutenticacaoWebFactory.AdminEmail);

        livre.StatusCode.Should().Be((HttpStatusCode)429);
        usado.StatusCode.Should().Be((HttpStatusCode)429);
    }

    [Fact]
    public async Task Confirmar_TrocaERevogaOutrasSessoes_QuandoCodigoCerto()
    {
        await using var f = await NovaFabricaAsync();
        var antiga = await f.LoginAsync(Membro, Senha);
        var atual = await f.LoginAsync(Membro, Senha);
        (await antiga.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        await Solicitar(atual);

        var res = await Confirmar(atual, Novo, f.Otp.UltimoCodigo);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.Should().Contain(h => h.Key == "Set-Cookie" && h.Value.Any(c => c.StartsWith("bt_access=")));
        (await EmailDaContaAsync(f, Novo)).Should().Be(Novo);
        (await EmailDaContaAsync(f, Membro)).Should().BeNull();
        (await antiga.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var login = await f.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = Novo, senha = Senha });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Confirmar_MantemEmailConfirmado_QuandoSucesso()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client);

        await Confirmar(client, Novo, f.Otp.UltimoCodigo);

        using var scope = f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var u = await users.FindByEmailAsync(Novo);
        u!.EmailConfirmed.Should().BeTrue();
        u.UserName.Should().Be(Novo);
    }

    [Fact]
    public async Task Confirmar_Troca_QuandoNovoEmailTemApostrofo()
    {
        const string comApostrofo = "o'brien@exemplo.com";
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client, comApostrofo);

        var res = await Confirmar(client, comApostrofo, f.Otp.UltimoCodigo);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var u = await users.FindByEmailAsync(comApostrofo);
        u.Should().NotBeNull();
        u!.UserName.Should().Be(comApostrofo);
        u.EmailConfirmed.Should().BeTrue();
        (await users.FindByNameAsync(comApostrofo)).Should().NotBeNull();
    }
    [Fact]
    public async Task Confirmar_AvisaEnderecoAntigo_QuandoSucesso()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client);

        await Confirmar(client, Novo, f.Otp.UltimoCodigo);

        f.Otp.UltimoPara.Should().Be(Membro);
        f.Otp.UltimoCorpo.Should().Contain("alterado");
    }

    [Fact]
    public async Task Confirmar_Falha_QuandoCodigoDeOutroEndereco()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client, "a@teste.local");

        var res = await Confirmar(client, "b@teste.local", f.Otp.UltimoCodigo);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await EmailDaContaAsync(f, Membro)).Should().Be(Membro);
        (await EmailDaContaAsync(f, "b@teste.local")).Should().BeNull();
    }

    [Fact]
    public async Task Confirmar_ErroGenerico_QuandoEnderecoFoiOcupadoNoMeioTempo()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client);
        var codigo = f.Otp.UltimoCodigo;
        var errado = await Confirmar(client, Novo, "000000");
        using (var scope = f.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await users.CreateAsync(new ApplicationUser { Id = Guid.NewGuid(), UserName = Novo, Email = Novo, NomeExibicao = "X" });
        }

        var res = await Confirmar(client, Novo, codigo);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Be(await errado.Content.ReadAsStringAsync());
        (await EmailDaContaAsync(f, Membro)).Should().Be(Membro);
    }

    [Fact]
    public async Task Confirmar_ContaFalha_QuandoCodigoErrado()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);
        await Solicitar(client);
        var codigo = f.Otp.UltimoCodigo;

        for (var i = 0; i < 5; i++)
            (await Confirmar(client, Novo, "000000")).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Bloqueada: nem o código certo passa mais.
        (await Confirmar(client, Novo, codigo)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await EmailDaContaAsync(f, Membro)).Should().Be(Membro);
    }

    [Fact]
    public async Task PutEmail_Responde405Ou404_DepoisDeRemovido()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha);

        var res = await client.PutAsJsonAsync("/api/conta/email", new { novoEmail = Novo, senha = Senha });

        res.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Solicitar_Responde403_QuandoSemCsrf()
    {
        await using var f = await NovaFabricaAsync();
        var client = await f.LoginAsync(Membro, Senha, comCsrf: false);

        var res = await Solicitar(client);

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
