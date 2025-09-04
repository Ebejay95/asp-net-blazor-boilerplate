// src/CMC.Infrastructure/Migrations/20250903_EnsureCascadeOnProvisionedMaps.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    public partial class EnsureCascadeOnProvisionedMaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ProvisionedScenarioMaps.CustomerId -> Customers(Id) : CASCADE
            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedScenarioMaps_Customers_CustomerId",
                table: "ProvisionedScenarioMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedScenarioMaps_Customers_CustomerId",
                table: "ProvisionedScenarioMaps",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // ProvisionedControlMaps.CustomerId -> Customers(Id) : CASCADE
            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Customers_CustomerId",
                table: "ProvisionedControlMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Customers_CustomerId",
                table: "ProvisionedControlMaps",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // zurück auf RESTRICT
            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedScenarioMaps_Customers_CustomerId",
                table: "ProvisionedScenarioMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedScenarioMaps_Customers_CustomerId",
                table: "ProvisionedScenarioMaps",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_ProvisionedControlMaps_Customers_CustomerId",
                table: "ProvisionedControlMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_ProvisionedControlMaps_Customers_CustomerId",
                table: "ProvisionedControlMaps",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
