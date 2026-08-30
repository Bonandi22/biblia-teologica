using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Repositories;

public class LivroRepository : ILivroRepository
{
    private readonly AppDbContext _db;

    public LivroRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct)
        => await _db.Livros.AsNoTracking()
            .Where(l => l.Publicado)
            .OrderBy(l => l.Ordem)
            .ToListAsync(ct);

    public Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct)
        => _db.Livros.AsNoTracking().FirstOrDefaultAsync(l => l.Slug == slug, ct);
}
