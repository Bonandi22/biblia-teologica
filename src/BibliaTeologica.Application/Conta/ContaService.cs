using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta.Dtos;

namespace BibliaTeologica.Application.Conta;

public class ContaService : IContaService
{
    private readonly IIdentityService _identity;
    private readonly IEmissorTokens _emissor;
    private readonly IRefreshTokenStore _store;

    public ContaService(IIdentityService identity, IEmissorTokens emissor, IRefreshTokenStore store)
    {
        _identity = identity;
        _emissor = emissor;
        _store = store;
    }

    public async Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct)
    {
        var u = await _identity.ObterUsuarioAsync(userId, ct);
        if (u is null) return null;
        var criadoEm = await _identity.ObterCriadoEmAsync(userId, ct) ?? DateTime.UtcNow;
        return new PerfilDto(u.Email, u.NomeExibicao, u.Roles, criadoEm);
    }

    public Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct)
        => _identity.AtualizarNomeAsync(userId, dto.NomeExibicao, ct);

    public Task<IdentityResultDto> TrocarEmailAsync(Guid userId, TrocarEmailDto dto, CancellationToken ct)
        => _identity.TrocarEmailAsync(userId, dto.NovoEmail, dto.Senha, ct);

    public async Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct)
    {
        var r = await _identity.TrocarSenhaAsync(userId, dto.SenhaAtual, dto.NovaSenha, ct);
        if (!r.Sucesso)
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas, Erros: r.Erros);

        // Antes de emitir: encerra as outras sessões e preserva a nova, desta sessão.
        await _store.RevogarCadeiaAsync(userId, ct);
        return await _emissor.EmitirAsync(userId, ip, ct);
    }
}
