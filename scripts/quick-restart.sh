#!/bin/bash

# Quick restart script for GameSpace
# Usage: ./scripts/quick-restart.sh

set -e

echo "🔄 GameSpace Quick Restart"
echo "=========================="
echo

# Stop services
echo "Stopping services..."
docker-compose down

# Wait a moment
echo "Waiting..."
sleep 5

# Start services
echo "Starting services..."
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
echo "🎉 GameSpace restarted successfully!"
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"