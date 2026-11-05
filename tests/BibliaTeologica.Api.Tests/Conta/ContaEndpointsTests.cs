using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    private static async Task<JsonElement> ObterContaAsync(HttpClient client)
    {
        var res = await client.GetAsync("/api/conta");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Atualizar_GuardaTelefoneNormalizado_QuandoValido()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);

        var res = await client.PutAsJsonAsync("/api/conta",
            new { nomeExibicao = "Membro", telefone = "+55 (11) 99999-8888" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterContaAsync(client)).GetProperty("telefone").GetString().Should().Be("+5511999998888");
    }

    [Fact]
    public async Task Atualizar_ApagaTelefone_QuandoVazio()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        (await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Membro", telefone = "11999998888" }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var res = await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Membro", telefone = "" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterContaAsync(client)).GetProperty("telefone").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Atualizar_ApagaTelefone_QuandoAusente()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        (await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Membro", telefone = "11999998888" }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var res = await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Outro Nome" });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterContaAsync(client)).GetProperty("telefone").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Atualizar_Responde400_QuandoTelefoneInvalido()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);

        var res = await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Membro", telefone = "abc123" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Eu_NaoExpoeTelefone()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        (await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Membro", telefone = "11987654321" }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var res = await client.GetAsync("/api/auth/eu");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var texto = await res.Content.ReadAsStringAsync();
        texto.Should().NotContain("11987654321");
        texto.ToLowerInvariant().Should().NotContain("telefone");
    }

    [Fact]
    public async Task TrocarSenha_EnfileiraAviso_QuandoSucesso()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        var antes = _f.Otp.Enviados.Count;

        var res = await client.PostAsJsonAsync("/api/conta/senha",
            new { senhaAtual = AutenticacaoWebFactory.Senha, novaSenha = "OutraSenha!2" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        _f.Otp.Enviados.Skip(antes).Should().Contain(e =>
            e.Para == AutenticacaoWebFactory.MembroEmail && e.Assunto == "Sua senha foi alterada");
    }

    private async Task<(string Email, Guid Id)> CriarUsuarioAsync(string role = "Membro")
    {
        await _f.SemearUsuariosAsync();
        using var scope = _f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"apagar-{Guid.NewGuid():N}@teste.local";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true,
            NomeExibicao = "Apagar", DeveTrocarSenha = false
        };
        (await users.CreateAsync(user, AutenticacaoWebFactory.Senha)).Succeeded.Should().BeTrue();
        await users.AddToRoleAsync(user, role);
        return (email, user.Id);
    }

    private static HttpRequestMessage Apagar(string senha) => new(HttpMethod.Delete, "/api/conta")
    {
        Content = JsonContent.Create(new { senha })
    };

    [Fact]
    public async Task Apagar_RemoveContaESessoesELimpaCookies_QuandoSenhaCerta()
    {
        var (email, id) = await CriarUsuarioAsync();
        var client = await _f.LoginAsync(email, AutenticacaoWebFactory.Senha);
        // Outra sessão da mesma conta: o refresh dela, com cookies e CSRF ainda válidos, precisa morrer junto.
        var outraSessao = await _f.LoginAsync(email, AutenticacaoWebFactory.Senha);
        var antes = _f.Otp.Enviados.Count;

        var res = await client.SendAsync(Apagar(AutenticacaoWebFactory.Senha));

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var cookies = res.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("bt_access=;"));
        cookies.Should().Contain(c => c.StartsWith("bt_refresh=;"));
        cookies.Should().Contain(c => c.StartsWith("bt_csrf=;"));

        using (var scope = _f.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.RefreshTokens.CountAsync(t => t.UsuarioId == id)).Should().Be(0);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            (await users.FindByIdAsync(id.ToString())).Should().BeNull();
        }

        (await outraSessao.PostAsync("/api/auth/refresh", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _f.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email, senha = AutenticacaoWebFactory.Senha })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        _f.Otp.Enviados.Skip(antes).Should().Contain(e => e.Para == email && e.Assunto == "Sua conta foi apagada");

        // O e-mail fica livre para um novo cadastro.
        using var scope2 = _f.Services.CreateScope();
        var u2 = scope2.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await u2.CreateAsync(new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email, Email = email, NomeExibicao = "Novo"
        }, AutenticacaoWebFactory.Senha)).Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Apagar_Responde400EContaFalha_QuandoSenhaErrada()
    {
        var (email, id) = await CriarUsuarioAsync();
        var client = await _f.LoginAsync(email, AutenticacaoWebFactory.Senha);
        var antes = _f.Otp.Enviados.Count;

        var res = await client.SendAsync(Apagar("Errada!123"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = _f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByIdAsync(id.ToString());
        user.Should().NotBeNull();
        user!.AccessFailedCount.Should().Be(1);
        _f.Otp.Enviados.Skip(antes).Should().NotContain(e => e.Assunto == "Sua conta foi apagada");
    }

    [Fact]
    public async Task Apagar_Responde409_QuandoUltimoAdmin()
    {
        await _f.SemearUsuariosAsync();
        using (var scope = _f.Services.CreateScope())
        {
            // Deixa o admin de teste como único administrador.
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var a in await users.GetUsersInRoleAsync("Admin"))
                if (a.Email != AutenticacaoWebFactory.AdminEmail)
                    await users.DeleteAsync(a);
        }
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);

        var res = await client.SendAsync(Apagar(AutenticacaoWebFactory.Senha));

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var scope2 = _f.Services.CreateScope();
        var u = scope2.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await u.FindByEmailAsync(AutenticacaoWebFactory.AdminEmail)).Should().NotBeNull();
    }

    [Fact]
    public async Task Apagar_Permite_QuandoHaOutroAdmin()
    {
        var (email, id) = await CriarUsuarioAsync(role: "Admin");
        var client = await _f.LoginAsync(email, AutenticacaoWebFactory.Senha);

        var res = await client.SendAsync(Apagar(AutenticacaoWebFactory.Senha));

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = _f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await users.FindByIdAsync(id.ToString())).Should().BeNull();
    }

    [Fact]
    public async Task Apagar_Responde403_QuandoSemCsrf()
    {
        var (email, id) = await CriarUsuarioAsync();
        var client = await _f.LoginAsync(email, AutenticacaoWebFactory.Senha, comCsrf: false);

        var res = await client.SendAsync(Apagar(AutenticacaoWebFactory.Senha));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = _f.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await users.FindByIdAsync(id.ToString())).Should().NotBeNull();
    }
}
