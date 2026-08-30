using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public class LivroQueryService : ILivroQueryService
{
    private readonly ILivroRepository _repositorio;

    public LivroQueryService(ILivroRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct)
    {
        var livros = await _repositorio.ListarPublicadosAsync(ct);
        return livros.Select(LivroMapper.ToListItem).ToList();
    }

    public async Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct)
    {
        var livro = await _repositorio.ObterPorSlugAsync(slug, ct);
        return livro is null || !livro.Publicado ? null : LivroMapper.ToDetail(livro);
    }
}
