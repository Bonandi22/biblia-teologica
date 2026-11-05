using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    // Hash com o mesmo custo do hasher padrão, calculado uma vez; a senha por trás dele é irrelevante.
    private static readonly Lazy<string> HashDescartavel =
        new(() => new PasswordHasher<ApplicationUser>().HashPassword(new ApplicationUser(), Guid.NewGuid().ToString("N")));

    private readonly UserManager<ApplicationUser> _users;

    private readonly ILogger<IdentityService>? _log;

    public IdentityService(UserManager<ApplicationUser> users, ILogger<IdentityService>? log = null)
    {
        _users = users;
        _log = log;
    }

    // Sem SignInManager: ele zera as falhas com a senha certa, e quem tem a senha poderia
    // errar o código 2FA para sempre sem bloquear. As falhas só zeram depois do 2FA.
    public async Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null)
        {
            // Hash descartável: o tempo fica igual ao de uma senha errada numa conta real.
            _users.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), HashDescartavel.Value, senha);
            return new ResultadoSenha(false, false, null);
        }
        if (await _users.IsLockedOutAsync(user)) return new ResultadoSenha(false, true, null);
        if (await _users.CheckPasswordAsync(user, senha)) return new ResultadoSenha(true, false, user.Id);

        if (lockoutOnFailure) await _users.AccessFailedAsync(user);
        return new ResultadoSenha(false, await _users.IsLockedOutAsync(user), null);
    }

    public async Task<bool> EstaBloqueadoAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        return user is not null && await _users.IsLockedOutAsync(user);
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

        // O código chegou ao endereço, logo ele é do dono da conta.
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _users.UpdateAsync(user);
        }
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

    public async Task<IdentityResultDto> ValidarSenhaAsync(string senha, CancellationToken ct)
    {
        var erros = new List<string>();
        var modelo = new ApplicationUser();
        foreach (var validador in _users.PasswordValidators)
        {
            var r = await validador.ValidateAsync(_users, modelo, senha);
            if (!r.Succeeded) erros.AddRange(r.Errors.Select(e => e.Description));
        }
        return erros.Count == 0 ? IdentityResultDto.Ok() : IdentityResultDto.Falha(erros.ToArray());
    }

    public async Task<ResultadoCriacao> CriarMembroAsync(NovoMembro dados, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dados.Email,
            Email = dados.Email,
            EmailConfirmed = false,
            PhoneNumber = dados.Telefone,
            NomeExibicao = dados.NomeExibicao,
            DeveTrocarSenha = false,
            TermoVersao = dados.TermoVersao,
            TermoAceitoEm = dados.TermoAceitoEm
        };
        IdentityResult criado;
        try
        {
            criado = await _users.CreateAsync(user, dados.Senha);
        }
        catch (DbUpdateException ex)
        {
            // Cadastro concorrente do mesmo e-mail: o índice único venceu a validação.
            _log?.LogWarning(ex, "Violação de unicidade no cadastro; tratada como e-mail já existente.");
            return new ResultadoCriacao(null, true, Array.Empty<string>());
        }
        if (!criado.Succeeded)
        {
            if (criado.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                    or nameof(IdentityErrorDescriber.DuplicateUserName)))
            {
                // Sem hash extra: CreateAsync já calculou o hash da senha antes de checar a duplicidade.
                return new ResultadoCriacao(null, true, Array.Empty<string>());
            }
            return new ResultadoCriacao(null, false, criado.Errors.Select(e => e.Description).ToArray());
        }
        await _users.AddToRoleAsync(user, IdentitySeeder.RoleMembro);
        return new ResultadoCriacao(user.Id, false, Array.Empty<string>());
    }

    public async Task<string> GerarTokenRedefinicaoAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Utilizador não encontrado.");
        return await _users.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<ResultadoRedefinicao> RedefinirSenhaAsync(string email, string token, string novaSenha, CancellationToken ct)
    {
        var invalido = new ResultadoRedefinicao(null, true, Array.Empty<string>());
        var user = await _users.FindByEmailAsync(email);
        if (user is null) return invalido;

        // Token antes da senha: só quem tem um link válido descobre a regra de senha.
        if (!await _users.VerifyUserTokenAsync(user, _users.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token))
            return invalido;

        var r = await _users.ResetPasswordAsync(user, token, novaSenha);
        if (!r.Succeeded)
            return new ResultadoRedefinicao(user.Id, false, r.Errors.Select(e => e.Description).ToArray());

        // O link chegou ao endereço, logo ele é do dono da conta.
        user.EmailConfirmed = true;
        user.DeveTrocarSenha = false;
        await _users.UpdateAsync(user);
        await _users.SetLockoutEndDateAsync(user, null);
        await _users.ResetAccessFailedCountAsync(user);
        return new ResultadoRedefinicao(user.Id, false, Array.Empty<string>());
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

    public async Task<ResultadoSenha> ConferirSenhaAsync(Guid userId, string senha, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return new ResultadoSenha(false, false, null);
        if (await _users.IsLockedOutAsync(user)) return new ResultadoSenha(false, true, null);
        if (await _users.CheckPasswordAsync(user, senha)) return new ResultadoSenha(true, false, user.Id);

        await _users.AccessFailedAsync(user);
        return new ResultadoSenha(false, await _users.IsLockedOutAsync(user), null);
    }

    // A finalidade carrega o endereço: um código pedido para A não vale para B.
    private string FinalidadeTrocaEmail(string novoEmail) => "TrocarEmail:" + _users.NormalizeEmail(novoEmail.Trim());

    public async Task<string> GerarCodigoTrocaEmailAsync(Guid userId, string novoEmail, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Utilizador não encontrado.");
        return await _users.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, FinalidadeTrocaEmail(novoEmail));
    }

    public async Task<IdentityResultDto> ConfirmarTrocaEmailAsync(Guid userId, string novoEmail, string codigo, CancellationToken ct)
    {
        var invalido = IdentityResultDto.Falha("Código inválido ou vencido.");
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null || await _users.IsLockedOutAsync(user)) return invalido;

        novoEmail = novoEmail.Trim();
        if (!await _users.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, FinalidadeTrocaEmail(novoEmail), codigo))
        {
            await _users.AccessFailedAsync(user);
            return invalido;
        }

        // Uma só gravação: ou a conta troca inteira ou fica como estava. O código chegou ao novo
        // endereço, logo ele é do dono. UpdateAsync valida e recalcula os nomes normalizados; o
        // stamp novo invalida o código usado e os demais ligados ao stamp.
        user.Email = novoEmail;
        user.UserName = novoEmail;
        user.EmailConfirmed = true;
        user.SecurityStamp = Guid.NewGuid().ToString();
        var r = await _users.UpdateAsync(user);
        if (!r.Succeeded)
        {
            // Endereço ocupado no meio-tempo (ou inválido): o mesmo erro genérico, sem dizer o motivo.
            await _users.AccessFailedAsync(user);
            return invalido;
        }

        return IdentityResultDto.Ok();
    }

    public async Task AtualizarPerfilAsync(Guid userId, string nomeExibicao, string? telefone, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return;
        user.NomeExibicao = nomeExibicao;
        if (user.PhoneNumber != telefone)
        {
            user.PhoneNumber = telefone;
            user.PhoneNumberConfirmed = false;
        }
        await _users.UpdateAsync(user);
    }

    public async Task<string?> ObterTelefoneAsync(Guid userId, CancellationToken ct)
        => (await _users.FindByIdAsync(userId.ToString()))?.PhoneNumber;

    public async Task<bool> EhUltimoAdminAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null || !await _users.IsInRoleAsync(user, IdentitySeeder.RoleAdmin)) return false;
        var admins = await _users.GetUsersInRoleAsync(IdentitySeeder.RoleAdmin);
        return !admins.Any(a => a.Id != userId);
    }

    public async Task<IdentityResultDto> ApagarUsuarioAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityResultDto.Falha("Utilizador não encontrado.");
        // Os refresh tokens saem junto, pela FK em cascata.
        return Mapear(await _users.DeleteAsync(user));
    }

    public async Task<int> ApagarNaoConfirmadasAsync(DateTime criadasAntesDe, CancellationToken ct)
    {
        var velhas = await _users.Users
            .Where(u => !u.EmailConfirmed && u.TermoAceitoEm != null && u.CriadoEm < criadasAntesDe)
            .ToListAsync(ct);
        var apagadas = 0;
        foreach (var user in velhas)
        {
            // Os refresh tokens saem junto, pela FK em cascata.
            if ((await _users.DeleteAsync(user)).Succeeded) apagadas++;
        }
        return apagadas;
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
