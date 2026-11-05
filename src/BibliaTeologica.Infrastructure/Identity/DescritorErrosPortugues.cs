using Microsoft.AspNetCore.Identity;

namespace BibliaTeologica.Infrastructure.Identity;

public class DescritorErrosPortugues : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Erro(nameof(DefaultError), "Ocorreu um erro inesperado.");
    public override IdentityError ConcurrencyFailure() => Erro(nameof(ConcurrencyFailure), "Os dados foram alterados por outra operação. Tente novamente.");
    public override IdentityError PasswordMismatch() => Erro(nameof(PasswordMismatch), "A senha está incorreta.");
    public override IdentityError InvalidToken() => Erro(nameof(InvalidToken), "O código ou link é inválido ou expirou.");
    public override IdentityError InvalidUserName(string? userName) => Erro(nameof(InvalidUserName), "O nome de usuário é inválido.");
    public override IdentityError InvalidEmail(string? email) => Erro(nameof(InvalidEmail), "O e-mail é inválido.");
    public override IdentityError DuplicateUserName(string userName) => Erro(nameof(DuplicateUserName), "Este nome de usuário já está em uso.");
    public override IdentityError DuplicateEmail(string email) => Erro(nameof(DuplicateEmail), "Este e-mail já está em uso.");
    public override IdentityError PasswordTooShort(int length) => Erro(nameof(PasswordTooShort), $"A senha precisa ter pelo menos {length} caracteres.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Erro(nameof(PasswordRequiresNonAlphanumeric), "A senha precisa ter pelo menos um símbolo.");
    public override IdentityError PasswordRequiresDigit() => Erro(nameof(PasswordRequiresDigit), "A senha precisa ter pelo menos um número.");
    public override IdentityError PasswordRequiresLower() => Erro(nameof(PasswordRequiresLower), "A senha precisa ter pelo menos uma letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Erro(nameof(PasswordRequiresUpper), "A senha precisa ter pelo menos uma letra maiúscula.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Erro(nameof(PasswordRequiresUniqueChars), $"A senha precisa ter pelo menos {uniqueChars} caracteres diferentes.");
    public override IdentityError UserAlreadyHasPassword() => Erro(nameof(UserAlreadyHasPassword), "A conta já tem uma senha.");
    public override IdentityError UserLockoutNotEnabled() => Erro(nameof(UserLockoutNotEnabled), "O bloqueio não está habilitado para esta conta.");
    public override IdentityError UserAlreadyInRole(string role) => Erro(nameof(UserAlreadyInRole), "A conta já tem este perfil.");
    public override IdentityError UserNotInRole(string role) => Erro(nameof(UserNotInRole), "A conta não tem este perfil.");

    private static IdentityError Erro(string codigo, string descricao) => new() { Code = codigo, Description = descricao };
}
