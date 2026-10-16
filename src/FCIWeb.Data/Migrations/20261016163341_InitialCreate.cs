using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCIWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Richieste",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NDG = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Intestazione = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UOCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Matricola = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Stato = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Richieste", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Richieste_DataCreazione",
                table: "Richieste",
                column: "DataCreazione");

            migrationBuilder.CreateIndex(
                name: "IX_Richieste_NDG",
                table: "Richieste",
                column: "NDG");

            migrationBuilder.CreateIndex(
                name: "IX_Richieste_Stato",
                table: "Richieste",
                column: "Stato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Richieste");
        }
    }
}
