using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public interface ILivroQueryService
{
    Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct);
    Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct);
}
