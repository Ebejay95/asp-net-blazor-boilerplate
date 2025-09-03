using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerControlCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDos_Controls_ControlId",
                table: "ToDos");

            migrationBuilder.AddForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ToDos_Controls_ControlId",
                table: "ToDos",
                column: "ControlId",
                principalTable: "Controls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls");

            migrationBuilder.DropForeignKey(
                name: "FK_ToDos_Controls_ControlId",
                table: "ToDos");

            migrationBuilder.AddForeignKey(
                name: "FK_Controls_Customers_CustomerId",
                table: "Controls",
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
        }
    }
}
