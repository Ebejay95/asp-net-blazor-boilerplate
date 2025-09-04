using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ControlTags_And_ControlIndustries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlTags_Tags_TagId1",
                table: "LibraryControlTags");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioTags_Tags_TagId1",
                table: "LibraryScenarioTags");

            migrationBuilder.DropIndex(
                name: "IX_LibraryScenarioTags_TagId1",
                table: "LibraryScenarioTags");

            migrationBuilder.DropIndex(
                name: "IX_LibraryScenarioIndustries_IndustryId1",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControlTags_TagId1",
                table: "LibraryControlTags");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControlIndustries_IndustryId1",
                table: "LibraryControlIndustries");

            migrationBuilder.DropIndex(
                name: "IX_Frameworks_Name_Version",
                table: "Frameworks");

            migrationBuilder.DropIndex(
                name: "IX_FrameworkIndustries_IndustryId1",
                table: "FrameworkIndustries");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "UX_Customers_Name_Active",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "LibraryScenarioTags");

            migrationBuilder.DropColumn(
                name: "IndustryId1",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "LibraryControlTags");

            migrationBuilder.DropColumn(
                name: "IndustryId1",
                table: "LibraryControlIndustries");

            migrationBuilder.DropColumn(
                name: "IndustryId1",
                table: "FrameworkIndustries");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ControlIndustries",
                columns: table => new
                {
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlIndustries", x => new { x.ControlId, x.IndustryId });
                    table.ForeignKey(
                        name: "FK_ControlIndustries_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlIndustries_Industries_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "Industries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlTags",
                columns: table => new
                {
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlTags", x => new { x.ControlId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ControlTags_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Frameworks_Name_Version_Active",
                table: "Frameworks",
                columns: new[] { "Name", "Version" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ControlIndustries_IndustryId",
                table: "ControlIndustries",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlTags_TagId",
                table: "ControlTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlIndustries");

            migrationBuilder.DropTable(
                name: "ControlTags");

            migrationBuilder.DropIndex(
                name: "UX_Frameworks_Name_Version_Active",
                table: "Frameworks");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Name",
                table: "Customers");

            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "LibraryScenarioTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryScenarioIndustries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "LibraryControlTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryControlIndustries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndustryId1",
                table: "FrameworkIndustries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Customers",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScenarioTags_TagId1",
                table: "LibraryScenarioTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScenarioIndustries_IndustryId1",
                table: "LibraryScenarioIndustries",
                column: "IndustryId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlTags_TagId1",
                table: "LibraryControlTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlIndustries_IndustryId1",
                table: "LibraryControlIndustries",
                column: "IndustryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Frameworks_Name_Version",
                table: "Frameworks",
                columns: new[] { "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_FrameworkIndustries_IndustryId1",
                table: "FrameworkIndustries",
                column: "IndustryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "UX_Customers_Name_Active",
                table: "Customers",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlTags_Tags_TagId1",
                table: "LibraryControlTags",
                column: "TagId1",
                principalTable: "Tags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioTags_Tags_TagId1",
                table: "LibraryScenarioTags",
                column: "TagId1",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
