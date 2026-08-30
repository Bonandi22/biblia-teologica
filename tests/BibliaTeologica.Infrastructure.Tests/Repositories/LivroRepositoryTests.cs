using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Repositories;

public sealed class LivroRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _db;

    public LivroRepositoryTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);
        _db.Database.EnsureCreated();
    }

    private static Livro CreateLivro(string nome, int ordem, bool publicado = true)
    {
        var l = Livro.Create(nome, SecaoCanonica.Pentateuco, ordem,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        if (!publicado)
            typeof(Livro).GetProperty(nameof(Livro.Publicado))!
                .GetSetMethod(nonPublic: true)!.Invoke(l, new object?[] { false });
        return l;
    }

    [Fact]
    public async Task ListarPublicadosAsync_RetornaSoPublicados_OrdenadosPorOrdem()
    {
        _db.Livros.AddRange(CreateLivro("ÊXODO", 2), CreateLivro("GÊNESIS", 1), CreateLivro("LEVÍTICO", 3, publicado: false));
        await _db.SaveChangesAsync();

        var repo = new LivroRepository(_db);
        var resultado = await repo.ListarPublicadosAsync(CancellationToken.None);

        resultado.Select(l => l.Slug).Should().Equal("genesis", "exodo");
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaLivro_QuandoExiste()
    {
        _db.Livros.Add(CreateLivro("GÊNESIS", 1));
        await _db.SaveChangesAsync();

        var repo = new LivroRepository(_db);
        (await repo.ObterPorSlugAsync("genesis", CancellationToken.None)).Should().NotBeNull();
        (await repo.ObterPorSlugAsync("nao-existe", CancellationToken.None)).Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
    }
}
