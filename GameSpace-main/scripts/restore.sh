#!/bin/bash

# Complete restore script for GameSpace
# Usage: ./scripts/restore.sh [backup_name] [environment]

set -e

BACKUP_NAME=${1}
ENVIRONMENT=${2:-production}
BACKUP_DIR="./backups"

if [ -z "$BACKUP_NAME" ]; then
    echo "Usage: $0 <backup_name> [environment]"
    echo "Available backups:"
    ls -la $BACKUP_DIR/manifest_*.txt 2>/dev/null || echo "No backups found"
    exit 1
fi

echo "Restoring GameSpace from backup $BACKUP_NAME for $ENVIRONMENT environment..."

# Check if backup exists
if [ ! -f "$BACKUP_DIR/manifest_$BACKUP_NAME.txt" ]; then
    echo "Error: Backup manifest not found: $BACKUP_DIR/manifest_$BACKUP_NAME.txt"
    exit 1
fi

# Display backup information
echo "Backup information:"
cat $BACKUP_DIR/manifest_$BACKUP_NAME.txt

# Confirm restore
read -p "Are you sure you want to restore from this backup? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Restore cancelled."
    exit 1
fi

# 1. Stop current services
echo "1. Stopping current services..."
docker-compose -f docker-compose.$ENVIRONMENT.yml down

# 2. Restore application files
echo "2. Restoring application files..."
if [ -f "$BACKUP_DIR/app_$BACKUP_NAME.tar.gz" ]; then
    tar -xzf $BACKUP_DIR/app_$BACKUP_NAME.tar.gz
    echo "Application files restored"
else
    echo "Warning: Application backup not found, skipping..."
fi

# 3. Restore database
echo "3. Restoring database..."
if [ -f "$BACKUP_DIR/${BACKUP_NAME}.bak.gz" ]; then
    gunzip $BACKUP_DIR/${BACKUP_NAME}.bak.gz
    ./scripts/restore-db.sh ${BACKUP_NAME}.bak $ENVIRONMENT
    echo "Database restored"
else
    echo "Warning: Database backup not found, skipping..."
fi

# 4. Restore logs
echo "4. Restoring logs..."
if [ -f "$BACKUP_DIR/logs_$BACKUP_NAME.tar.gz" ]; then
    tar -xzf $BACKUP_DIR/logs_$BACKUP_NAME.tar.gz
    echo "Logs restored"
else
    echo "Warning: Logs backup not found, skipping..."
fi

# 5. Start services
echo "5. Starting services..."
docker-compose -f docker-compose.$ENVIRONMENT.yml up -d

# 6. Wait for services to be ready
echo "6. Waiting for services to be ready..."
sleep 30

# 7. Run health check
echo "7. Running health check..."
./scripts/health-check.sh $ENVIRONMENT

echo "🎉 Restore completed successfully!"