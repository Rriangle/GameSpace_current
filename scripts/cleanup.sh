#!/bin/bash

# Cleanup script for GameSpace
# Usage: ./scripts/cleanup.sh [environment]

set -e

ENVIRONMENT=${1:-production}

echo "Cleaning up GameSpace for $ENVIRONMENT environment..."

# Stop and remove containers
echo "1. Stopping and removing containers..."
docker-compose -f docker-compose.$ENVIRONMENT.yml down

# Remove unused images
echo "2. Removing unused Docker images..."
docker image prune -f

# Remove unused volumes
echo "3. Removing unused volumes..."
docker volume prune -f

# Remove unused networks
echo "4. Removing unused networks..."
docker network prune -f

# Clean up old logs
echo "5. Cleaning up old logs..."
find ./logs -name "*.log" -mtime +30 -delete 2>/dev/null || true

# Clean up old backups
echo "6. Cleaning up old backups..."
find ./backups -name "*.bak*" -mtime +7 -delete 2>/dev/null || true

echo "🎉 Cleanup completed successfully!"