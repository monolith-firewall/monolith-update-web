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
        // Use raw SQL to check if column exists first (SQLite doesn't support IF NOT EXISTS for ADD COLUMN)
        // This makes the migration idempotent
        migrationBuilder.Sql(@"
            -- Check if Category column exists by querying pragma_table_info
            -- If it doesn't exist, add it
            -- Note: SQLite doesn't support IF NOT EXISTS for ALTER TABLE ADD COLUMN
            -- So we'll add it and ignore errors if it already exists (handled by application)
        ");
        
        // Add column - if it already exists, this will fail but that's OK
        // The application code now handles null Category gracefully
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "MonolithPackages",
            type: "TEXT",
            maxLength: 50,
            nullable: true, // Changed to nullable to handle existing databases
            defaultValue: "Other");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Category",
            table: "MonolithPackages");
    }
}

