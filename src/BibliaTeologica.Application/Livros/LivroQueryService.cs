using AutoMapper;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public class LivroQueryService : ILivroQueryService
{
    private readonly ILivroRepository _repositorio;
    private readonly IMapper _mapper;

    public LivroQueryService(ILivroRepository repositorio, IMapper mapper)
    {
        _repositorio = repositorio;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct)
    {
        var livros = await _repositorio.ListarPublicadosAsync(ct);
        return livros.Select(_mapper.Map<LivroListItemDto>).ToList();
    }

    public async Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct)
    {
        var livro = await _repositorio.ObterPorSlugAsync(slug, ct);
        return livro is null || !livro.Publicado ? null : _mapper.Map<LivroDetailDto>(livro);
    }
}
