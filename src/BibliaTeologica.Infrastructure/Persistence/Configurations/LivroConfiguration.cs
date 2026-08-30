using System.Text.Json;
using BibliaTeologica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliaTeologica.Infrastructure.Persistence.Configurations;

public class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("Livros");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Slug).HasMaxLength(120).IsRequired();
        builder.HasIndex(l => l.Slug).IsUnique();

        builder.Property(l => l.Nome).HasMaxLength(120).IsRequired();
        builder.HasIndex(l => l.Ordem).IsUnique();

        builder.Property(l => l.Secao).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.Property(l => l.Quem).IsRequired();
        builder.Property(l => l.Porque).IsRequired();
        builder.Property(l => l.ParaQuem).IsRequired();
        builder.Property(l => l.Estrutura).IsRequired();
        builder.Property(l => l.Tema).IsRequired();
        builder.Property(l => l.Versiculo).IsRequired();
        builder.Property(l => l.Publicado).IsRequired();
        builder.Property(l => l.CriadoEm).IsRequired();
        builder.Property(l => l.AtualizadoEm).IsRequired();

        builder.Ignore(l => l.Testamento);

        // `Ideias` (IReadOnlyList get-only) apenas lê o campo `_ideias`. Persistimos o CAMPO
        // como coluna TEXT (JSON) e ignoramos a propriedade pública — nenhum dado é perdido,
        // porque a leitura da propriedade devolve exatamente o conteúdo de `_ideias`.
        builder.Ignore(l => l.Ideias);

        builder.Property<List<string>>("_ideias")
            .HasColumnName("Ideias")
            .HasColumnType("TEXT")
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    v => v.ToList()));
    }
}
