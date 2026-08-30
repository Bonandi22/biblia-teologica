using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Entities;

public class LivroTests
{
    private static Livro LivroValido(string nome = "GÊNESIS", int ordem = 1)
        => Livro.Create(nome, SecaoCanonica.Pentateuco, ordem,
            quem: "Atribuído a Moisés", porque: "Dar origem a Israel",
            paraQuem: "Israel no deserto", estrutura: "História primitiva e patriarcal",
            tema: "Deus criador e aliança", ideias: new[] { "Criação", "Queda", "Aliança com Abraão" },
            versiculo: "Gênesis 1:1");

    [Fact]
    public void Create_PreencheCamposEGeraSlug_QuandoDadosValidos()
    {
        var livro = LivroValido();

        livro.Slug.Should().Be("genesis");
        livro.Nome.Should().Be("GÊNESIS");
        livro.Secao.Should().Be(SecaoCanonica.Pentateuco);
        livro.Ordem.Should().Be(1);
        livro.Ideias.Should().HaveCount(3);
        livro.Publicado.Should().BeTrue();
        livro.Testamento.Should().Be(Testamento.Antigo);
        livro.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        livro.AtualizadoEm.Should().Be(livro.CriadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_LancaDomainException_QuandoNomeVazio(string nome)
    {
        var acao = () => LivroValido(nome: nome);
        acao.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(67)]
    [InlineData(-1)]
    public void Create_LancaDomainException_QuandoOrdemForaDoIntervalo(int ordem)
    {
        var acao = () => LivroValido(ordem: ordem);
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_LancaDomainException_QuandoIdeiasVazia()
    {
        var acao = () => Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "Confrontar retribuição", "Israel", "Prólogo, diálogos, epílogo",
            "Justiça de Deus", ideias: Array.Empty<string>(), versiculo: "Jó 42:5");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_LancaDomainException_QuandoIdeiasSoTemBranco()
    {
        var acao = () => Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "x", "Israel", "y", "z", ideias: new[] { "  ", "" }, versiculo: "Jó 42:5");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_IgnoraIdeiasEmBranco_MasMantemAsValidas()
    {
        var livro = Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "x", "Israel", "y", "z", ideias: new[] { "Aposta celestial", "  ", "Restauração" },
            versiculo: "Jó 42:5");
        livro.Ideias.Should().Equal("Aposta celestial", "Restauração");
    }
}
