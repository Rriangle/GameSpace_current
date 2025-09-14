#!/bin/bash

# Quick status script for GameSpace
# Usage: ./scripts/quick-status.sh

set -e

echo "📊 GameSpace Quick Status"
echo "========================="
echo

# Check if services are running
if docker-compose ps | grep -q "Up"; then
    echo "✅ Services are running"
    echo
    echo "Service Status:"
    docker-compose ps
    echo
    
    # Check application health
    if curl -f -s "http://localhost/api/health" > /dev/null; then
        echo "✅ Application is healthy"
        echo "Application URL: http://localhost"
        echo "Admin URL: http://localhost/admin"
        echo "API Health: http://localhost/api/health"
    else
        echo "❌ Application is not responding"
    fi
else
    echo "❌ Services are not running"
    echo "To start, run: ./scripts/quick-start.sh"
fi