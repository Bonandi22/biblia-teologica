using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Conteudo.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public sealed class PanoramaFactory : AppFactory
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), $"biblia-conteudo-{Guid.NewGuid():N}");

    public PanoramaFactory()
    {
        Directory.CreateDirectory(_pasta);
        ConfiguracaoExtra["Conteudo:Pasta"] = _pasta;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { if (disposing && Directory.Exists(_pasta)) Directory.Delete(_pasta, true); }
        catch (IOException) { /* pasta temporária */ }
    }
}

public class PanoramaEndpointsTests : IClassFixture<PanoramaFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly PanoramaFactory _factory;

    public PanoramaEndpointsTests(PanoramaFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await using var db = await _factory.AbrirContextoAsync();
        if (db.DocumentosConteudo.Any()) return;

        var bloco = new BlocoDto("Explicação", [], [], []);
        var panorama = new LivroPanoramaDto("isaias", "Resumo", [new PerfilItemDto("Origem", "Filho de Amoz.", ["Isaías 1:1"])], bloco, bloco, bloco, [], [], []);
        var estudo = new EstudoDto("senaqueribe", "Senaqueribe", "Resumo do estudo", "# Corpo", ["isaias"], [], []);
        db.DocumentosConteudo.AddRange(
            DocumentoConteudo.Criar(TipoConteudo.Livro, "isaias", "ISAÍAS", "Resumo", JsonSerializer.Serialize(panorama, Web)),
            DocumentoConteudo.Criar(TipoConteudo.Estudo, "senaqueribe", "Senaqueribe", "Resumo do estudo", JsonSerializer.Serialize(estudo, Web)));
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GET_ApiPanoramasLivrosSlug_Retorna200ComPanorama_QuandoExiste()
    {
        var dto = await _factory.CreateClient().GetFromJsonAsync<LivroPanoramaDto>("/api/panoramas/livros/isaias");

        dto.Should().NotBeNull();
        dto!.LivroSlug.Should().Be("isaias");
        dto.Panorama.Explicacao.Should().Be("Explicação");
        dto.Perfil.Should().ContainSingle().Which.Rotulo.Should().Be("Origem");
    }

    [Fact]
    public async Task GET_ApiPanoramasLivrosSlug_NaoExpoeCategoriaNemCumprimento_QuandoExiste()
    {
        var json = await _factory.CreateClient().GetStringAsync("/api/panoramas/livros/isaias");

        json.Should().Contain("\"promessas\"");
        json.Should().NotContain("categoria");
        json.Should().NotContain("cumprimento");
        json.Should().NotContain("fatoHistorico");
    }

    [Fact]
    public async Task GET_PanoramasLivros_RetornaSlugsPublicados()
    {
        var slugs = await _factory.CreateClient().GetFromJsonAsync<List<string>>("/api/panoramas/livros");

        slugs.Should().Equal("isaias");
    }

    [Fact]
    public async Task POST_PanoramasLivros_Retorna405()
    {
        var resposta = await _factory.CreateClient().PostAsync("/api/panoramas/livros", null);

        resposta.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GET_ApiPanoramasLivrosSlug_Retorna404_QuandoDocumentoNaoExiste()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/panoramas/livros/genesis");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiPanoramasLivrosSlug_Retorna404_QuandoSlugForaDoFormato()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/panoramas/livros/Isaias%20'");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiEstudos_Retorna200ComResumos()
    {
        var lista = await _factory.CreateClient().GetFromJsonAsync<List<EstudoResumoDto>>("/api/estudos");

        lista.Should().ContainSingle().Which.Should().Be(new EstudoResumoDto("senaqueribe", "Senaqueribe", "Resumo do estudo"));
    }

    [Fact]
    public async Task GET_ApiEstudosSlug_Retorna200ComEstudo_QuandoExiste()
    {
        var dto = await _factory.CreateClient().GetFromJsonAsync<EstudoDto>("/api/estudos/senaqueribe");

        dto.Should().NotBeNull();
        dto!.CorpoMarkdown.Should().Be("# Corpo");
    }

    [Fact]
    public async Task GET_ApiEstudosSlug_Retorna404_QuandoDocumentoNaoExiste()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/estudos/nao-existe");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiEstudosSlug_Retorna404_QuandoSlugForaDoFormato()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/estudos/Isaias%20'");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_ApiEstudos_Retorna405()
    {
        var resposta = await _factory.CreateClient().PostAsJsonAsync("/api/estudos", new { });
        resposta.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GET_ApiLivros_MantemContrato_QuandoPanoramaExiste()
    {
        var cliente = _factory.CreateClient();

        var lista = await cliente.GetFromJsonAsync<JsonElement>("/api/livros");
        var detalhe = await cliente.GetFromJsonAsync<JsonElement>("/api/livros/isaias");

        lista.EnumerateArray().First().EnumerateObject().Select(p => p.Name)
            .Should().BeEquivalentTo("slug", "nome", "ordem", "secao", "secaoRotulo");
        detalhe.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(
            "slug", "nome", "ordem", "secao", "secaoRotulo", "testamento", "quem", "porque",
            "paraQuem", "estrutura", "tema", "ideias", "versiculo");
    }
}
