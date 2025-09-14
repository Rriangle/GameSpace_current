#!/bin/bash

# Database restore script for GameSpace
# Usage: ./scripts/restore-db.sh [backup_file] [environment]

set -e

# Configuration
BACKUP_FILE=${1}
ENVIRONMENT=${2:-production}
CONTAINER_NAME="gamespace-main-sqlserver-1"
BACKUP_DIR="./backups"
DB_NAME="GameSpaceDatabase"

if [ -z "$BACKUP_FILE" ]; then
    echo "Error: Please provide backup file name"
    echo "Usage: $0 <backup_file> [environment]"
    echo "Available backups:"
    ls -la $BACKUP_DIR/*.bak* 2>/dev/null || echo "No backups found"
    exit 1
fi

# Check if backup file exists
if [ ! -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
    echo "Error: Backup file $BACKUP_DIR/$BACKUP_FILE not found"
    exit 1
fi

echo "Starting database restore from $BACKUP_FILE..."

# Copy backup to container
docker cp $BACKUP_DIR/$BACKUP_FILE $CONTAINER_NAME:/var/opt/mssql/backup/

# Restore database
docker exec $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "$PROD_DB_PASSWORD" \
  -Q "RESTORE DATABASE [$DB_NAME] FROM DISK = '/var/opt/mssql/backup/$BACKUP_FILE' WITH REPLACE, STATS = 10"

echo "Database restore completed successfully!"