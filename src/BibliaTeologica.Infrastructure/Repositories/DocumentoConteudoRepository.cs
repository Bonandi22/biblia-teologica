using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Repositories;

public class DocumentoConteudoRepository : IDocumentoConteudoRepository
{
    private readonly AppDbContext _db;

    public DocumentoConteudoRepository(AppDbContext db) => _db = db;

    public async Task SubstituirTudoAsync(IReadOnlyList<DocumentoConteudo> documentos, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await _db.DocumentosConteudo.ExecuteDeleteAsync(ct);
        _db.DocumentosConteudo.AddRange(documentos);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public Task<DocumentoConteudo?> ObterAsync(TipoConteudo tipo, string slug, CancellationToken ct)
        => _db.DocumentosConteudo.AsNoTracking().FirstOrDefaultAsync(d => d.Tipo == tipo && d.Slug == slug, ct);

    public async Task<IReadOnlyList<DocumentoConteudo>> ListarAsync(TipoConteudo tipo, CancellationToken ct)
        => await _db.DocumentosConteudo.AsNoTracking()
            .Where(d => d.Tipo == tipo)
            .OrderBy(d => d.Slug)
            .ToListAsync(ct);
}
