using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliaTeologica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Secao = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    Quem = table.Column<string>(type: "TEXT", nullable: false),
                    Porque = table.Column<string>(type: "TEXT", nullable: false),
                    ParaQuem = table.Column<string>(type: "TEXT", nullable: false),
                    Estrutura = table.Column<string>(type: "TEXT", nullable: false),
                    Tema = table.Column<string>(type: "TEXT", nullable: false),
                    Versiculo = table.Column<string>(type: "TEXT", nullable: false),
                    Publicado = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ideias = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Livros_Ordem",
                table: "Livros",
                column: "Ordem",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Livros_Slug",
                table: "Livros",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Livros");
        }
    }
}
