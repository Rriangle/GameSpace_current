#!/bin/bash

# Quick reset script for GameSpace
# Usage: ./scripts/quick-reset.sh

set -e

echo "🔄 GameSpace Quick Reset"
echo "========================"
echo

# Confirm reset
read -p "Are you sure you want to reset GameSpace? This will stop all services and remove data. (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Reset cancelled."
    exit 1
fi

# Stop services
echo "Stopping services..."
docker-compose down

# Remove containers
echo "Removing containers..."
docker-compose rm -f

# Remove volumes
echo "Removing volumes..."
docker volume prune -f

# Remove images
echo "Removing images..."
docker image prune -f

# Remove networks
echo "Removing networks..."
docker network prune -f

# Clean up logs
echo "Cleaning up logs..."
rm -rf logs/*

# Clean up backups
echo "Cleaning up backups..."
rm -rf backups/*

echo
echo "🎉 GameSpace reset completed!"
echo "To start fresh, run: ./scripts/quick-setup.sh"