using System.Text;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Conteudo;
using BibliaTeologica.Infrastructure.Email;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BibliaTeologica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<LivroSeeder>();

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Secao));
        services.Configure<EmailOptions>(config.GetSection(EmailOptions.Secao));
        services.Configure<ConteudoOptions>(config.GetSection(ConteudoOptions.Secao));
        services.AddScoped<IFonteConteudo, FonteConteudoArquivos>();

        var jwt = config.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey em falta ou com menos de 32 caracteres.");

        var maxTentativas = config.GetValue<int?>("Identity:Lockout:MaxTentativas") ?? 5;
        var lockoutMinutos = config.GetValue<int?>("Identity:Lockout:Minutos") ?? 15;

        services.AddDataProtection();

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = true;
                o.Lockout.MaxFailedAccessAttempts = maxTentativas;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutos);
                o.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    RoleClaimType = "role",
                    NameClaimType = "name",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("bt_access", out var cookie))
                            ctx.Token = cookie;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        var sink = config.GetValue<string>("Email:Sink") ?? "Dev";
        if (string.Equals(sink, "Smtp", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        else
            services.AddScoped<IEmailSender, LogEmailSender>();

        var admins = config.GetSection(AdminSeedOptions.Secao).Get<List<AdminSeedEntrada>>() ?? new();
        services.AddScoped<IdentitySeeder>(sp => new IdentitySeeder(
            sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
            sp.GetRequiredService<UserManager<ApplicationUser>>(),
            admins,
            sp.GetRequiredService<ILogger<IdentitySeeder>>()));

        return services;
    }
}
