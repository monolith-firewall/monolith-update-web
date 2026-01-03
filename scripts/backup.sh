#!/bin/bash
# Backup script for database and update files

set -e

# Configuration
BACKUP_DIR="$(dirname "$0")/../backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="backup_${DATE}.tar.gz"

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

echo "Creating backup..."
echo "Backup file: $BACKUP_FILE"

# Navigate to project root
cd "$(dirname "$0")/.."

# Create backup
tar -czf "$BACKUP_DIR/$BACKUP_FILE" \
    --exclude='data/*.db-shm' \
    --exclude='data/*.db-wal' \
    data/ \
    wwwroot/updates/ 2>/dev/null || true

if [ -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
    SIZE=$(du -h "$BACKUP_DIR/$BACKUP_FILE" | cut -f1)
    echo "Backup created successfully: $BACKUP_FILE ($SIZE)"

    # Keep only last 10 backups
    cd "$BACKUP_DIR"
    ls -t backup_*.tar.gz | tail -n +11 | xargs -r rm
    echo "Cleanup: Kept last 10 backups"
else
    echo "Backup failed!"
    exit 1
fi
