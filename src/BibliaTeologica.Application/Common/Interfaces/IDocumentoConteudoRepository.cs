using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface IDocumentoConteudoRepository
{
    /// <summary>Troca todos os documentos pelos informados, numa única transação.</summary>
    Task SubstituirTudoAsync(IReadOnlyList<DocumentoConteudo> documentos, CancellationToken ct);
    Task<DocumentoConteudo?> ObterAsync(TipoConteudo tipo, string slug, CancellationToken ct);
    Task<IReadOnlyList<DocumentoConteudo>> ListarAsync(TipoConteudo tipo, CancellationToken ct);
}
