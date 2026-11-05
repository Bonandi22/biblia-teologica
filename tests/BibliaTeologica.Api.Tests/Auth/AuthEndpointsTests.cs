using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class AuthEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;

    public AuthEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    private async Task<HttpClient> ClienteSemeadoAsync()
    {
        await _f.SemearUsuariosAsync();
        return _f.CreateClient();
    }

    [Fact]
    public async Task Login_Retorna200ComDesafio_QuandoCredenciaisValidas()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = AutenticacaoWebFactory.Senha });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        corpo!["desafio2fa"].GetBoolean().Should().BeTrue();
        _f.Otp.UltimoCodigo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Retorna401_QuandoSenhaErrada()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = "errada" });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FluxoCompleto_LoginE2fa_GravaCookiesEDaAcessoAoEu()
    {
        var client = _f.CreateClient();
        await _f.SemearUsuariosAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.MembroEmail, senha = AutenticacaoWebFactory.Senha });
        var corpo = await login.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var td = corpo!["tokenDesafio"].GetString();
        var codigo = _f.Otp.UltimoCodigo!;

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = td, codigo });
        v.StatusCode.Should().Be(HttpStatusCode.OK);
        v.Headers.Should().Contain(h => h.Key == "Set-Cookie"
            && h.Value.Any(c => c.StartsWith("bt_access=")));

        var eu = await client.GetAsync("/api/auth/eu");
        eu.StatusCode.Should().Be(HttpStatusCode.OK);

        // Contrato: corpo "nu", sem wrapper "usuario" (o frontend faz http.get<Usuario> direto).
        using var doc = JsonDocument.Parse(await eu.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("usuario", out _).Should().BeFalse();
        doc.RootElement.GetProperty("email").GetString().Should().Be(AutenticacaoWebFactory.MembroEmail);
    }

    [Fact]
    public async Task Login2fa_Retorna401_QuandoCodigoErrado()
    {
        var client = await ClienteSemeadoAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = AutenticacaoWebFactory.Senha });
        var corpo = await login.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var td = corpo!["tokenDesafio"].GetString();

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = td, codigo = "000000" });

        v.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>Faz o login e devolve os cookies de refresh e CSRF, sem o de acesso: é a sessão com o acesso vencido.</summary>
    private async Task<(string Refresh, string Csrf)> SessaoComAcessoVencidoAsync()
    {
        var client = await ClienteSemeadoAsync();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.MembroEmail, senha = AutenticacaoWebFactory.Senha });
        var corpo = await login.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var v = await client.PostAsJsonAsync("/api/auth/login/2fa",
            new { tokenDesafio = corpo!["tokenDesafio"].GetString(), codigo = _f.Otp.UltimoCodigo! });
        v.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = v.Headers.GetValues("Set-Cookie")
            .Select(c => c.Split(';')[0].Split('=', 2))
            .ToDictionary(p => p[0], p => p[1]);
        return (cookies["bt_refresh"], cookies["bt_csrf"]);
    }

    private async Task<HttpResponseMessage> PostarComCookiesAsync(string url, string refresh, string csrf, bool comHeaderCsrf)
    {
        var client = _f.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("Cookie", $"bt_refresh={refresh}; bt_csrf={csrf}");
        if (comHeaderCsrf) req.Headers.Add("X-CSRF-Token", Uri.UnescapeDataString(csrf));
        return await client.SendAsync(req);
    }

    [Fact]
    public async Task Logout_RevogaRefresh_QuandoAcessoVencido()
    {
        var (refresh, csrf) = await SessaoComAcessoVencidoAsync();

        var res = await PostarComCookiesAsync("/api/auth/logout", refresh, csrf, comHeaderCsrf: true);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        res.Headers.GetValues("Set-Cookie").Should().Contain(c => c.StartsWith("bt_refresh=;"));
        var depois = await PostarComCookiesAsync("/api/auth/refresh", refresh, csrf, comHeaderCsrf: true);
        depois.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Retorna403ENaoRevoga_QuandoSemCsrf()
    {
        var (refresh, csrf) = await SessaoComAcessoVencidoAsync();

        var res = await PostarComCookiesAsync("/api/auth/logout", refresh, csrf, comHeaderCsrf: false);

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var depois = await PostarComCookiesAsync("/api/auth/refresh", refresh, csrf, comHeaderCsrf: true);
        depois.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Eu_Retorna401_SemCookie()
    {
        await _f.SemearUsuariosAsync();

        var res = await _f.CreateClient().GetAsync("/api/auth/eu");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static object Corpo(string email, string senha = "Senha!Forte1", bool termo = true,
        string? telefone = null, string? site = null)
        => new { email, senha, nomeExibicao = "Novo", aceitouTermo = termo, telefone, site };

    private static string EmailNovo() => $"novo-{Guid.NewGuid():N}@teste.local";

    private async Task<ApplicationUser?> BuscarAsync(string email)
    {
        using var scope = _f.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
    }

    private int AvisosPara(string email) => _f.Otp.Enviados.Count(e =>
        string.Equals(e.Para, email, StringComparison.OrdinalIgnoreCase) && e.Assunto == ModelosEmail.CadastroExistente().Assunto);

    [Fact]
    public async Task Registar_Responde202ComDesafio_QuandoContaNova()
    {
        var client = await ClienteSemeadoAsync();
        var email = EmailNovo();

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(email, telefone: "+55 (11) 98765-4321"));

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        corpo!["tokenDesafio"].GetString().Should().NotBeNullOrWhiteSpace();
        var user = await BuscarAsync(email);
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeFalse();
        user.TermoVersao.Should().Be(TermoPrivacidade.VersaoAtual);
        user.TermoAceitoEm.Should().NotBeNull();
        user.PhoneNumber.Should().Be("+5511987654321");
        _f.Otp.UltimoPara.Should().Be(email);
        _f.Otp.UltimoCodigo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Registar_Responde202ComMesmoFormato_QuandoEmailJaExiste()
    {
        var client = await ClienteSemeadoAsync();
        var antes = await BuscarAsync(AutenticacaoWebFactory.MembroEmail);
        var avisosAntes = AvisosPara(AutenticacaoWebFactory.MembroEmail);

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(AutenticacaoWebFactory.MembroEmail, "Outra!Senha9"));

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        corpo!.Keys.Should().BeEquivalentTo(new[] { "tokenDesafio" });
        var depois = await BuscarAsync(AutenticacaoWebFactory.MembroEmail);
        depois!.PasswordHash.Should().Be(antes!.PasswordHash);
        depois.TermoVersao.Should().BeNull();
        AvisosPara(AutenticacaoWebFactory.MembroEmail).Should().Be(avisosAntes + 1);
        _f.Otp.UltimoCorpo.Should().NotContain("<strong>");

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa",
            new { tokenDesafio = corpo["tokenDesafio"].GetString(), codigo = "123456" });
        v.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Registar_MesmaConta_QuandoEmailDifereEmCaixaEEspaco()
    {
        var client = await ClienteSemeadoAsync();
        var avisosAntes = AvisosPara(AutenticacaoWebFactory.MembroEmail);

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(" Membro@Teste.Local "));

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        AvisosPara(AutenticacaoWebFactory.MembroEmail).Should().Be(avisosAntes + 1);
    }

    [Fact]
    public async Task Registar_NaoCriaNemEnvia_QuandoCampoSiteVeioPreenchido()
    {
        await using var f = new AutenticacaoWebFactory();
        var client = f.CreateClient();
        var email = EmailNovo();

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(email, site: "http://spam.example"));

        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        f.Otp.Enviados.Should().BeEmpty();
        await f.SemearUsuariosAsync();
        using var scope = f.Services.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))
            .Should().BeNull();
    }

    [Fact]
    public async Task Registar_Responde400_QuandoSenhaFraca_MesmoComEmailExistente()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(AutenticacaoWebFactory.MembroEmail, "abcdefgh"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registar_Responde400_QuandoTermoNaoAceito_MesmoComEmailExistente()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(AutenticacaoWebFactory.MembroEmail, termo: false));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registar_Responde400_QuandoTelefoneInvalido()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(EmailNovo(), telefone: "12ab"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registar_DescartaEnvioE_Responde202_QuandoLimiteEsgotado()
    {
        await using var f = new AutenticacaoWebFactory();
        f.ConfiguracaoExtra["EnvioEmail:MaxPorJanela"] = "1";
        var client = f.CreateClient();
        await f.SemearUsuariosAsync();
        var email = EmailNovo();

        for (var i = 0; i < 4; i++)
        {
            var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(email));
            res.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        // 1 código (cadastro) + 1 aviso; o resto cai no limite.
        f.Otp.Enviados.Count(e => e.Para == email).Should().Be(2);
    }

    [Fact]
    public async Task VerificarCodigo_AbreSessaoEConfirmaEmail_QuandoCadastroNovo()
    {
        var client = await ClienteSemeadoAsync();
        var email = EmailNovo();
        var res = await client.PostAsJsonAsync("/api/auth/registar", Corpo(email));
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa",
            new { tokenDesafio = corpo!["tokenDesafio"].GetString(), codigo = _f.Otp.UltimoCodigo });

        v.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/auth/eu")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await BuscarAsync(email))!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task RegistarMembro_Retorna400_QuandoEmailInvalido()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar",
            new { email = "nao-e-email", senha = "Senha!Forte1", nomeExibicao = "X", aceitouTermo = true });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
