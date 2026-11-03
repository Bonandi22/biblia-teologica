using AwesomeAssertions;
using BibliaTeologica.Application.Conteudo;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Conteudo;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Tests.Conteudo;

public sealed class ImportacaoConteudoTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "bt-import-" + Guid.NewGuid().ToString("N"));
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _db;

    public ImportacaoConteudoTests()
    {
        Directory.CreateDirectory(Path.Combine(_pasta, "livros"));
        Directory.CreateDirectory(Path.Combine(_pasta, "estudos"));
        File.WriteAllText(Path.Combine(_pasta, "bibliografia.yaml"),
            "- id: obra1\n  autor: Fulano\n  titulo: Obra Um\n  detalhes: Editora X\n  url: null\n");

        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);
        _db.Database.EnsureCreated();
        _db.Livros.Add(Livro.Create("ISAÍAS", SecaoCanonica.ProfetasMaiores, 23, "q", "p", "pq", "e", "t", new[] { "i" }, "v"));
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
        try { Directory.Delete(_pasta, true); } catch (IOException) { }
    }

    private static string LivroYaml(string resumo) => $$"""
        estado: revisado
        livro: isaias
        resumo: "{{resumo}}"
        perfil:
          - rotulo: "Origem"
            texto: "t"
        panorama:
          explicacao: "Texto."
        contexto:
          explicacao: "Contexto.[^1]"
          notas:
            - { numero: 1, obra: obra1, local: "p. 1" }
        mundo:
          explicacao: "Mundo.[^1]"
          notas:
            - { numero: 1, obra: obra1, local: "p. 2" }
        """;

    private void EscreverLivro(string yaml) =>
        File.WriteAllText(Path.Combine(_pasta, "livros", "isaias.yaml"), yaml);

    private ImportadorConteudo Criar(string? pasta = null) => new(
        new FonteConteudoArquivos(Options.Create(new ConteudoOptions { Pasta = pasta ?? _pasta }), NullLogger<FonteConteudoArquivos>.Instance),
        new LivroRepository(_db),
        new DocumentoConteudoRepository(_db),
        NullLogger<ImportadorConteudo>.Instance);

    private Task<List<DocumentoConteudo>> Docs() => _db.DocumentosConteudo.AsNoTracking().ToListAsync();

    [Fact]
    public async Task ImportarAsync_AtualizaDocumento_QuandoArquivoCorrigido()
    {
        EscreverLivro(LivroYaml("primeiro"));
        await Criar().ImportarAsync(false, default);
        EscreverLivro(LivroYaml("corrigido"));
        await Criar().ImportarAsync(false, default);

        var doc = (await Docs()).Should().ContainSingle().Subject;
        doc.Resumo.Should().Be("corrigido");
        doc.Json.Should().Contain("corrigido");
    }

    [Fact]
    public async Task ImportarAsync_RemoveDocumento_QuandoArquivoApagado()
    {
        EscreverLivro(LivroYaml("primeiro"));
        await Criar().ImportarAsync(false, default);
        (await Docs()).Should().HaveCount(1);

        File.Delete(Path.Combine(_pasta, "livros", "isaias.yaml"));
        await Criar().ImportarAsync(false, default);

        (await Docs()).Should().BeEmpty();
    }

    [Fact]
    public async Task ImportarAsync_NaoAlteraBanco_QuandoHaArquivoInvalido()
    {
        EscreverLivro(LivroYaml("primeiro"));
        await Criar().ImportarAsync(false, default);
        EscreverLivro(LivroYaml("novo").Replace("livro: isaias", "livro: inexistente"));

        var agir = () => Criar().ImportarAsync(false, default);

        await agir.Should().ThrowAsync<ConteudoInvalidoException>();
        var doc = (await Docs()).Should().ContainSingle().Subject;
        doc.Resumo.Should().Be("primeiro");
    }

    [Fact]
    public async Task ImportarAsync_NaoAlteraLivros_QuandoImportaPanorama()
    {
        EscreverLivro(LivroYaml("primeiro"));
        var antes = await _db.Livros.AsNoTracking().ToListAsync();

        await Criar().ImportarAsync(false, default);

        var depois = await _db.Livros.AsNoTracking().ToListAsync();
        depois.Should().BeEquivalentTo(antes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConteudoReal_ImportaSemErros_QuandoRepositorioAtual(bool incluirRascunhos)
    {
        // Lê o content/ copiado para a saída do build, contra os 66 livros do seed.
        _db.Livros.RemoveRange(_db.Livros);
        await _db.SaveChangesAsync();
        await new LivroSeeder(_db, NullLogger<LivroSeeder>.Instance).SeedAsync();
        var importador = Criar(Path.Combine(AppContext.BaseDirectory, "content"));

        var agir = () => importador.ImportarAsync(incluirRascunhos, default);

        await agir.Should().NotThrowAsync();
    }
}
