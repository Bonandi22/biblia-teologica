using System.Text.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Application.Conteudo.Dtos;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Tests.Conteudo;

public class ConstrutorDocumentosTests
{
    private static readonly IReadOnlyDictionary<string, string> Titulos =
        new Dictionary<string, string> { ["isaias"] = "ISAÍAS" };

    private static RegistroObra Obra() =>
        new() { Id = "obra1", Autor = "Fulano", Titulo = "Obra Um", Detalhes = "Editora X, 2000", Url = "https://exemplo.org/obra" };

    private static RegistroItem Acontecimento(string quando, int ano, string texto) =>
        new() { Quando = quando, Ano = ano, Texto = texto };

    private static RegistroBloco Bloco(params RegistroItem[] acontecimentos) =>
        new()
        {
            Explicacao = "Texto.[^1]",
            Acontecimentos = acontecimentos.ToList(),
            Notas = new() { new RegistroNota { Numero = 1, Obra = "obra1", Local = "p. 10" } },
        };

    private static RegistroLivro Livro(RegistroBloco panorama, RegistroBloco contexto, RegistroBloco mundo) =>
        new()
        {
            Arquivo = "isaias.yaml", Estado = "revisado", Livro = "isaias", Resumo = "r", Quem = "q",
            ParaQuem = "pq", Porque = "p", Panorama = panorama, Contexto = contexto, Mundo = mundo,
        };

    private static LivroPanoramaDto Construir(RegistroLivro livro)
    {
        var docs = ConstrutorDocumentos.Construir(
            new ConjuntoRegistros(new[] { Obra() }, new[] { livro }, Array.Empty<RegistroEstudo>()), Titulos);
        var doc = docs.Should().ContainSingle().Subject;
        doc.Tipo.Should().Be(TipoConteudo.Livro);
        doc.Slug.Should().Be("isaias");
        return JsonSerializer.Deserialize<LivroPanoramaDto>(doc.Json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    [Fact]
    public void Construir_FormataCitacaoCompleta_QuandoNotaTemObraELocal()
    {
        var dto = Construir(Livro(Bloco(), Bloco(), Bloco()));

        var nota = dto.Panorama.Notas.Should().ContainSingle().Subject;
        nota.Numero.Should().Be(1);
        nota.Citacao.Should().Be("Fulano. Obra Um. Editora X, 2000, p. 10.");
        nota.Url.Should().Be("https://exemplo.org/obra");
    }

    [Fact]
    public void Construir_OrdenaAcontecimentosPorAno_QuandoContextoEMundo()
    {
        var desordenado = new[] { Acontecimento("depois", -500, "b"), Acontecimento("antes", -701, "a") };
        var dto = Construir(Livro(Bloco(), Bloco(desordenado), Bloco(desordenado)));

        dto.Contexto.Acontecimentos.Select(a => a.Quando).Should().Equal("antes", "depois");
        dto.Mundo.Acontecimentos.Select(a => a.Quando).Should().Equal("antes", "depois");
        dto.Contexto.Acontecimentos[0].Item.Nome.Should().BeEmpty();
    }

    [Fact]
    public void Construir_MantemOrdemDoArquivo_QuandoPanorama()
    {
        var naOrdem = new[] { Acontecimento("depois", -500, "b"), Acontecimento("antes", -701, "a") };
        var dto = Construir(Livro(Bloco(naOrdem), Bloco(), Bloco()));

        dto.Panorama.Acontecimentos.Select(a => a.Quando).Should().Equal("depois", "antes");
    }
}
