using System.Threading.RateLimiting;
using BibliaTeologica.Api.Auth;
using BibliaTeologica.Api.Middleware;
using BibliaTeologica.Application;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Infrastructure;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();
builder.Services.AddSingleton<CookieWriter>();
builder.Services.AddScoped<CsrfActionFilter>();

// Claims do JWT são consumidas com os nomes crus ("sub", "email", "name", "role",
// "deve_trocar_senha") — desligar o mapeamento herdado do JwtSecurityTokenHandler.
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
    options => options.MapInboundClaims = false);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var origensCors = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddPolicy("spa", policy =>
    policy.WithOrigins(origensCors).AllowAnyHeader().AllowAnyMethod()));

var permitLimit = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 100;
var windowSeconds = builder.Configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
var authLimit = builder.Configuration.GetValue<int?>("AuthRateLimiting:PermitLimit") ?? 10;
var authWindow = builder.Configuration.GetValue<int?>("AuthRateLimiting:WindowSeconds") ?? 60;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0
            }));
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authLimit,
            Window = TimeSpan.FromSeconds(authWindow),
            QueueLimit = 0
        }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<LivroSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();

    // Rascunhos só entram fora de Production, mesmo que a config peça.
    var incluirRascunhos = builder.Configuration.GetValue<bool>("Conteudo:IncluirRascunhos")
                           && !app.Environment.IsProduction();
    var logImportacao = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ImportacaoConteudo");
    try
    {
        await scope.ServiceProvider.GetRequiredService<ImportadorConteudo>().ImportarAsync(incluirRascunhos, CancellationToken.None);
    }
    catch (ConteudoInvalidoException ex)
    {
        foreach (var erro in ex.Erros)
            logImportacao.LogError("Conteúdo inválido: {Erro}", erro);
        throw;
    }
    catch (Exception ex)
    {
        logImportacao.LogError(ex, "Falha ao importar o conteúdo.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
