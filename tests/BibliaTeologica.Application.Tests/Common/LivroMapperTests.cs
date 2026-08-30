using AwesomeAssertions;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Xunit;

namespace BibliaTeologica.Application.Tests.Common;

public class LivroMapperTests
{
    private static Livro CriarLivro(SecaoCanonica secao = SecaoCanonica.Pentateuco)
        => Livro.Create("GÊNESIS", secao, 1,
            "quem", "porque", "paraQuem", "estrutura", "tema",
            new[] { "ideia 1", "ideia 2" }, "Gn 1:1");

    [Fact]
    public void ToListItem_ProjetaSlugNomeOrdemESecao()
    {
        var dto = LivroMapper.ToListItem(CriarLivro());

        dto.Slug.Should().Be("genesis");
        dto.Nome.Should().Be("GÊNESIS");
        dto.Ordem.Should().Be(1);
        dto.Secao.Should().Be("Pentateuco");
        dto.SecaoRotulo.Should().Be("PENTATEUCO");
    }

    [Fact]
    public void ToDetail_CopiaTodosOsCamposDeTexto()
    {
        var dto = LivroMapper.ToDetail(CriarLivro());

        dto.Slug.Should().Be("genesis");
        dto.Nome.Should().Be("GÊNESIS");
        dto.Ordem.Should().Be(1);
        dto.Secao.Should().Be("Pentateuco");
        dto.SecaoRotulo.Should().Be("PENTATEUCO");
        dto.Testamento.Should().Be("Antigo");
        dto.Quem.Should().Be("quem");
        dto.Porque.Should().Be("porque");
        dto.ParaQuem.Should().Be("paraQuem");
        dto.Estrutura.Should().Be("estrutura");
        dto.Tema.Should().Be("tema");
        dto.Versiculo.Should().Be("Gn 1:1");
        dto.Ideias.Should().Equal("ideia 1", "ideia 2");
    }

    [Fact]
    public void ToDetail_DerivaTestamentoNovoParaSecaoDoNovoTestamento()
    {
        var dto = LivroMapper.ToDetail(CriarLivro(SecaoCanonica.Evangelhos));

        dto.Secao.Should().Be("Evangelhos");
        dto.SecaoRotulo.Should().Be("EVANGELHOS");
        dto.Testamento.Should().Be("Novo");
    }
}
