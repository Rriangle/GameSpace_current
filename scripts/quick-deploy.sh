#!/bin/bash

# Quick deploy script for GameSpace
# Usage: ./scripts/quick-deploy.sh

set -e

echo "🚀 GameSpace Quick Deploy"
echo "========================="
echo

# Check if we're in the right directory
if [ ! -f "docker-compose.yml" ]; then
    echo "❌ docker-compose.yml not found. Please run this script from the GameSpace root directory."
    exit 1
fi

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Build and start services
echo "Building and starting services..."
docker-compose up -d --build

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
echo "🎉 GameSpace deployed successfully!"
echo "=================================="
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"
echo
echo "Useful commands:"
echo "  View logs: docker-compose logs -f"
echo "  Stop: docker-compose down"
echo "  Restart: docker-compose restart"
echo "  Status: docker-compose ps"