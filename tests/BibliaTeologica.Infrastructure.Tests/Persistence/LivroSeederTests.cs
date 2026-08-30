using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class LivroSeederTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public LivroSeederTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
    }

    private AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task SeedAsync_Insere66Livros_QuandoTabelaVazia()
    {
        await using var ctx = CriarContexto();
        await new LivroSeeder(ctx, NullLogger<LivroSeeder>.Instance).SeedAsync();

        ctx.Livros.Count().Should().Be(66);
        (await ctx.Livros.OrderBy(l => l.Ordem).FirstAsync()).Slug.Should().Be("genesis");
        (await ctx.Livros.OrderBy(l => l.Ordem).LastAsync()).Slug.Should().Be("apocalipse");
        ctx.Livros.Select(l => l.Ordem).OrderBy(o => o).Should().Equal(Enumerable.Range(1, 66));
    }

    [Fact]
    public async Task SeedAsync_NaoDuplica_QuandoJaHaLivros()
    {
        await using var ctx = CriarContexto();
        var seeder = new LivroSeeder(ctx, NullLogger<LivroSeeder>.Instance);
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        ctx.Livros.Count().Should().Be(66);
    }

    public void Dispose() => _conexao.Dispose();
}
