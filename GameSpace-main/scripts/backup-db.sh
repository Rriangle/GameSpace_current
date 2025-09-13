#!/bin/bash

# Database backup script for GameSpace
# Usage: ./scripts/backup-db.sh [environment] [backup_name]

set -e

# Configuration
ENVIRONMENT=${1:-production}
BACKUP_NAME=${2:-$(date +%Y%m%d_%H%M%S)}
CONTAINER_NAME="gamespace-main-sqlserver-1"
BACKUP_DIR="./backups"
DB_NAME="GameSpaceDatabase"

# Create backup directory if it doesn't exist
mkdir -p $BACKUP_DIR

echo "Starting database backup for $ENVIRONMENT environment..."

# Create backup
docker exec $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "$PROD_DB_PASSWORD" \
  -Q "BACKUP DATABASE [$DB_NAME] TO DISK = '/var/opt/mssql/backup/${BACKUP_NAME}.bak' WITH FORMAT, INIT, NAME = 'GameSpace Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Copy backup from container to host
docker cp $CONTAINER_NAME:/var/opt/mssql/backup/${BACKUP_NAME}.bak $BACKUP_DIR/

echo "Backup completed: $BACKUP_DIR/${BACKUP_NAME}.bak"

# Optional: Compress backup
gzip $BACKUP_DIR/${BACKUP_NAME}.bak
echo "Backup compressed: $BACKUP_DIR/${BACKUP_NAME}.bak.gz"

# Optional: Upload to cloud storage (uncomment and configure as needed)
# aws s3 cp $BACKUP_DIR/${BACKUP_NAME}.bak.gz s3://your-backup-bucket/