#!/bin/bash
# Script to apply the Category column migration to the production database

set -e

echo "Applying Category column migration..."

# Option 1: Using EF Core migrations (recommended)
cd "$(dirname "$0")/../src" || exit 1

# Check if dotnet-ef is installed
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    echo "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
fi

# Apply migrations
echo "Applying database migrations..."
dotnet ef database update

echo "Migration applied successfully!"

# Option 2: Manual SQL (if EF Core doesn't work)
# Uncomment the following if you need to apply manually:
# DB_FILE="${DB_FILE:-monolith_updates.db}"
# if [ -f "$DB_FILE" ]; then
#     echo "Applying SQL migration manually..."
#     sqlite3 "$DB_FILE" <<EOF
# ALTER TABLE MonolithPackages ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other';
# EOF
#     echo "Manual migration applied!"
# else
#     echo "Database file not found: $DB_FILE"
#     exit 1
# fi
