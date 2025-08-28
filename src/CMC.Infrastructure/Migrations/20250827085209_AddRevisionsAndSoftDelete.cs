using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRevisionsAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "LibraryFrameworks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "LibraryFrameworks",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LibraryFrameworks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Revisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Table = table.Column<string>(type: "text", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFrameworks_IsDeleted",
                table: "LibraryFrameworks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Revisions_Table_AssetId_CreatedAt",
                table: "Revisions",
                columns: new[] { "Table", "AssetId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Revisions");

            migrationBuilder.DropIndex(
                name: "IX_LibraryFrameworks_IsDeleted",
                table: "LibraryFrameworks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LibraryFrameworks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LibraryFrameworks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LibraryFrameworks");
        }
    }
}
