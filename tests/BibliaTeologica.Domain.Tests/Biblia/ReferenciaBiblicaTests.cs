using AwesomeAssertions;
using BibliaTeologica.Domain.Biblia;
using BibliaTeologica.Domain.Slugs;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Biblia;

public class ReferenciaBiblicaTests
{
    [Theory]
    [InlineData("Isaías 7:14", "isaias")]
    [InlineData("1 Pedro 2:22–25", "1-pedro")]
    [InlineData("1 Pedro 2:22-25", "1-pedro")]
    [InlineData("Isaías 36–37", "isaias")]
    [InlineData("Isaías 36-37", "isaias")]
    [InlineData("Isaías 40", "isaias")]
    [InlineData("Cantares 2:1", "cantares-de-salomao")]
    [InlineData("Salmo 23", "salmos")]
    public void TentarLer_RetornaSlug_QuandoReferenciaValida(string texto, string slugEsperado)
    {
        var ok = ReferenciaBiblica.TentarLer(texto, out var slug);

        ok.Should().BeTrue();
        slug.Should().Be(slugEsperado);
    }

    [Theory]
    [InlineData("Hezequias 1")]
    [InlineData("Isaías")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Isaías 7:")]
    [InlineData("Isaías x:14")]
    public void TentarLer_RetornaFalso_QuandoReferenciaInvalida(string texto)
    {
        var ok = ReferenciaBiblica.TentarLer(texto, out var slug);

        ok.Should().BeFalse();
        slug.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Isaías 52:13–53:12")]
    [InlineData("Isaías 52:13-53:12")]
    public void TentarLer_Aceita_QuandoIntervaloEntreCapitulos(string texto)
    {
        ReferenciaBiblica.TentarLer(texto, out var slug).Should().BeTrue();
        slug.Should().Be("isaias");
    }

    [Theory]
    [InlineData("Isaías 0:1")]
    [InlineData("Isaías 1:0")]
    [InlineData("Isaías 0")]
    [InlineData("Isaías 37–36")]
    [InlineData("Isaías 7:14–10")]
    [InlineData("Isaías 53:12–52:13")]
    [InlineData("Isaías 52–53:12")]
    [InlineData("Isaías ٧:١٤")]
    [InlineData("Isaías ７:１４")]
    public void TentarLer_RetornaFalso_QuandoCapituloZeroIntervaloInvertidoOuDigitoNaoAscii(string texto)
    {
        ReferenciaBiblica.TentarLer(texto, out var slug).Should().BeFalse();
        slug.Should().BeEmpty();
    }

    [Fact]
    public void TentarLer_RetornaFalso_QuandoTextoNulo()
    {
        ReferenciaBiblica.TentarLer(null!, out _).Should().BeFalse();
    }

    // Nomes copiados de livros.json (Infrastructure); o projeto de testes de Domain não referencia Infrastructure.
    public static TheoryData<string> NomesDosLivros => new()
    {
        "GÊNESIS", "ÊXODO", "LEVÍTICO", "NÚMEROS", "DEUTERONÔMIO", "JOSUÉ", "JUÍZES", "RUTE",
        "1 SAMUEL", "2 SAMUEL", "1 REIS", "2 REIS", "1 CRÔNICAS", "2 CRÔNICAS", "ESDRAS", "NEEMIAS",
        "ESTER", "JÓ", "SALMOS", "PROVÉRBIOS", "ECLESIASTES", "CANTARES DE SALOMÃO", "ISAÍAS",
        "JEREMIAS", "LAMENTAÇÕES", "EZEQUIEL", "DANIEL", "OSÉIAS", "JOEL", "AMÓS", "OBADIAS", "JONAS",
        "MIQUÉIAS", "NAUM", "HABACUQUE", "SOFONIAS", "AGEU", "ZACARIAS", "MALAQUIAS", "MATEUS",
        "MARCOS", "LUCAS", "JOÃO", "ATOS DOS APÓSTOLOS", "ROMANOS", "1 CORÍNTIOS", "2 CORÍNTIOS",
        "GÁLATAS", "EFÉSIOS", "FILIPENSES", "COLOSSENSES", "1 TESSALONICENSES", "2 TESSALONICENSES",
        "1 TIMÓTEO", "2 TIMÓTEO", "TITO", "FILEMOM", "HEBREUS", "TIAGO", "1 PEDRO", "2 PEDRO",
        "1 JOÃO", "2 JOÃO", "3 JOÃO", "JUDAS", "APOCALIPSE",
    };

    [Theory]
    [MemberData(nameof(NomesDosLivros))]
    public void TentarLer_ReconheceLivro_QuandoNomeVemDoLivrosJson(string nome)
    {
        var ok = ReferenciaBiblica.TentarLer($"{nome} 1:1", out var slug);

        ok.Should().BeTrue();
        slug.Should().Be(Slug.From(nome));
    }

    [Fact]
    public void NomesDosLivros_TemSessentaESeis()
    {
        ((IEnumerable<object[]>)NomesDosLivros).Count().Should().Be(66);
    }
}
