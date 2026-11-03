using System.Text;
using AwesomeAssertions;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Conteudo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Tests.Conteudo;

public sealed class FonteConteudoArquivosTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "bt-conteudo-" + Guid.NewGuid().ToString("N"));

    public FonteConteudoArquivosTests()
    {
        Directory.CreateDirectory(Path.Combine(_pasta, "livros"));
        Directory.CreateDirectory(Path.Combine(_pasta, "estudos"));
    }

    public void Dispose()
    {
        try { Directory.Delete(_pasta, true); } catch (IOException) { }
    }

    private FonteConteudoArquivos Criar() =>
        new(Options.Create(new ConteudoOptions { Pasta = _pasta }), NullLogger<FonteConteudoArquivos>.Instance);

    private void Escrever(string relativo, string texto, bool bom = false)
    {
        var caminho = Path.Combine(_pasta, relativo);
        File.WriteAllBytes(caminho, new UTF8Encoding(bom).GetPreamble().Concat(Encoding.UTF8.GetBytes(texto)).ToArray());
    }

    private const string Bibliografia = """
        - id: arc
          autor: Sociedade Bíblica do Brasil
          titulo: Bíblia Sagrada
          detalhes: "d"
          url: null
        """;

    private const string Livro = """
        estado: revisado
        livro: isaias
        resumo: "r"
        quem: "q"
        paraQuem: "p"
        porque: "po"
        panorama:
          explicacao: "Texto.[^1]"
          personagens:
            - nome: Ezequias
              texto: "t"
              referencias: ["Isaías 36:1"]
          notas:
            - { numero: 1, obra: arc, local: "Isaías 36:1" }
        profecias:
          - referencia: "Isaías 7:14"
            anuncia: "a"
            categoria: cumpridaEmCristo
            cumprimento: ["Mateus 1:22–23"]
        citacoes:
          - trecho: "Isaías 40:3"
            citadoEm: ["Mateus 3:3"]
        """;

    private const string Estudo = """
        ---
        estado: revisado
        titulo: "Cerco"
        resumo: "r"
        livros: [isaias, 2-reis]
        relacionados: []
        notas: [{ numero: 1, obra: arc, local: "x" }]
        ---
        # Corpo

        Texto com --- no meio.
        """;

    [Fact]
    public async Task LerAsync_LeTodosOsTipos_QuandoArquivosRevisados()
    {
        Escrever("bibliografia.yaml", Bibliografia);
        Escrever("livros/isaias.yaml", Livro);
        Escrever("estudos/cerco.md", Estudo);

        var r = await Criar().LerAsync(false, default);

        r.Obras.Should().ContainSingle().Which.Id.Should().Be("arc");
        var livro = r.Livros.Should().ContainSingle().Subject;
        livro.Livro.Should().Be("isaias");
        livro.Arquivo.Should().EndWith("isaias.yaml");
        livro.Panorama!.Personagens.Should().ContainSingle().Which.Nome.Should().Be("Ezequias");
        livro.Profecias.Single().Categoria.Should().Be("cumpridaEmCristo");
        livro.Citacoes.Single().CitadoEm.Should().Equal("Mateus 3:3");
        var estudo = r.Estudos.Should().ContainSingle().Subject;
        estudo.Titulo.Should().Be("Cerco");
        estudo.Livros.Should().Equal("isaias", "2-reis");
        estudo.Arquivo.Should().EndWith("cerco.md");
        estudo.CorpoMarkdown.Should().Contain("# Corpo").And.Contain("--- no meio").And.NotContain("estado:");
    }

    [Fact]
    public async Task LerAsync_LeNormalmente_QuandoArquivoTemCrlfEBom()
    {
        Escrever("bibliografia.yaml", Bibliografia.Replace("\n", "\r\n"), bom: true);
        Escrever("livros/isaias.yaml", Livro.Replace("\n", "\r\n"), bom: true);
        Escrever("estudos/cerco.md", Estudo.Replace("\n", "\r\n"), bom: true);

        var r = await Criar().LerAsync(false, default);

        r.Obras.Should().HaveCount(1);
        r.Livros.Should().HaveCount(1);
        r.Estudos.Single().Titulo.Should().Be("Cerco");
        r.Estudos.Single().CorpoMarkdown.Should().NotContain("\r");
    }

    [Fact]
    public async Task LerAsync_IgnoraRascunhoInvalido_QuandoNaoIncluiRascunhos()
    {
        Escrever("livros/isaias.yaml", "estado: rascunho\nlivro: [quebrado\n");
        Escrever("estudos/x.md", "---\nestado: rascunho\ntitulo: [quebrado\n---\ncorpo");

        var r = await Criar().LerAsync(false, default);

        r.Livros.Should().BeEmpty();
        r.Estudos.Should().BeEmpty();
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoRascunhoInvalidoEIncluiRascunhos()
    {
        Escrever("livros/isaias.yaml", "estado: rascunho\nlivro: [quebrado\n");

        var acao = () => Criar().LerAsync(true, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("isaias.yaml"));
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoRevisadoTemYamlInvalido()
    {
        Escrever("livros/isaias.yaml", "estado: revisado\nlivro: [quebrado\n");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("isaias.yaml"));
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoElementoDeListaYamlENulo()
    {
        Escrever("livros/isaias.yaml", "estado: revisado\nlivro: isaias\npanorama:\n  personagens:\n    -\n    - nome: A\n      referencias: [~]\ncitacoes:\n  -\n");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("isaias.yaml"));
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoEstudoTemListaComElementoNulo()
    {
        Escrever("estudos/x.md", "---\nestado: revisado\ntitulo: t\nlivros: [isaias, ~]\n---\ncorpo");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("x.md"));
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoEstudoSemFrontmatter()
    {
        Escrever("estudos/x.md", "# sem frontmatter");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("x.md"));
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoArquivoMaiorQue1Mb()
    {
        Escrever("livros/isaias.yaml", "estado: revisado\nresumo: \"" + new string('a', 1024 * 1024 + 10) + "\"\n");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("isaias.yaml"));
    }

    [FactSymlink]
    public async Task LerAsync_NaoSegueLink_QuandoArquivoAponteParaForaDaPasta()
    {
        var fora = Path.Combine(Path.GetTempPath(), "bt-fora-" + Guid.NewGuid().ToString("N") + ".yaml");
        File.WriteAllText(fora, Livro);
        try
        {
            File.CreateSymbolicLink(Path.Combine(_pasta, "livros", "isaias.yaml"), fora);

            var r = await Criar().LerAsync(false, default);

            r.Livros.Should().BeEmpty();
        }
        finally
        {
            File.Delete(fora);
        }
    }

    [Fact]
    public async Task LerAsync_IgnoraRascunho_QuandoEstudoSemFechamentoDeFrontmatter()
    {
        Escrever("estudos/x.md", "---\nestado: rascunho\ntitulo: t\ncorpo sem fechamento");

        var r = await Criar().LerAsync(false, default);

        r.Estudos.Should().BeEmpty();
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoEstudoRevisadoSemFechamentoDeFrontmatter()
    {
        Escrever("estudos/x.md", "---\nestado: revisado\ntitulo: t\ncorpo sem fechamento");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("x.md"));
    }

    [Fact]
    public async Task LerAsync_IgnoraRascunho_QuandoArquivoMaiorQue1Mb()
    {
        Escrever("livros/isaias.yaml", "estado: rascunho\nresumo: \"" + new string('a', 1024 * 1024 + 10) + "\"\n");

        var r = await Criar().LerAsync(false, default);

        r.Livros.Should().BeEmpty();
    }

    [Fact]
    public async Task LerAsync_LancaConteudoInvalido_QuandoBibliografiaTemElementoNulo()
    {
        Escrever("bibliografia.yaml", "- id: arc\n-\n");

        var acao = () => Criar().LerAsync(false, default);

        (await acao.Should().ThrowAsync<ConteudoInvalidoException>())
            .Which.Erros.Should().Contain(e => e.Contains("bibliografia.yaml"));
    }

    [Fact]
    public async Task LerAsync_IgnoraReadmeEGuia_QuandoNaRaiz()
    {
        Escrever("README.md", "# processo");
        Escrever("GUIA-DE-ESTILO.md", "# guia");

        var r = await Criar().LerAsync(false, default);

        r.Estudos.Should().BeEmpty();
        r.Livros.Should().BeEmpty();
        r.Obras.Should().BeEmpty();
    }
}

/// <summary>Fact que se marca como skip quando o SO/usuário não pode criar symlinks (xUnit 2 não tem skip dinâmico).</summary>
public sealed class FactSymlinkAttribute : FactAttribute
{
    public FactSymlinkAttribute()
    {
        var dir = Path.Combine(Path.GetTempPath(), "bt-sym-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var alvo = Path.Combine(dir, "alvo");
            File.WriteAllText(alvo, "x");
            File.CreateSymbolicLink(Path.Combine(dir, "link"), alvo);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            Skip = "Sem privilégio para criar symlink neste ambiente.";
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch (IOException) { }
        }
    }
}
