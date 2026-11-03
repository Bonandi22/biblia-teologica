using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Application.Conteudo.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/panoramas")]
public class PanoramasController : ControllerBase
{
    private readonly IConteudoQueryService _conteudo;

    public PanoramasController(IConteudoQueryService conteudo) => _conteudo = conteudo;

    // Público: só os slugs dos livros com panorama publicado, para marcar o índice. Sem [Authorize] por design (ver CLAUDE.md).
    [HttpGet("livros")]
    public async Task<ActionResult<IReadOnlyList<string>>> ListarLivros(CancellationToken ct)
        => Ok(await _conteudo.ListarLivrosComPanoramaAsync(ct));

    // Público: conteúdo editorial publicado, só leitura, sem dados sensíveis. Sem [Authorize] por design (ver CLAUDE.md).
    [HttpGet("livros/{slug:regex(^[[a-z0-9-]]{{1,120}}$)}")]
    public async Task<ActionResult<LivroPanoramaDto>> ObterPorLivro(string slug, CancellationToken ct)
    {
        var panorama = await _conteudo.ObterPanoramaAsync(slug, ct);
        return panorama is null ? NotFound() : Ok(panorama);
    }
}
