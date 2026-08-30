using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
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

    [Fact]
    public async Task Eu_Retorna401_SemCookie()
    {
        await _f.SemearUsuariosAsync();

        var res = await _f.CreateClient().GetAsync("/api/auth/eu");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegistarMembro_Retorna201()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar",
            new { email = $"novo-{Guid.NewGuid():N}@teste.local", senha = "Senha!Forte1", nomeExibicao = "Novo" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegistarMembro_Retorna400_QuandoEmailInvalido()
    {
        var client = await ClienteSemeadoAsync();

        var res = await client.PostAsJsonAsync("/api/auth/registar",
            new { email = "nao-e-email", senha = "Senha!Forte1", nomeExibicao = "X" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
