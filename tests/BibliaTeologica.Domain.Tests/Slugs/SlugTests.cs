using AwesomeAssertions;
using BibliaTeologica.Domain.Slugs;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Slugs;

public class SlugTests
{
    [Theory]
    [InlineData("GÊNESIS", "genesis")]
    [InlineData("ÊXODO", "exodo")]
    [InlineData("1 SAMUEL", "1-samuel")]
    [InlineData("CANTARES DE SALOMÃO", "cantares-de-salomao")]
    [InlineData("  Provérbios  ", "proverbios")]
    [InlineData("ATOS DOS APÓSTOLOS", "atos-dos-apostolos")]
    public void From_GeraSlugMinusculoSemAcento_QuandoNomeValido(string entrada, string esperado)
        => Slug.From(entrada).Should().Be(esperado);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_LancaArgumentException_QuandoTextoVazio(string? entrada)
    {
        var acao = () => Slug.From(entrada!);
        acao.Should().Throw<ArgumentException>();
    }
}
