using AwesomeAssertions;
using BibliaTeologica.Application.Common;
using Xunit;

namespace BibliaTeologica.Application.Tests.Common;

public class TelefoneTests
{
    [Theory]
    [InlineData("+351 912 345 678", true, "+351912345678")]
    [InlineData("(11) 98765-4321", true, "11987654321")]
    [InlineData(null, true, null)]
    [InlineData("   ", true, null)]
    [InlineData("---", false, null)]
    [InlineData("1234567", false, null)]
    [InlineData("1234567890123456", false, null)]
    [InlineData("12a45678", false, null)]
    [InlineData("12+345678", false, null)]
    public void TentarNormalizar_DevolveResultadoEsperado_QuandoEntradaVaria(string? entrada, bool ok, string? esperado)
    {
        var resultado = Telefone.TentarNormalizar(entrada, out var normalizado);

        resultado.Should().Be(ok);
        normalizado.Should().Be(esperado);
    }
}
