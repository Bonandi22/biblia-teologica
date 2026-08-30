using AwesomeAssertions;
using BibliaTeologica.Domain.Enums;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Enums;

public class SecaoCanonicaExtensionsTests
{
    [Theory]
    [InlineData(SecaoCanonica.Pentateuco, "PENTATEUCO")]
    [InlineData(SecaoCanonica.LivrosHistoricos, "LIVROS HISTÓRICOS")]
    [InlineData(SecaoCanonica.CartasPaulinas, "CARTAS PAULINAS")]
    [InlineData(SecaoCanonica.Apocalipse, "APOCALIPSE")]
    public void Rotulo_RetornaTextoDeExibicao(SecaoCanonica secao, string esperado)
        => secao.Rotulo().Should().Be(esperado);

    [Theory]
    [InlineData(SecaoCanonica.Pentateuco, Testamento.Antigo)]
    [InlineData(SecaoCanonica.ProfetasMenores, Testamento.Antigo)]
    [InlineData(SecaoCanonica.Evangelhos, Testamento.Novo)]
    [InlineData(SecaoCanonica.Apocalipse, Testamento.Novo)]
    public void Testamento_DerivaDaSecao(SecaoCanonica secao, Testamento esperado)
        => secao.Testamento().Should().Be(esperado);

    [Theory]
    [InlineData("PENTATEUCO", SecaoCanonica.Pentateuco)]
    [InlineData("livros poéticos", SecaoCanonica.LivrosPoeticos)]
    [InlineData("PROFETAS MENORES", SecaoCanonica.ProfetasMenores)]
    public void FromRotulo_ResolveEnum_IgnorandoCase(string rotulo, SecaoCanonica esperado)
        => SecaoCanonicaExtensions.FromRotulo(rotulo).Should().Be(esperado);

    [Fact]
    public void FromRotulo_LancaArgumentException_QuandoRotuloDesconhecido()
    {
        var acao = () => SecaoCanonicaExtensions.FromRotulo("APÓCRIFOS");
        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TodasAsSecoes_TemDescription()
    {
        foreach (var s in Enum.GetValues<SecaoCanonica>())
            s.Rotulo().Should().NotBe(s.ToString());
    }
}
