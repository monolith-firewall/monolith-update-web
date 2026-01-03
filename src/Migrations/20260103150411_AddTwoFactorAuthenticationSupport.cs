using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonolithUpdateSite.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuthenticationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecoveryCodes",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorEnabledAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorSetupCompleted",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TwoFactorEnabled",
                table: "AspNetUsers",
                column: "TwoFactorEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TwoFactorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RecoveryCodes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabledAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorSetupCompleted",
                table: "AspNetUsers");
        }
    }
}
