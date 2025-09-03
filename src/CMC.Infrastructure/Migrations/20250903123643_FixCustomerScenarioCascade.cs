using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerScenarioCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Scenarios_Customers_CustomerId",
                table: "Scenarios",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
