using System.Text.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Application.Conteudo.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Moq;

namespace BibliaTeologica.Application.Tests.Conteudo;

public class ConteudoQueryServiceTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private static BlocoDto BlocoVazio(string texto) => new(texto, [], [], []);

    private static LivroPanoramaDto Panorama() =>
        new("isaias", "r", "q", "pq", "p", BlocoVazio("a"), BlocoVazio("b"), BlocoVazio("c"), [], []);

    private static EstudoDto Estudo(string slug) =>
        new(slug, "Título " + slug, "Resumo " + slug, "# Corpo", ["isaias"], [], []);

    private static DocumentoConteudo Doc(TipoConteudo tipo, string slug, string titulo, string resumo, object dto) =>
        DocumentoConteudo.Criar(tipo, slug, titulo, resumo, JsonSerializer.Serialize(dto, Web));

    private static (ConteudoQueryService Servico, Mock<IDocumentoConteudoRepository> Repo) Criar()
    {
        var repo = new Mock<IDocumentoConteudoRepository>();
        return (new ConteudoQueryService(repo.Object), repo);
    }

    [Fact]
    public async Task ObterPanoramaAsync_RetornaDto_QuandoDocumentoExiste()
    {
        var (servico, repo) = Criar();
        repo.Setup(r => r.ObterAsync(TipoConteudo.Livro, "isaias", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Doc(TipoConteudo.Livro, "isaias", "ISAÍAS", "r", Panorama()));

        var dto = await servico.ObterPanoramaAsync("isaias", CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.LivroSlug.Should().Be("isaias");
        dto.Panorama.Explicacao.Should().Be("a");
    }

    [Fact]
    public async Task ObterPanoramaAsync_RetornaNulo_QuandoDocumentoNaoExiste()
    {
        var (servico, _) = Criar();

        (await servico.ObterPanoramaAsync("isaias", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task ListarEstudosAsync_RetornaResumos_QuandoHaEstudos()
    {
        var (servico, repo) = Criar();
        repo.Setup(r => r.ListarAsync(TipoConteudo.Estudo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Doc(TipoConteudo.Estudo, "a", "Título a", "Resumo a", Estudo("a")),
                Doc(TipoConteudo.Estudo, "b", "Título b", "Resumo b", Estudo("b")),
            });

        var lista = await servico.ListarEstudosAsync(CancellationToken.None);

        lista.Should().Equal(new EstudoResumoDto("a", "Título a", "Resumo a"), new EstudoResumoDto("b", "Título b", "Resumo b"));
    }

    [Fact]
    public async Task ObterEstudoAsync_RetornaDto_QuandoDocumentoExiste()
    {
        var (servico, repo) = Criar();
        repo.Setup(r => r.ObterAsync(TipoConteudo.Estudo, "a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Doc(TipoConteudo.Estudo, "a", "Título a", "Resumo a", Estudo("a")));

        var dto = await servico.ObterEstudoAsync("a", CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.CorpoMarkdown.Should().Be("# Corpo");
        dto.Livros.Should().Equal("isaias");
    }

    [Fact]
    public async Task ObterEstudoAsync_RetornaNulo_QuandoDocumentoNaoExiste()
    {
        var (servico, _) = Criar();

        (await servico.ObterEstudoAsync("a", CancellationToken.None)).Should().BeNull();
    }
}
