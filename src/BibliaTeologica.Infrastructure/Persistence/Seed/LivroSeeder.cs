using System.Text.Json;
using System.Text.Json.Serialization;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Persistence.Seed;

public class LivroSeeder
{
    private static readonly string CaminhoJson =
        Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "livros.json");

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _db;
    private readonly ILogger<LivroSeeder> _log;

    public LivroSeeder(AppDbContext db, ILogger<LivroSeeder> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Livros.AnyAsync(ct))
            return;

        await using var stream = File.OpenRead(CaminhoJson);
        var registros = await JsonSerializer.DeserializeAsync<List<RegistroSeed>>(stream, OpcoesJson, ct)
            ?? throw new InvalidOperationException("livros.json vazio ou inválido.");

        var livros = registros.Select((r, i) => Livro.Create(
            r.Nome,
            SecaoCanonicaExtensions.FromRotulo(r.Section),
            i + 1,
            r.Quem, r.Porque, r.ParaQuem, r.Estrutura, r.Tema, r.Ideias, r.Versiculo));

        _db.Livros.AddRange(livros);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Seed concluído: {Total} livros inseridos.", registros.Count);
    }

    private sealed record RegistroSeed(
        string Section,
        string Nome,
        string Quem,
        string Porque,
        [property: JsonPropertyName("praquem")] string ParaQuem,
        string Estrutura,
        string Tema,
        List<string> Ideias,
        string Versiculo);
}
