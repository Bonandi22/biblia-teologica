using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task GuardarAsync(Guid userId, string hash, DateTime expiraEm, string? ip, CancellationToken ct);

    Task<RefreshTokenInfo?> ObterAsync(string hash, CancellationToken ct);

    /// <summary>Revoga o hash antigo, encadeia-o ao novo e guarda o novo.</summary>
    Task RotacionarAsync(string hashAntigo, Guid userId, string hashNovo, DateTime expira,
        string? ip, CancellationToken ct);

    Task RevogarAsync(string hash, CancellationToken ct);

    Task RevogarCadeiaAsync(Guid userId, CancellationToken ct);
}
