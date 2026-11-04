using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliaTeologica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Panorama_DocumentosConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosConteudo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Resumo = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosConteudo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosConteudo_Tipo_Slug",
                table: "DocumentosConteudo",
                columns: new[] { "Tipo", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosConteudo");
        }
    }
}
