using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class RedefinicaoSenhaTests
{
    private const string NovaSenha = "Nova!Senha9";
    private const string UrlPublica = "https://site.teste";

    private static async Task<AutenticacaoWebFactory> NovaFabricaAsync(int limite = 100000)
    {
        var f = new AutenticacaoWebFactory();
        f.ConfiguracaoExtra["Site:UrlPublica"] = UrlPublica;
        f.ConfiguracaoExtra["EnvioEmail:MaxPorJanela"] = limite.ToString();
        await f.SemearUsuariosAsync();
        return f;
    }

    private static Task<HttpResponseMessage> Esqueci(HttpClient c, string email, string? site = null)
        => c.PostAsJsonAsync("/api/auth/senha/esqueci", new { email, site });

    private static (string Email, string Token) LerLink(AutenticacaoWebFactory f)
    {
        var href = Regex.Match(f.Otp.UltimoCorpo!, "href=\"([^\"]+)\"").Groups[1].Value;
        var frag = href[(href.IndexOf('#') + 1)..];
        var partes = frag.Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
        return (partes["email"], partes["token"]);
    }

    private static Task<HttpResponseMessage> Redefinir(HttpClient c, string email, string token, string senha = NovaSenha)
        => c.PostAsJsonAsync("/api/auth/senha/redefinir", new { email, token, novaSenha = senha });

    [Fact]
    public async Task Esqueci_Responde202SemEnviar_QuandoEmailDesconhecido()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();

        var res = await Esqueci(client, "ninguem@teste.local");

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await res.Content.ReadAsStringAsync()).Should().BeEmpty();
        f.Otp.Enviados.Should().BeEmpty();
    }

    [Fact]
    public async Task Esqueci_Responde202EEnviaLink_QuandoContaExiste()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();

        var res = await Esqueci(client, AutenticacaoWebFactory.MembroEmail);

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        f.Otp.UltimoPara.Should().Be(AutenticacaoWebFactory.MembroEmail);
        f.Otp.UltimoCorpo.Should().Contain($"href=\"{UrlPublica}/redefinir-senha#email=");
    }

    [Fact]
    public async Task Esqueci_IgnoraCabecalhoHost_AoMontarLink()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/senha/esqueci")
        {
            Content = JsonContent.Create(new { email = AutenticacaoWebFactory.MembroEmail })
        };
        req.Headers.Host = "evil.test";

        var res = await client.SendAsync(req);

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        f.Otp.UltimoCorpo.Should().Contain($"href=\"{UrlPublica}/redefinir-senha#").And.NotContain("evil.test");
    }

    [Fact]
    public async Task Esqueci_Responde202SemEnviar_QuandoSiteVeioPreenchido()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();

        var res = await Esqueci(client, AutenticacaoWebFactory.MembroEmail, site: "http://spam.example");

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        f.Otp.Enviados.Should().BeEmpty();
    }

    [Fact]
    public async Task Esqueci_Responde202SemEnviar_QuandoLimiteEsgotado()
    {
        await using var f = await NovaFabricaAsync(limite: 2);
        var client = f.CreateClient();

        for (var i = 0; i < 4; i++)
            (await Esqueci(client, AutenticacaoWebFactory.MembroEmail)).StatusCode.Should().Be(HttpStatusCode.Accepted);

        f.Otp.Enviados.Count(e => e.Para == AutenticacaoWebFactory.MembroEmail).Should().Be(2);
    }

    [Fact]
    public async Task Esqueci_NaoAfetaLimiteDoLogin_QuandoRepetido()
    {
        await using var f = await NovaFabricaAsync(limite: 2);
        var client = f.CreateClient();
        for (var i = 0; i < 4; i++) await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var antes = f.Otp.Enviados.Count;

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.MembroEmail, senha = AutenticacaoWebFactory.Senha });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        f.Otp.Enviados.Count.Should().Be(antes + 1);
    }

    [Fact]
    public async Task Redefinir_TrocaSenhaERevogaSessoes_QuandoTokenValido()
    {
        await using var f = await NovaFabricaAsync();
        var sessao = await f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        (await sessao.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (email, token) = LerLink(f);

        var res = await Redefinir(client, email, token);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await sessao.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        f.Otp.UltimoPara.Should().Be(AutenticacaoWebFactory.MembroEmail);
        f.Otp.UltimoCorpo.Should().Contain("alterada");
        var velha = await client.PostAsJsonAsync("/api/auth/login",
            new { email, senha = AutenticacaoWebFactory.Senha });
        velha.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var nova = await client.PostAsJsonAsync("/api/auth/login", new { email, senha = NovaSenha });
        nova.StatusCode.Should().Be(HttpStatusCode.OK);
        (await nova.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>())!["desafio2fa"].GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Redefinir_EnviaAvisoDeSeguranca_QuandoAnonimoTentouCadastrarOEndereco()
    {
        await using var f = await NovaFabricaAsync(limite: 2);
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (email, token) = LerLink(f);
        // Um anônimo tenta cadastrar o endereço várias vezes; isso não pode calar o aviso de segurança.
        for (var i = 0; i < 3; i++)
            await client.PostAsJsonAsync("/api/auth/registar", new
            {
                email, senha = "Senha!Forte1", nomeExibicao = "X", aceitouTermo = true
            });
        var antes = f.Otp.Enviados.Count;

        var res = await Redefinir(client, email, token);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        f.Otp.Enviados.Count.Should().Be(antes + 1);
        f.Otp.UltimoCorpo.Should().Contain("alterada");
    }
    [Fact]
    public async Task Login_Funciona_QuandoContaNaoConfirmadaRedefineASenha()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();
        var email = $"novo-{Guid.NewGuid():N}@teste.local";
        await client.PostAsJsonAsync("/api/auth/registar", new
        {
            email, senha = "Senha!Forte1", nomeExibicao = "X", aceitouTermo = true
        });
        (await client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha!Forte1" }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await Esqueci(client, email);
        var (_, token) = LerLink(f);

        (await Redefinir(client, email, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostAsJsonAsync("/api/auth/login", new { email, senha = NovaSenha }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Redefinir_Responde400Generico_QuandoTokenInvalidoOuEmailInexistente()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (_, token) = LerLink(f);

        var tokenRuim = await Redefinir(client, AutenticacaoWebFactory.MembroEmail, "token-ruim");
        var semConta = await Redefinir(client, "ninguem@teste.local", token);

        tokenRuim.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        semConta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var a = await tokenRuim.Content.ReadAsStringAsync();
        var b = await semConta.Content.ReadAsStringAsync();
        a.Should().Be(b).And.Contain("Este link é inválido ou venceu.");
    }

    [Fact]
    public async Task Redefinir_Responde400ComRegra_QuandoTokenValidoESenhaFraca()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (email, token) = LerLink(f);

        var res = await Redefinir(client, email, token, "abcdefgh");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().NotContain("Este link é inválido");
        // O token segue valendo: a senha fraca não o consome.
        (await Redefinir(client, email, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Redefinir_Falha_QuandoTokenReutilizado()
    {
        await using var f = await NovaFabricaAsync();
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (email, token) = LerLink(f);
        (await Redefinir(client, email, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var res = await Redefinir(client, email, token, "Outra!Senha7");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Redefinir_ConfirmaEmailEDesbloqueia_QuandoSucesso()
    {
        await using var f = await NovaFabricaAsync();
        using (var scope = f.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var u = (await users.FindByEmailAsync(AutenticacaoWebFactory.MembroEmail))!;
            u.EmailConfirmed = false;
            u.DeveTrocarSenha = true;
            await users.UpdateAsync(u);
            await users.SetLockoutEndDateAsync(u, DateTimeOffset.UtcNow.AddHours(1));
            await users.AccessFailedAsync(u);
        }
        var client = f.CreateClient();
        await Esqueci(client, AutenticacaoWebFactory.MembroEmail);
        var (email, token) = LerLink(f);

        (await Redefinir(client, email, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope2 = f.Services.CreateScope();
        var users2 = scope2.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var depois = (await users2.FindByEmailAsync(AutenticacaoWebFactory.MembroEmail))!;
        depois.EmailConfirmed.Should().BeTrue();
        depois.DeveTrocarSenha.Should().BeFalse();
        depois.AccessFailedCount.Should().Be(0);
        (await users2.IsLockedOutAsync(depois)).Should().BeFalse();
    }
}
