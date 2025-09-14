#!/bin/bash

# Quick update script for GameSpace
# Usage: ./scripts/quick-update.sh

set -e

echo "🔄 GameSpace Quick Update"
echo "========================="
echo

# Pull latest changes
echo "Pulling latest changes..."
git pull origin main

# Build new image
echo "Building new Docker image..."
docker-compose build

# Stop current containers
echo "Stopping current containers..."
docker-compose down

# Start updated containers
echo "Starting updated containers..."
docker-compose up -d

# Wait for services
echo "Waiting for services to be ready..."
sleep 30

# Check health
echo "Checking service health..."
if curl -f -s "http://localhost/api/health" > /dev/null; then
    echo "✅ Application is healthy"
else
    echo "❌ Application is not responding"
    echo "Please check the logs: docker-compose logs"
    exit 1
fi

echo
echo "🎉 GameSpace updated successfully!"
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"