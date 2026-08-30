using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Persistence.Seed;

public class IdentitySeeder
{
    public const string RoleAdmin = "Admin";
    public const string RoleMembro = "Membro";

    private readonly RoleManager<IdentityRole<Guid>> _roles;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IReadOnlyList<AdminSeedEntrada> _admins;
    private readonly ILogger<IdentitySeeder> _log;

    public IdentitySeeder(RoleManager<IdentityRole<Guid>> roles, UserManager<ApplicationUser> users,
        IEnumerable<AdminSeedEntrada> admins, ILogger<IdentitySeeder> log)
    {
        _roles = roles;
        _users = users;
        _admins = admins.ToList();
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        foreach (var role in new[] { RoleAdmin, RoleMembro })
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole<Guid>(role));

        foreach (var entrada in _admins)
        {
            if (string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.SenhaInicial))
            {
                _log.LogWarning("Entrada de AdminSeed ignorada (email ou senha vazios).");
                continue;
            }
            if (await _users.FindByEmailAsync(entrada.Email) is not null) continue;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = entrada.Email,
                Email = entrada.Email,
                EmailConfirmed = true,
                NomeExibicao = "Administrador",
                DeveTrocarSenha = true
            };
            var criado = await _users.CreateAsync(user, entrada.SenhaInicial);
            if (!criado.Succeeded)
            {
                _log.LogError("Falha ao criar admin seed {Email}: {Erros}", entrada.Email,
                    string.Join("; ", criado.Errors.Select(e => e.Description)));
                continue;
            }
            await _users.AddToRoleAsync(user, RoleAdmin);
            _log.LogInformation("Admin seed criado: {Email}", entrada.Email);
        }
    }
}
