-- Add Category column to MonolithPackages table
-- This migration adds the Category field for grouping packages (Network, VPN, etc.)

-- Check if column already exists (idempotent)
-- If running on SQLite, you can run this directly:
ALTER TABLE MonolithPackages ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other';

-- Note: SQLite doesn't support checking if a column exists before adding it
-- If the column already exists, this will fail with "duplicate column name"
-- In that case, the column is already added and you can ignore the error
