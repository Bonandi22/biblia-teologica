using AwesomeAssertions;
using AutoMapper;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Application.Livros;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Livros;

public class LivroQueryServiceTests
{
    private readonly Mock<ILivroRepository> _repo = new();
    private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<LivroMappingProfile>()).CreateMapper();

    private LivroQueryService CriarServico() => new(_repo.Object, _mapper);

    private static Livro Livro(string nome, int ordem, SecaoCanonica secao = SecaoCanonica.Pentateuco)
        => BibliaTeologica.Domain.Entities.Livro.Create(nome, secao, ordem,
            "quem", "porque", "paraQuem", "estrutura", "tema", new[] { "ideia" }, "versiculo");

    [Fact]
    public async Task ListarAsync_MapeiaEPreservaOrdemDoRepositorio()
    {
        _repo.Setup(r => r.ListarPublicadosAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[] { Livro("GÊNESIS", 1), Livro("ÊXODO", 2) });

        var resultado = await CriarServico().ListarAsync(CancellationToken.None);

        resultado.Select(l => l.Slug).Should().Equal("genesis", "exodo");
        resultado[0].SecaoRotulo.Should().Be("PENTATEUCO");
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaDetalhe_QuandoLivroPublicado()
    {
        _repo.Setup(r => r.ObterPorSlugAsync("genesis", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Livro("GÊNESIS", 1));

        var dto = await CriarServico().ObterPorSlugAsync("genesis", CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Testamento.Should().Be("Antigo");
        dto.Secao.Should().Be("Pentateuco");
        dto.SecaoRotulo.Should().Be("PENTATEUCO");
        dto.Ideias.Should().ContainSingle();
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaNull_QuandoSlugNaoExiste()
    {
        _repo.Setup(r => r.ObterPorSlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Livro?)null);

        (await CriarServico().ObterPorSlugAsync("inexistente", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaNull_QuandoLivroDespublicado()
    {
        var livro = Livro("GÊNESIS", 1);
        typeof(BibliaTeologica.Domain.Entities.Livro)
            .GetProperty(nameof(BibliaTeologica.Domain.Entities.Livro.Publicado))!
            .GetSetMethod(nonPublic: true)!.Invoke(livro, new object?[] { false });
        _repo.Setup(r => r.ObterPorSlugAsync("genesis", It.IsAny<CancellationToken>())).ReturnsAsync(livro);

        (await CriarServico().ObterPorSlugAsync("genesis", CancellationToken.None)).Should().BeNull();
    }
}
