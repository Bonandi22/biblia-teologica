using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Application.Conteudo;

/// <summary>
/// Importação idempotente: lê os arquivos, valida tudo e só então substitui os documentos no banco.
/// Qualquer erro de leitura ou validação aborta antes de tocar no banco.
/// </summary>
public sealed class ImportadorConteudo
{
    private readonly IFonteConteudo _fonte;
    private readonly ILivroRepository _livros;
    private readonly IDocumentoConteudoRepository _documentos;
    private readonly ILogger<ImportadorConteudo> _logger;

    public ImportadorConteudo(
        IFonteConteudo fonte,
        ILivroRepository livros,
        IDocumentoConteudoRepository documentos,
        ILogger<ImportadorConteudo> logger)
    {
        _fonte = fonte;
        _livros = livros;
        _documentos = documentos;
        _logger = logger;
    }

    /// <exception cref="ConteudoInvalidoException">Arquivo ilegível ou conteúdo inválido (todos os erros).</exception>
    public async Task ImportarAsync(bool incluirRascunhos, CancellationToken ct)
    {
        var conjunto = await _fonte.LerAsync(incluirRascunhos, ct);
        var livros = await _livros.ListarPublicadosAsync(ct);

        var erros = ValidadorConteudo.Validar(conjunto, livros.Select(l => new LivroCanonico(l.Slug, l.Ordem)).ToList());
        if (erros.Count > 0)
            throw new ConteudoInvalidoException(erros);

        var titulos = livros.ToDictionary(l => l.Slug, l => l.Nome, StringComparer.Ordinal);
        var docs = ConstrutorDocumentos.Construir(conjunto, titulos);
        await _documentos.SubstituirTudoAsync(docs, ct);

        _logger.LogInformation("Conteúdo importado: {Livros} livro(s), {Estudos} estudo(s).",
            conjunto.Livros.Count, conjunto.Estudos.Count);
    }
}
