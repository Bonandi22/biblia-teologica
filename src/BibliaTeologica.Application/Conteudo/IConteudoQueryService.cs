using BibliaTeologica.Application.Conteudo.Dtos;

namespace BibliaTeologica.Application.Conteudo;

public interface IConteudoQueryService
{
    Task<LivroPanoramaDto?> ObterPanoramaAsync(string slug, CancellationToken ct);
    Task<IReadOnlyList<EstudoResumoDto>> ListarEstudosAsync(CancellationToken ct);
    Task<EstudoDto?> ObterEstudoAsync(string slug, CancellationToken ct);
}
