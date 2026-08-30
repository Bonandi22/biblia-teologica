using BibliaTeologica.Domain.Entities;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface ILivroRepository
{
    Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct);
    Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct);
}
