#!/bin/bash

# Quick backup script for GameSpace
# Usage: ./scripts/quick-backup.sh

set -e

BACKUP_NAME=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="./backups"

echo "💾 GameSpace Quick Backup"
echo "========================="
echo

# Create backup directory
mkdir -p $BACKUP_DIR

# Create backup
echo "Creating backup: $BACKUP_NAME"
tar -czf $BACKUP_DIR/quick_backup_$BACKUP_NAME.tar.gz \
    --exclude=node_modules \
    --exclude=.git \
    --exclude=logs \
    --exclude=backups \
    .

echo "✅ Backup completed: $BACKUP_DIR/quick_backup_$BACKUP_NAME.tar.gz"
echo
echo "To restore this backup:"
echo "  tar -xzf $BACKUP_DIR/quick_backup_$BACKUP_NAME.tar.gz"