using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class LivrosEndpointsTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public LivrosEndpointsTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_ApiLivros_Retorna200ComListaOrdenada()
    {
        var cliente = _factory.CreateClient();

        var livros = await cliente.GetFromJsonAsync<List<LivroListItemDto>>("/api/livros");

        livros.Should().NotBeNull();
        livros!.Should().HaveCount(66);
        livros.Select(l => l.Ordem).Should().BeInAscendingOrder();
        livros.First().Slug.Should().Be("genesis");
        livros.Last().Slug.Should().Be("apocalipse");
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna200ComDetalhe_QuandoExiste()
    {
        var cliente = _factory.CreateClient();

        var livro = await cliente.GetFromJsonAsync<LivroDetailDto>("/api/livros/genesis");

        livro.Should().NotBeNull();
        livro!.Nome.Should().Be("GÊNESIS");
        livro.Testamento.Should().Be("Antigo");
        livro.Ideias.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna404_QuandoNaoExiste()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/livros/nao-existe");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna404_QuandoDespublicado()
    {
        var cliente = _factory.CreateClient();
        await using (var db = await _factory.AbrirContextoAsync())
        {
            var livro = db.Livros.First(l => l.Slug == "obadias");
            db.Entry(livro).Property(nameof(Livro.Publicado)).CurrentValue = false;
            await db.SaveChangesAsync();
        }

        var resposta = await cliente.GetAsync("/api/livros/obadias");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Restore Obadias to published state for test isolation
        await using (var db = await _factory.AbrirContextoAsync())
        {
            var livro = db.Livros.First(l => l.Slug == "obadias");
            db.Entry(livro).Property(nameof(Livro.Publicado)).CurrentValue = true;
            await db.SaveChangesAsync();
        }
    }
}
