using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add2FAFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFABackupCodes",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Comma-separated backup codes for 2FA recovery");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TwoFAEnabledAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Timestamp when 2FA was first enabled");

            migrationBuilder.AddColumn<string>(
                name: "TwoFASecret",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "Base32-encoded TOTP secret for two-factor authentication");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users",
                column: "PasswordResetToken");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TwoFASecret",
                table: "Users",
                column: "TwoFASecret");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TwoFASecret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFABackupCodes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFAEnabledAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFASecret",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
