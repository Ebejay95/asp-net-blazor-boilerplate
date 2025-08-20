using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersAndUpdateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Customers table
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: false),
                    RevenuePerYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            // Add new columns to Users table
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Users",
                type: "uuid",
                nullable: true);

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Industry",
                table: "Customers",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive",
                table: "Customers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerId",
                table: "Users",
                column: "CustomerId");

            // Create foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Customers_CustomerId",
                table: "Users",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Customers_CustomerId",
                table: "Users");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Users_CustomerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Name",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Industry",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsActive",
                table: "Customers");

            // Remove columns from Users table
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Users");

            // Drop Customers table
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
