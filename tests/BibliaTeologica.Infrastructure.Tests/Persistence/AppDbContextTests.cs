using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public AppDbContextTests()
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
    public async Task PersisteERecupera_Livro_ComIdeiasComoJson()
    {
        await using (var ctx = CriarContexto())
        {
            ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
                "quem", "porque", "paraQuem", "estrutura", "tema",
                new[] { "Criação", "Queda" }, "Gênesis 1:1"));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            var livro = await ctx.Livros.SingleAsync();
            livro.Slug.Should().Be("genesis");
            livro.Secao.Should().Be(SecaoCanonica.Pentateuco);
            livro.Ideias.Should().Equal("Criação", "Queda");
        }
    }

    [Fact]
    public async Task RejeitaSlugDuplicado()
    {
        await using var ctx = CriarContexto();
        ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v"));
        ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 2,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v"));

        var acao = async () => await ctx.SaveChangesAsync();
        await acao.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task PersisteERecupera_RefreshToken()
    {
        var userId = Guid.NewGuid();
        await using (var ctx = CriarContexto())
        {
            ctx.Users.Add(new BibliaTeologica.Infrastructure.Identity.ApplicationUser
            {
                Id = userId, UserName = "a@b.c", Email = "a@b.c", NomeExibicao = "A"
            });
            ctx.RefreshTokens.Add(new BibliaTeologica.Infrastructure.Identity.RefreshToken
            {
                Id = Guid.NewGuid(), UsuarioId = userId, TokenHash = "hash-1",
                ExpiraEm = DateTime.UtcNow.AddDays(1)
            });
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            var token = await ctx.RefreshTokens.SingleAsync();
            token.UsuarioId.Should().Be(userId);
            token.RevogadoEm.Should().BeNull();
        }
    }

    public void Dispose() => _conexao.Dispose();
}
