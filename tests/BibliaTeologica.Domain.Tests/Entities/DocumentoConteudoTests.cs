using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Entities;

public class DocumentoConteudoTests
{
    [Fact]
    public void Criar_LancaDomainException_QuandoSlugVazio()
    {
        var acao = () => DocumentoConteudo.Criar(TipoConteudo.Livro, " ", "Isaías", "resumo", "{}");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_LancaDomainException_QuandoJsonVazio()
    {
        var acao = () => DocumentoConteudo.Criar(TipoConteudo.Livro, "isaias", "Isaías", "resumo", "");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_DefineImportadoEmUtc_QuandoValido()
    {
        var antes = DateTime.UtcNow;
        var doc = DocumentoConteudo.Criar(TipoConteudo.Livro, "isaias", "Isaías", "resumo", "{}");

        doc.ImportadoEm.Kind.Should().Be(DateTimeKind.Utc);
        doc.ImportadoEm.Should().BeOnOrAfter(antes);
        doc.Tipo.Should().Be(TipoConteudo.Livro);
        doc.Slug.Should().Be("isaias");
    }
}
