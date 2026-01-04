using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonolithUpdateSite.Migrations;

/// <summary>
/// Adds a Category field to MonolithPackages for grouping packages (Network, VPN, etc).
/// </summary>
public partial class AddPackageCategory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "MonolithPackages",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "Other");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Category",
            table: "MonolithPackages");
    }
}

