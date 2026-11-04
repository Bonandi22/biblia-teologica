using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;

namespace BibliaTeologica.Domain.Entities;

public class DocumentoConteudo
{
    private DocumentoConteudo() { }

    private DocumentoConteudo(TipoConteudo tipo, string slug, string titulo, string resumo, string json)
    {
        Tipo = tipo;
        Slug = slug;
        Titulo = titulo;
        Resumo = resumo;
        Json = json;
        ImportadoEm = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public TipoConteudo Tipo { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public string Resumo { get; private set; } = null!;
    public string Json { get; private set; } = null!;
    public DateTime ImportadoEm { get; private set; }

    public static DocumentoConteudo Criar(TipoConteudo tipo, string slug, string titulo, string resumo, string json)
    {
        if (!Enum.IsDefined(tipo)) throw new DomainException("Tipo de conteúdo inválido.");
        Requer(slug, nameof(slug));
        Requer(titulo, nameof(titulo));
        Requer(json, nameof(json));
        if (resumo is null) throw new DomainException("Campo obrigatório ausente: resumo.");

        return new DocumentoConteudo(tipo, slug.Trim(), titulo.Trim(), resumo, json);

        static void Requer(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new DomainException($"Campo obrigatório ausente: {campo}.");
        }
    }
}
