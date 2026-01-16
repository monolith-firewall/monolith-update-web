#!/bin/bash
# Quick fix script to add Category column to MonolithPackages table
# This can be run directly on the production server or in a Docker container

set -e

DB_FILE="${DB_FILE:-/app/data/monolith_updates.db}"

# If running in Docker, the DB might be at a different location
if [ ! -f "$DB_FILE" ]; then
    # Try common locations
    for loc in "/app/data/monolith_updates.db" "/app/monolith_updates.db" "./monolith_updates.db"; do
        if [ -f "$loc" ]; then
            DB_FILE="$loc"
            break
        fi
    done
fi

if [ ! -f "$DB_FILE" ]; then
    echo "Error: Database file not found at $DB_FILE"
    echo "Please set DB_FILE environment variable to the correct path"
    exit 1
fi

echo "Adding Category column to MonolithPackages table..."
echo "Database: $DB_FILE"

# Check if sqlite3 is available
if command -v sqlite3 &> /dev/null; then
    sqlite3 "$DB_FILE" <<EOF
-- Add Category column if it doesn't exist
-- Note: SQLite doesn't support IF NOT EXISTS for ALTER TABLE ADD COLUMN
-- So we'll try to add it and ignore errors if it already exists
ALTER TABLE MonolithPackages ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other';
EOF
    echo "Category column added successfully!"
    
    # Verify
    echo "Verifying column exists..."
    sqlite3 "$DB_FILE" "PRAGMA table_info(MonolithPackages);" | grep -q Category && echo "✓ Category column verified" || echo "✗ Column not found"
else
    echo "Error: sqlite3 command not found"
    echo "Please install sqlite3 or use the EF Core migration method"
    exit 1
fi
