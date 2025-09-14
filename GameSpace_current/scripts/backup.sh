#!/bin/bash

# Complete backup script for GameSpace
# Usage: ./scripts/backup.sh [environment]

set -e

ENVIRONMENT=${1:-production}
BACKUP_NAME=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="./backups"

echo "Creating complete backup for $ENVIRONMENT environment..."

# Create backup directory
mkdir -p $BACKUP_DIR

# 1. Database backup
echo "1. Backing up database..."
./scripts/backup-db.sh $ENVIRONMENT $BACKUP_NAME

# 2. Application files backup
echo "2. Backing up application files..."
tar -czf $BACKUP_DIR/app_$BACKUP_NAME.tar.gz \
    --exclude=node_modules \
    --exclude=.git \
    --exclude=logs \
    --exclude=backups \
    .

# 3. Configuration backup
echo "3. Backing up configuration..."
cp docker-compose.$ENVIRONMENT.yml $BACKUP_DIR/
cp .env $BACKUP_DIR/ 2>/dev/null || true

# 4. Logs backup (last 7 days)
echo "4. Backing up recent logs..."
if [ -d "./logs" ]; then
    tar -czf $BACKUP_DIR/logs_$BACKUP_NAME.tar.gz logs/
fi

# 5. Create backup manifest
echo "5. Creating backup manifest..."
cat > $BACKUP_DIR/manifest_$BACKUP_NAME.txt << EOF
GameSpace Backup Manifest
========================
Date: $(date)
Environment: $ENVIRONMENT
Backup Name: $BACKUP_NAME

Files:
- Database: ${BACKUP_NAME}.bak.gz
- Application: app_${BACKUP_NAME}.tar.gz
- Configuration: docker-compose.${ENVIRONMENT}.yml
- Logs: logs_${BACKUP_NAME}.tar.gz

Restore Instructions:
1. Extract application files: tar -xzf app_${BACKUP_NAME}.tar.gz
2. Restore database: ./scripts/restore-db.sh ${BACKUP_NAME}.bak.gz
3. Start services: docker-compose -f docker-compose.${ENVIRONMENT}.yml up -d
EOF

echo "🎉 Backup completed successfully!"
echo "Backup location: $BACKUP_DIR/"
echo "Backup name: $BACKUP_NAME"