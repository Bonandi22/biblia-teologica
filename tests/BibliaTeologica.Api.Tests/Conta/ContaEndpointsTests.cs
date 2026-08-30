using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests.Conta;

public class ContaEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;

    public ContaEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    [Fact]
    public async Task GetConta_Retorna200_ComPerfil()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);

        var res = await client.GetAsync("/api/conta");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        corpo!["email"].GetString().Should().Be(AutenticacaoWebFactory.MembroEmail);
    }

    [Fact]
    public async Task GetConta_Retorna401_SemAutenticacao()
    {
        await _f.SemearUsuariosAsync();

        var res = await _f.CreateClient().GetAsync("/api/conta");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutConta_Retorna403_SemHeaderCsrf()
    {
        var client = await _f.LoginAsync(
            AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha, comCsrf: false);

        var res = await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Novo Nome" });

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostSenha_TrocaEReemiteCookies()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);

        var res = await client.PostAsJsonAsync("/api/conta/senha",
            new { senhaAtual = AutenticacaoWebFactory.Senha, novaSenha = "OutraSenha!2" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.Should().Contain(h => h.Key == "Set-Cookie"
            && h.Value.Any(c => c.StartsWith("bt_access=")));
    }
}
