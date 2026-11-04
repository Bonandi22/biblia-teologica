using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Application.Conteudo.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/estudos")]
public class EstudosController : ControllerBase
{
    private readonly IConteudoQueryService _conteudo;

    public EstudosController(IConteudoQueryService conteudo) => _conteudo = conteudo;

    // Público: estudos editoriais publicados, só leitura, sem dados sensíveis. Sem [Authorize] por design (ver CLAUDE.md).
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EstudoResumoDto>>> Listar(CancellationToken ct)
        => Ok(await _conteudo.ListarEstudosAsync(ct));

    // Público: mesma justificativa da listagem.
    [HttpGet("{slug:regex(^[[a-z0-9-]]{{1,120}}$)}")]
    public async Task<ActionResult<EstudoDto>> ObterPorSlug(string slug, CancellationToken ct)
    {
        var estudo = await _conteudo.ObterEstudoAsync(slug, ct);
        return estudo is null ? NotFound() : Ok(estudo);
    }
}
