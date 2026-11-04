using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Identity;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;

    public RefreshTokenStore(AppDbContext db) => _db = db;

    public async Task GuardarAsync(Guid userId, string hash, DateTime expiraEm, string? ip, CancellationToken ct)
    {
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = hash,
            ExpiraEm = expiraEm,
            CriadoEm = DateTime.UtcNow,
            CriadoPorIp = ip
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RefreshTokenInfo?> ObterAsync(string hash, CancellationToken ct)
    {
        var t = await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        return t is null ? null : new RefreshTokenInfo(t.UsuarioId, t.TokenHash, t.ExpiraEm, t.RevogadoEm, t.SubstituidoPorHash is not null);
    }

    public async Task RotacionarAsync(string hashAntigo, Guid userId, string hashNovo,
        DateTime expira, string? ip, CancellationToken ct)
    {
        var antigo = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hashAntigo, ct);
        if (antigo is { RevogadoEm: null })
        {
            antigo.RevogadoEm = DateTime.UtcNow;
            antigo.SubstituidoPorHash = hashNovo;
        }
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = hashNovo,
            ExpiraEm = expira,
            CriadoEm = DateTime.UtcNow,
            CriadoPorIp = ip
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevogarAsync(string hash, CancellationToken ct)
    {
        var t = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (t is { RevogadoEm: null })
        {
            t.RevogadoEm = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevogarCadeiaAsync(Guid userId, CancellationToken ct)
    {
        var ativos = await _db.RefreshTokens
            .Where(x => x.UsuarioId == userId && x.RevogadoEm == null)
            .ToListAsync(ct);
        foreach (var t in ativos) t.RevogadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
