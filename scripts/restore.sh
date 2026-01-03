#!/bin/bash
# Restore script for database and update files

set -e

BACKUP_DIR="$(dirname "$0")/../backups"

# Check if backup file is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <backup_file>"
    echo ""
    echo "Available backups:"
    ls -lh "$BACKUP_DIR"/backup_*.tar.gz 2>/dev/null || echo "No backups found"
    exit 1
fi

BACKUP_FILE="$1"

# Check if file exists
if [ ! -f "$BACKUP_DIR/$BACKUP_FILE" ] && [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: Backup file not found: $BACKUP_FILE"
    exit 1
fi

# Use full path if just filename provided
if [ ! -f "$BACKUP_FILE" ]; then
    BACKUP_FILE="$BACKUP_DIR/$BACKUP_FILE"
fi

echo "Restoring from backup: $BACKUP_FILE"
echo "WARNING: This will overwrite existing data!"
read -p "Continue? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo "Restore cancelled"
    exit 0
fi

# Navigate to project root
cd "$(dirname "$0")/.."

# Stop docker if running
if command -v docker-compose &> /dev/null; then
    echo "Stopping docker services..."
    docker-compose down 2>/dev/null || true
fi

# Extract backup
echo "Extracting backup..."
tar -xzf "$BACKUP_FILE"

echo "Restore completed successfully!"
echo "You can now start the application"
