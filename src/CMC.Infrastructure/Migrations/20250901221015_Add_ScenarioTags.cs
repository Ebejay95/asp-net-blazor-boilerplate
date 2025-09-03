using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ScenarioTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScenarioTags_Tags_TagId1",
                table: "ScenarioTags");

            migrationBuilder.DropIndex(
                name: "IX_ScenarioTags_TagId1",
                table: "ScenarioTags");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "ScenarioTags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "ScenarioTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTags_TagId1",
                table: "ScenarioTags",
                column: "TagId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ScenarioTags_Tags_TagId1",
                table: "ScenarioTags",
                column: "TagId1",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
