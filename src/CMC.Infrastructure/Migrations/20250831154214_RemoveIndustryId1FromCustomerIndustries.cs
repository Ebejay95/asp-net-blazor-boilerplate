using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIndustryId1FromCustomerIndustries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIndustries_Industries_IndustryId1",
                table: "CustomerIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Customers_CustomerId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlFrameworks_LibraryControls_ControlId",
                table: "LibraryControlFrameworks");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlIndustries_LibraryControls_ControlId",
                table: "LibraryControlIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryControls_ControlId",
                table: "LibraryControlScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_ScenarioId",
                table: "LibraryControlScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioIndustries_LibraryScenarios_ScenarioId",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios");

            migrationBuilder.DropIndex(
                name: "IX_ReportDefinitions_CustomerId_Name",
                table: "ReportDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControls_Tag",
                table: "LibraryControls");

            migrationBuilder.DropIndex(
                name: "IX_Frameworks_IsDeleted",
                table: "Frameworks");

            migrationBuilder.DropIndex(
                name: "IX_CustomerIndustries_IndustryId1",
                table: "CustomerIndustries");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "LibraryScenarios");

            migrationBuilder.DropColumn(
                name: "FrequencyEffect",
                table: "LibraryControlScenarios");

            migrationBuilder.DropColumn(
                name: "ImpactEffect",
                table: "LibraryControlScenarios");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "LibraryControls");

            migrationBuilder.DropColumn(
                name: "IndustryId1",
                table: "CustomerIndustries");

            migrationBuilder.RenameColumn(
                name: "ScenarioId",
                table: "LibraryScenarioIndustries",
                newName: "LibraryScenarioId");

            migrationBuilder.RenameColumn(
                name: "ScenarioId",
                table: "LibraryControlScenarios",
                newName: "LibraryScenarioId");

            migrationBuilder.RenameColumn(
                name: "ControlId",
                table: "LibraryControlScenarios",
                newName: "LibraryControlId");

            migrationBuilder.RenameIndex(
                name: "IX_LibraryControlScenarios_ScenarioId",
                table: "LibraryControlScenarios",
                newName: "IX_LibraryControlScenarios_LibraryScenarioId");

            migrationBuilder.RenameColumn(
                name: "ControlId",
                table: "LibraryControlIndustries",
                newName: "LibraryControlId");

            migrationBuilder.RenameColumn(
                name: "ControlId",
                table: "LibraryControlFrameworks",
                newName: "LibraryControlId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ToDos",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<decimal>(
                name: "ImpactPctRevenue",
                table: "Scenarios",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualFrequency",
                table: "Scenarios",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "RiskAcceptances",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Reports",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ReportDefinitions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "ImpactPctRevenue",
                table: "LibraryScenarios",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualFrequency",
                table: "LibraryScenarios",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryScenarioIndustries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryScenarioId1",
                table: "LibraryControlScenarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryControlIndustries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "FrameworkIndustries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "HashSha256",
                table: "Evidence",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Evidence",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Controls",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Freshness",
                table: "Controls",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "EvidenceWeight",
                table: "Controls",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Coverage",
                table: "Controls",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.CreateTable(
                name: "ControlScenarios",
                columns: table => new
                {
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlScenarios", x => new { x.ControlId, x.ScenarioId });
                    table.ForeignKey(
                        name: "FK_ControlScenarios_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlScenarios_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionedControlMaps",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LibraryControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionedControlMaps", x => new { x.CustomerId, x.LibraryControlId, x.ScenarioId });
                    table.ForeignKey(
                        name: "FK_ProvisionedControlMaps_Controls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvisionedControlMaps_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvisionedControlMaps_LibraryControls_LibraryControlId",
                        column: x => x.LibraryControlId,
                        principalTable: "LibraryControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvisionedControlMaps_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProvisionedScenarioMaps",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LibraryScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisionedScenarioMaps", x => new { x.CustomerId, x.LibraryScenarioId });
                    table.ForeignKey(
                        name: "FK_ProvisionedScenarioMaps_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvisionedScenarioMaps_LibraryScenarios_LibraryScenarioId",
                        column: x => x.LibraryScenarioId,
                        principalTable: "LibraryScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvisionedScenarioMaps_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryControlTags",
                columns: table => new
                {
                    LibraryControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryControlTags", x => new { x.LibraryControlId, x.TagId });
                    table.ForeignKey(
                        name: "FK_LibraryControlTags_LibraryControls_LibraryControlId",
                        column: x => x.LibraryControlId,
                        principalTable: "LibraryControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryControlTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryControlTags_Tags_TagId1",
                        column: x => x.TagId1,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryScenarioTags",
                columns: table => new
                {
                    LibraryScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScenarioTags", x => new { x.LibraryScenarioId, x.TagId });
                    table.ForeignKey(
                        name: "FK_LibraryScenarioTags_LibraryScenarios_LibraryScenarioId",
                        column: x => x.LibraryScenarioId,
                        principalTable: "LibraryScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryScenarioTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryScenarioTags_Tags_TagId1",
                        column: x => x.TagId1,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScenarioTags",
                columns: table => new
                {
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTags", x => new { x.ScenarioId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ScenarioTags_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioTags_Tags_TagId1",
                        column: x => x.TagId1,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToDos_ControlId",
                table: "ToDos",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ToDos_ControlId_DependsOnTaskId",
                table: "ToDos",
                columns: new[] { "ControlId", "DependsOnTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_ToDos_DependsOnTaskId",
                table: "ToDos",
                column: "DependsOnTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ToDos_Status_IsDeleted",
                table: "ToDos",
                columns: new[] { "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "UX_Scenarios_Cust_LibScenario",
                table: "Scenarios",
                columns: new[] { "CustomerId", "LibraryScenarioId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAcceptances_ControlId",
                table: "RiskAcceptances",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAcceptances_ExpiresAt",
                table: "RiskAcceptances",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CustomerId_IsDeleted",
                table: "Reports",
                columns: new[] { "CustomerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DefinitionId",
                table: "Reports",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "UX_Reports_Def_Period_Cust",
                table: "Reports",
                columns: new[] { "DefinitionId", "PeriodStart", "PeriodEnd", "CustomerId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_Period",
                table: "Reports",
                sql: "\"PeriodEnd\" >= \"PeriodStart\"");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_CustomerId_Name",
                table: "ReportDefinitions",
                columns: new[] { "CustomerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScenarios_Name",
                table: "LibraryScenarios",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios",
                column: "LibraryScenarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControls_IsDeleted",
                table: "LibraryControls",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControls_Name_IsDeleted",
                table: "LibraryControls",
                columns: new[] { "Name", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_CollectedAt",
                table: "Evidence",
                column: "CollectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Controls_Status",
                table: "Controls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ControlScenarios_ScenarioId",
                table: "ControlScenarios",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlTags_TagId",
                table: "LibraryControlTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControlTags_TagId1",
                table: "LibraryControlTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScenarioTags_TagId",
                table: "LibraryScenarioTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScenarioTags_TagId1",
                table: "LibraryScenarioTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionedControlMaps_ControlId",
                table: "ProvisionedControlMaps",
                column: "ControlId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionedControlMaps_LibraryControlId",
                table: "ProvisionedControlMaps",
                column: "LibraryControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionedControlMaps_ScenarioId",
                table: "ProvisionedControlMaps",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionedScenarioMaps_LibraryScenarioId",
                table: "ProvisionedScenarioMaps",
                column: "LibraryScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisionedScenarioMaps_ScenarioId",
                table: "ProvisionedScenarioMaps",
                column: "ScenarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTags_TagId",
                table: "ScenarioTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTags_TagId1",
                table: "ScenarioTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlFrameworks_LibraryControls_LibraryControlId",
                table: "LibraryControlFrameworks",
                column: "LibraryControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlIndustries_LibraryControls_LibraryControlId",
                table: "LibraryControlIndustries",
                column: "LibraryControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryControls_LibraryControlId",
                table: "LibraryControlScenarios",
                column: "LibraryControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId",
                table: "LibraryControlScenarios",
                column: "LibraryScenarioId",
                principalTable: "LibraryScenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios",
                column: "LibraryScenarioId1",
                principalTable: "LibraryScenarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioIndustries_LibraryScenarios_LibraryScenarioId",
                table: "LibraryScenarioIndustries",
                column: "LibraryScenarioId",
                principalTable: "LibraryScenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportDefinitions_Customers_CustomerId",
                table: "ReportDefinitions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Customers_CustomerId",
                table: "Reports",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportDefinitions_DefinitionId",
                table: "Reports",
                column: "DefinitionId",
                principalTable: "ReportDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAcceptances_Controls_ControlId",
                table: "RiskAcceptances",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAcceptances_Customers_CustomerId",
                table: "RiskAcceptances",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ToDos_Controls_ControlId",
                table: "ToDos",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ToDos_ToDos_DependsOnTaskId",
                table: "ToDos",
                column: "DependsOnTaskId",
                principalTable: "ToDos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls");

            migrationBuilder.DropForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlFrameworks_LibraryControls_LibraryControlId",
                table: "LibraryControlFrameworks");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlIndustries_LibraryControls_LibraryControlId",
                table: "LibraryControlIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryControls_LibraryControlId",
                table: "LibraryControlScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId",
                table: "LibraryControlScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScenarioIndustries_LibraryScenarios_LibraryScenarioId",
                table: "LibraryScenarioIndustries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportDefinitions_Customers_CustomerId",
                table: "ReportDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Customers_CustomerId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportDefinitions_DefinitionId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAcceptances_Controls_ControlId",
                table: "RiskAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAcceptances_Customers_CustomerId",
                table: "RiskAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDos_Controls_ControlId",
                table: "ToDos");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDos_ToDos_DependsOnTaskId",
                table: "ToDos");

            migrationBuilder.DropTable(
                name: "ControlScenarios");

            migrationBuilder.DropTable(
                name: "LibraryControlTags");

            migrationBuilder.DropTable(
                name: "LibraryScenarioTags");

            migrationBuilder.DropTable(
                name: "ProvisionedControlMaps");

            migrationBuilder.DropTable(
                name: "ProvisionedScenarioMaps");

            migrationBuilder.DropTable(
                name: "ScenarioTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_ToDos_ControlId",
                table: "ToDos");

            migrationBuilder.DropIndex(
                name: "IX_ToDos_ControlId_DependsOnTaskId",
                table: "ToDos");

            migrationBuilder.DropIndex(
                name: "IX_ToDos_DependsOnTaskId",
                table: "ToDos");

            migrationBuilder.DropIndex(
                name: "IX_ToDos_Status_IsDeleted",
                table: "ToDos");

            migrationBuilder.DropIndex(
                name: "UX_Scenarios_Cust_LibScenario",
                table: "Scenarios");

            migrationBuilder.DropIndex(
                name: "IX_RiskAcceptances_ControlId",
                table: "RiskAcceptances");

            migrationBuilder.DropIndex(
                name: "IX_RiskAcceptances_ExpiresAt",
                table: "RiskAcceptances");

            migrationBuilder.DropIndex(
                name: "IX_Reports_CustomerId_IsDeleted",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DefinitionId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "UX_Reports_Def_Period_Cust",
                table: "Reports");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_Period",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ReportDefinitions_CustomerId_Name",
                table: "ReportDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_LibraryScenarios_Name",
                table: "LibraryScenarios");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControlScenarios_LibraryScenarioId1",
                table: "LibraryControlScenarios");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControls_IsDeleted",
                table: "LibraryControls");

            migrationBuilder.DropIndex(
                name: "IX_LibraryControls_Name_IsDeleted",
                table: "LibraryControls");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_CollectedAt",
                table: "Evidence");

            migrationBuilder.DropIndex(
                name: "IX_Controls_Status",
                table: "Controls");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "RiskAcceptances");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ReportDefinitions");

            migrationBuilder.DropColumn(
                name: "LibraryScenarioId1",
                table: "LibraryControlScenarios");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Evidence");

            migrationBuilder.RenameColumn(
                name: "LibraryScenarioId",
                table: "LibraryScenarioIndustries",
                newName: "ScenarioId");

            migrationBuilder.RenameColumn(
                name: "LibraryScenarioId",
                table: "LibraryControlScenarios",
                newName: "ScenarioId");

            migrationBuilder.RenameColumn(
                name: "LibraryControlId",
                table: "LibraryControlScenarios",
                newName: "ControlId");

            migrationBuilder.RenameIndex(
                name: "IX_LibraryControlScenarios_LibraryScenarioId",
                table: "LibraryControlScenarios",
                newName: "IX_LibraryControlScenarios_ScenarioId");

            migrationBuilder.RenameColumn(
                name: "LibraryControlId",
                table: "LibraryControlIndustries",
                newName: "ControlId");

            migrationBuilder.RenameColumn(
                name: "LibraryControlId",
                table: "LibraryControlFrameworks",
                newName: "ControlId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ToDos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<decimal>(
                name: "ImpactPctRevenue",
                table: "Scenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualFrequency",
                table: "Scenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Scenarios",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "ImpactPctRevenue",
                table: "LibraryScenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualFrequency",
                table: "LibraryScenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "LibraryScenarios",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryScenarioIndustries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FrequencyEffect",
                table: "LibraryControlScenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ImpactEffect",
                table: "LibraryControlScenarios",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "LibraryControls",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "LibraryControlIndustries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IndustryId1",
                table: "FrameworkIndustries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HashSha256",
                table: "Evidence",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<Guid>(
                name: "IndustryId1",
                table: "CustomerIndustries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Controls",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,4)",
                oldPrecision: 9,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Freshness",
                table: "Controls",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "EvidenceWeight",
                table: "Controls",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Coverage",
                table: "Controls",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_CustomerId_Name",
                table: "ReportDefinitions",
                columns: new[] { "CustomerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryControls_Tag",
                table: "LibraryControls",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Frameworks_IsDeleted",
                table: "Frameworks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerIndustries_IndustryId1",
                table: "CustomerIndustries",
                column: "IndustryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerIndustries_Industries_IndustryId1",
                table: "CustomerIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Customers_CustomerId",
                table: "Evidence",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FrameworkIndustries_Industries_IndustryId1",
                table: "FrameworkIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlFrameworks_LibraryControls_ControlId",
                table: "LibraryControlFrameworks",
                column: "ControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlIndustries_Industries_IndustryId1",
                table: "LibraryControlIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlIndustries_LibraryControls_ControlId",
                table: "LibraryControlIndustries",
                column: "ControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryControls_ControlId",
                table: "LibraryControlScenarios",
                column: "ControlId",
                principalTable: "LibraryControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryControlScenarios_LibraryScenarios_ScenarioId",
                table: "LibraryControlScenarios",
                column: "ScenarioId",
                principalTable: "LibraryScenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioIndustries_Industries_IndustryId1",
                table: "LibraryScenarioIndustries",
                column: "IndustryId1",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScenarioIndustries_LibraryScenarios_ScenarioId",
                table: "LibraryScenarioIndustries",
                column: "ScenarioId",
                principalTable: "LibraryScenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
