#!/bin/bash

# Quick cleanup script for GameSpace
# Usage: ./scripts/quick-cleanup.sh

set -e

echo "🧹 GameSpace Quick Cleanup"
echo "=========================="
echo

# Stop services
echo "Stopping services..."
docker-compose down

# Remove unused images
echo "Removing unused Docker images..."
docker image prune -f

# Remove unused volumes
echo "Removing unused volumes..."
docker volume prune -f

# Remove unused networks
echo "Removing unused networks..."
docker network prune -f

# Clean up old logs
echo "Cleaning up old logs..."
find ./logs -name "*.log" -mtime +7 -delete 2>/dev/null || true

# Clean up old backups
echo "Cleaning up old backups..."
find ./backups -name "*.tar.gz" -mtime +7 -delete 2>/dev/null || true

echo "✅ Cleanup completed successfully!"
echo
echo "To start again, run: ./scripts/quick-start.sh"