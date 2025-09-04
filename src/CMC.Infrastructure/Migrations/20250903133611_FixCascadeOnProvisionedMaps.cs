using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeOnProvisionedMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Controls_ControlId",
                table: "ProvisionedControlMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Scenarios_ScenarioId",
                table: "ProvisionedControlMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedScenarioMaps_Scenarios_ScenarioId",
                table: "ProvisionedScenarioMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Controls_ControlId",
                table: "ProvisionedControlMaps",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Scenarios_ScenarioId",
                table: "ProvisionedControlMaps",
                column: "ScenarioId",
                principalTable: "Scenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedScenarioMaps_Scenarios_ScenarioId",
                table: "ProvisionedScenarioMaps",
                column: "ScenarioId",
                principalTable: "Scenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Controls_ControlId",
                table: "ProvisionedControlMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Scenarios_ScenarioId",
                table: "ProvisionedControlMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedScenarioMaps_Scenarios_ScenarioId",
                table: "ProvisionedScenarioMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Controls_ControlId",
                table: "ProvisionedControlMaps",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Scenarios_ScenarioId",
                table: "ProvisionedControlMaps",
                column: "ScenarioId",
                principalTable: "Scenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedScenarioMaps_Scenarios_ScenarioId",
                table: "ProvisionedScenarioMaps",
                column: "ScenarioId",
                principalTable: "Scenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
