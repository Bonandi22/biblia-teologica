using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BibliaTeologica.Api.Tests;

/// <summary>Sink de e-mail em memória — captura o último corpo enviado e extrai o código 2FA.</summary>
public sealed class OtpSink : IEmailSender
{
    public string? UltimoCorpo { get; private set; }
    public string? UltimoPara { get; private set; }

    /// <summary>Quando verdadeiro, o envio lança, como um SMTP fora do ar.</summary>
    public bool Falhar { get; set; }

    private readonly ConcurrentQueue<(string Para, string Assunto, string Corpo)> _enviados = new();

    /// <summary>Todos os e-mails recebidos, na ordem de envio.</summary>
    public IReadOnlyList<(string Para, string Assunto, string Corpo)> Enviados => _enviados.ToArray();

    public Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        if (Falhar) throw new InvalidOperationException("detalhe-interno-smtp");
        _enviados.Enqueue((para, assunto, corpoHtml));
        UltimoPara = para;
        UltimoCorpo = corpoHtml;
        return Task.CompletedTask;
    }

    public string? UltimoCodigo => UltimoCorpo is null
        ? null
        : Regex.Match(UltimoCorpo, @"<strong>([^<]+)</strong>").Groups[1].Value;
}

/// <summary>Dublê de <see cref="IFilaEmail"/> que entrega ao <see cref="OtpSink"/> sem esperar.</summary>
public sealed class FilaEmailImediata(OtpSink sink) : IFilaEmail
{
    public void Enfileirar(string para, string assunto, string corpoHtml) =>
        sink.EnviarAsync(para, assunto, corpoHtml, CancellationToken.None).GetAwaiter().GetResult();
}

/// <summary>
/// Factory de testes de autenticação. Herda de <see cref="AppFactory"/> (reusa o SQLite temp,
/// a chave JWT de teste e o mecanismo <c>ConfiguracaoExtra</c>) e acrescenta o <see cref="OtpSink"/>,
/// o seed de utilizadores de teste e um helper de login ponta-a-ponta.
/// </summary>
public class AutenticacaoWebFactory : AppFactory
{
    public const string AdminEmail = "admin@teste.local";
    public const string MembroEmail = "membro@teste.local";
    public const string Senha = "Teste!Forte1";

    public readonly OtpSink Otp = new();

    public AutenticacaoWebFactory()
    {
        ConfiguracaoExtra["Email:Sink"] = "Dev";
        // Rate limit generoso: a fixture é partilhada por vários testes de auth no mesmo servidor.
        ConfiguracaoExtra["AuthRateLimiting:PermitLimit"] = "100000";
        ConfiguracaoExtra["RateLimiting:PermitLimit"] = "100000";
        // Os testes partilham endereços: o limite por endereço não pode interferir.
        ConfiguracaoExtra["EnvioEmail:MaxPorJanela"] = "100000";
        ConfiguracaoExtra["Site:UrlPublica"] = "https://site.teste";
        // Cookies são Secure — o CookieContainer só os devolve sobre https.
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Otp);

            // Fila síncrona: o e-mail chega ao OtpSink na hora, para os testes serem determinísticos.
            services.RemoveAll<IFilaEmail>();
            services.AddSingleton<IFilaEmail>(new FilaEmailImediata(Otp));
        });
    }

    public async Task SemearUsuariosAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var r in new[] { "Admin", "Membro" })
            if (!await roles.RoleExistsAsync(r))
                await roles.CreateAsync(new IdentityRole<Guid>(r));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await GarantirUsuarioAsync(users, AdminEmail, "Admin");
        await GarantirUsuarioAsync(users, MembroEmail, "Membro");
    }

    /// <summary>
    /// Cria o utilizador se não existir; se já existir, repõe a baseline conhecida
    /// (senha, nome, e-mail confirmado, <c>DeveTrocarSenha=false</c>). Assim
    /// <see cref="SemearUsuariosAsync"/> deixa a fixture partilhada num estado
    /// determinístico mesmo que um teste anterior tenha trocado a senha/nome.
    /// </summary>
    private static async Task GarantirUsuarioAsync(UserManager<ApplicationUser> users, string email, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NomeExibicao = role,
                DeveTrocarSenha = false
            };
            await users.CreateAsync(user, Senha);
            await users.AddToRoleAsync(user, role);
            return;
        }

        await users.RemovePasswordAsync(user);
        await users.AddPasswordAsync(user, Senha);
        user.NomeExibicao = role;
        user.EmailConfirmed = true;
        user.DeveTrocarSenha = false;
        await users.UpdateAsync(user);

        if (!await users.IsInRoleAsync(user, role))
            await users.AddToRoleAsync(user, role);
    }

    /// <summary>
    /// Semeia os utilizadores, faz login + 2FA e devolve um <see cref="HttpClient"/> já autenticado
    /// (cookies geridos internamente). Quando <paramref name="comCsrf"/>, o cliente reenvia o valor
    /// do cookie <c>bt_csrf</c> no header <c>X-CSRF-Token</c> em pedidos não-GET.
    /// </summary>
    public async Task<HttpClient> LoginAsync(string email, string senha, bool comCsrf = true)
    {
        await SemearUsuariosAsync();
        var client = CreateDefaultClient(new Uri("https://localhost"), new ClienteAuthHandler(comCsrf));

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, senha });
        login.EnsureSuccessStatusCode();
        var corpo = await login.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var tokenDesafio = corpo!["tokenDesafio"].GetString();

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa",
            new { tokenDesafio, codigo = Otp.UltimoCodigo });
        v.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>Gere cookies manualmente e injeta o header CSRF a partir do cookie <c>bt_csrf</c>.</summary>
    private sealed class ClienteAuthHandler : DelegatingHandler
    {
        private static readonly Uri Alvo = new("https://localhost");
        private readonly CookieContainer _cookies = new();
        private readonly bool _comCsrf;

        public ClienteAuthHandler(bool comCsrf) => _comCsrf = comCsrf;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            // Pelo caminho do pedido, como o navegador: bt_refresh só vale em /api/auth.
            var header = _cookies.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrEmpty(header))
                request.Headers.TryAddWithoutValidation("Cookie", header);

            if (_comCsrf && request.Method != HttpMethod.Get && request.Method != HttpMethod.Head)
            {
                var csrf = _cookies.GetCookies(Alvo)["bt_csrf"]?.Value;
                if (!string.IsNullOrEmpty(csrf))
                    request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrf);
            }

            var res = await base.SendAsync(request, ct);
            if (res.Headers.TryGetValues("Set-Cookie", out var setCookies))
                foreach (var sc in setCookies)
                    _cookies.SetCookies(Alvo, sc);
            return res;
        }
    }
}
