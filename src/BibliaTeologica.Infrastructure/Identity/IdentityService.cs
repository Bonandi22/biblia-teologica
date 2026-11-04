using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;

namespace BibliaTeologica.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _users;

    public IdentityService(UserManager<ApplicationUser> users) => _users = users;

    // Sem SignInManager: ele zera as falhas com a senha certa, e quem tem a senha poderia
    // errar o código 2FA para sempre sem bloquear. As falhas só zeram depois do 2FA.
    public async Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null) return new ResultadoSenha(false, false, null);
        if (await _users.IsLockedOutAsync(user)) return new ResultadoSenha(false, true, null);
        if (await _users.CheckPasswordAsync(user, senha)) return new ResultadoSenha(true, false, user.Id);

        if (lockoutOnFailure) await _users.AccessFailedAsync(user);
        return new ResultadoSenha(false, await _users.IsLockedOutAsync(user), null);
    }

    public async Task<Guid?> ResolverIdPorEmailAsync(string email, CancellationToken ct)
        => (await _users.FindByEmailAsync(email))?.Id;

    public async Task<string> GerarTokenEmailAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Utilizador não encontrado.");
        return await _users.GenerateTwoFactorTokenAsync(user, "Email");
    }

    public async Task<bool> VerificarTokenEmailAsync(Guid userId, string codigo, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null || await _users.IsLockedOutAsync(user)) return false;
        if (!await _users.VerifyTwoFactorTokenAsync(user, "Email", codigo)) return false;

        // O código deriva do security stamp: trocá-lo invalida o código recém-usado.
        await _users.UpdateSecurityStampAsync(user);
        return true;
    }

    public async Task RegistarFalhaAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is not null) await _users.AccessFailedAsync(user);
    }

    public async Task ResetarFalhasAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is not null) await _users.ResetAccessFailedCountAsync(user);
    }

    public async Task<UsuarioDto?> ObterUsuarioAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var roles = await _users.GetRolesAsync(user);
        return new UsuarioDto(user.Email!, user.NomeExibicao, roles.ToList(), user.DeveTrocarSenha);
    }

    public async Task<string?> ObterEmailAsync(Guid userId, CancellationToken ct)
        => (await _users.FindByIdAsync(userId.ToString()))?.Email;

    public async Task<DateTime?> ObterCriadoEmAsync(Guid userId, CancellationToken ct)
        => (await _users.FindByIdAsync(userId.ToString()))?.CriadoEm;

    public async Task<IdentityResultDto> CriarMembroAsync(string email, string senha, string nomeExibicao, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NomeExibicao = nomeExibicao,
            DeveTrocarSenha = false
        };
        var criado = await _users.CreateAsync(user, senha);
        if (!criado.Succeeded) return Mapear(criado);
        await _users.AddToRoleAsync(user, IdentitySeeder.RoleMembro);
        return IdentityResultDto.Ok();
    }

    public async Task<IdentityResultDto> TrocarSenhaAsync(Guid userId, string atual, string nova, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityResultDto.Falha("Utilizador não encontrado.");
        var r = await _users.ChangePasswordAsync(user, atual, nova);
        if (r.Succeeded && user.DeveTrocarSenha)
        {
            user.DeveTrocarSenha = false;
            await _users.UpdateAsync(user);
        }
        return Mapear(r);
    }

    public async Task<IdentityResultDto> TrocarEmailAsync(Guid userId, string novoEmail, string senha, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityResultDto.Falha("Utilizador não encontrado.");
        if (!await _users.CheckPasswordAsync(user, senha))
            return IdentityResultDto.Falha("Senha incorreta.");

        var token = await _users.GenerateChangeEmailTokenAsync(user, novoEmail);
        var r = await _users.ChangeEmailAsync(user, novoEmail, token);
        if (r.Succeeded)
        {
            await _users.SetUserNameAsync(user, novoEmail);
        }
        return Mapear(r);
    }

    public async Task AtualizarNomeAsync(Guid userId, string nomeExibicao, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return;
        user.NomeExibicao = nomeExibicao;
        await _users.UpdateAsync(user);
    }

    public async Task LimparDeveTrocarSenhaAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is { DeveTrocarSenha: true })
        {
            user.DeveTrocarSenha = false;
            await _users.UpdateAsync(user);
        }
    }

    private static IdentityResultDto Mapear(IdentityResult r)
        => r.Succeeded ? IdentityResultDto.Ok() : IdentityResultDto.Falha(r.Errors.Select(e => e.Description).ToArray());
}
