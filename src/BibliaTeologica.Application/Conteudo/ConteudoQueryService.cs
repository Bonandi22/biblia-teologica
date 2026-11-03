using System.Text.Json;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conteudo.Dtos;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Conteudo;

public sealed class ConteudoQueryService : IConteudoQueryService
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    private readonly IDocumentoConteudoRepository _documentos;

    public ConteudoQueryService(IDocumentoConteudoRepository documentos) => _documentos = documentos;

    public async Task<LivroPanoramaDto?> ObterPanoramaAsync(string slug, CancellationToken ct)
    {
        var doc = await _documentos.ObterAsync(TipoConteudo.Livro, slug, ct);
        return doc is null ? null : JsonSerializer.Deserialize<LivroPanoramaDto>(doc.Json, OpcoesJson);
    }

    public async Task<IReadOnlyList<string>> ListarLivrosComPanoramaAsync(CancellationToken ct)
    {
        var docs = await _documentos.ListarAsync(TipoConteudo.Livro, ct);
        return docs.Select(d => d.Slug).ToList();
    }

    public async Task<IReadOnlyList<EstudoResumoDto>> ListarEstudosAsync(CancellationToken ct)
    {
        var docs = await _documentos.ListarAsync(TipoConteudo.Estudo, ct);
        return docs.Select(d => new EstudoResumoDto(d.Slug, d.Titulo, d.Resumo)).ToList();
    }

    public async Task<EstudoDto?> ObterEstudoAsync(string slug, CancellationToken ct)
    {
        var doc = await _documentos.ObterAsync(TipoConteudo.Estudo, slug, ct);
        return doc is null ? null : JsonSerializer.Deserialize<EstudoDto>(doc.Json, OpcoesJson);
    }
}
