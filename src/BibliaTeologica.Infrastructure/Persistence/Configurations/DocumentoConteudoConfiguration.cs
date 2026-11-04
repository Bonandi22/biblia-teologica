using BibliaTeologica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliaTeologica.Infrastructure.Persistence.Configurations;

public class DocumentoConteudoConfiguration : IEntityTypeConfiguration<DocumentoConteudo>
{
    public void Configure(EntityTypeBuilder<DocumentoConteudo> builder)
    {
        builder.ToTable("DocumentosConteudo");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.Slug).HasMaxLength(120).IsRequired();
        builder.HasIndex(d => new { d.Tipo, d.Slug }).IsUnique();

        builder.Property(d => d.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Resumo).HasColumnType("TEXT").IsRequired();
        builder.Property(d => d.Json).HasColumnType("TEXT").IsRequired();
        builder.Property(d => d.ImportadoEm).IsRequired();
    }
}
