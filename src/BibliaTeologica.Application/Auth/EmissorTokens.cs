using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;

namespace BibliaTeologica.Application.Auth;

public class EmissorTokens : IEmissorTokens
{
    private readonly IIdentityService _identity;
    private readonly ITokenService _token;
    private readonly IRefreshTokenStore _store;

    public EmissorTokens(IIdentityService identity, ITokenService token, IRefreshTokenStore store)
    {
        _identity = identity;
        _token = token;
        _store = store;
    }

    public async Task<ResultadoLoginDto> EmitirAsync(Guid userId, string? ip, CancellationToken ct)
    {
        var usuario = await _identity.ObterUsuarioAsync(userId, ct)
            ?? throw new InvalidOperationException("Utilizador não encontrado ao emitir tokens.");

        var access = _token.GerarAccessToken(userId, usuario.Email, usuario.NomeExibicao,
            usuario.Roles, usuario.DeveTrocarSenha);
        var (refreshCru, hash, refreshExp) = _token.GerarRefreshToken();
        await _store.GuardarAsync(userId, hash, refreshExp, ip, ct);

        return new ResultadoLoginDto(EstadoLogin.Autenticado, Usuario: usuario,
            Tokens: new TokensGerados(access, DateTime.UtcNow.AddMinutes(15), refreshCru, refreshExp,
                Guid.NewGuid().ToString("N")));
    }
}
