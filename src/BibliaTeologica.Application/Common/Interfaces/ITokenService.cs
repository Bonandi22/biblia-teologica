namespace BibliaTeologica.Application.Common.Interfaces;

public interface ITokenService
{
    string GerarAccessToken(Guid userId, string email, string nome,
        IEnumerable<string> roles, bool deveTrocarSenha);

    (string tokenCru, string hash, DateTime expiraEm) GerarRefreshToken();

    string Hash(string tokenCru);

    /// <summary>Token curto assinado que prova que o passo 1 do login passou.</summary>
    string GerarDesafioLogin(Guid userId);

    /// <summary>Devolve o userId se o desafio for válido e não expirado; senão null.</summary>
    Guid? ValidarDesafioLogin(string token);
}
