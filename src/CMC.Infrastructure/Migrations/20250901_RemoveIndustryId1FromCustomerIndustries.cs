// Migrations/20250901_RemoveIndustryId1FromCustomerIndustries.cs
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
public partial class RemoveIndustryId1FromCustomerIndustries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IndustryId1",
            table: "CustomerIndustries");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "IndustryId1",
            table: "CustomerIndustries",
            type: "uuid",
            nullable: false,
            defaultValueSql: "gen_random_uuid()" // oder mach’s nullable, wenn du nicht zurück willst
        );
    }
}
