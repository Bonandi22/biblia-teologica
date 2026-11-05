using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliaTeologica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Etapa2_TermoAceite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TermoAceitoEm",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermoVersao",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermoAceitoEm",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TermoVersao",
                table: "AspNetUsers");
        }
    }
}
