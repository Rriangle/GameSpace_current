#!/bin/bash

# Quick start script for GameSpace
# Usage: ./scripts/quick-start.sh

set -e

echo "🚀 GameSpace Quick Start"
echo "========================"
echo

# Check prerequisites
echo "Checking prerequisites..."

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    echo "Visit: https://docs.docker.com/compose/install/"
    exit 1
fi

echo "✅ Prerequisites check passed"
echo

# Start services
echo "Starting GameSpace services..."
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
echo "🎉 GameSpace is now running!"
echo "============================"
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"
echo
echo "Useful commands:"
echo "  View logs: docker-compose logs -f"
echo "  Stop: docker-compose down"
echo "  Restart: docker-compose restart"
echo "  Status: docker-compose ps"
echo
echo "For advanced management, use the scripts in the scripts/ directory."