using BibliaTeologica.Application.Livros;
using BibliaTeologica.Application.Livros.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/livros")]
public class LivrosController : ControllerBase
{
    private readonly ILivroQueryService _livros;

    public LivrosController(ILivroQueryService livros) => _livros = livros;

    // Público: catálogo de conteúdo, sem dados sensíveis. Sem [Authorize] por design (ver CLAUDE.md).
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LivroListItemDto>>> Listar(CancellationToken ct)
        => Ok(await _livros.ListarAsync(ct));

    [HttpGet("{slug}")]
    public async Task<ActionResult<LivroDetailDto>> ObterPorSlug(string slug, CancellationToken ct)
    {
        var livro = await _livros.ObterPorSlugAsync(slug, ct);
        return livro is null ? NotFound() : Ok(livro);
    }
}
