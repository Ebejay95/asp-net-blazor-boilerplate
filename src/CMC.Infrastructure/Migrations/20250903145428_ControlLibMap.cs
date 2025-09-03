using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ControlLibMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControlScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios");

            migrationBuilder.DropColumn(
                name: "LibraryScenarioId1",
                table: "LibraryControlScenarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LibraryScenarioId1",
                table: "LibraryControlScenarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios",
                column: "LibraryScenarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios",
                column: "LibraryScenarioId1",
                principalTable: "LibraryScenarios",
                principalColumn: "Id");
        }
    }
}
