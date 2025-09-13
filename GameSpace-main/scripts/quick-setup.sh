#!/bin/bash

# Quick setup script for GameSpace
# Usage: ./scripts/quick-setup.sh

set -e

echo "🚀 GameSpace Quick Setup"
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

# Create necessary directories
echo "Creating necessary directories..."
mkdir -p logs
mkdir -p backups
mkdir -p ssl
mkdir -p nginx

# Build Docker images
echo "Building Docker images..."
docker-compose build

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
echo "🎉 GameSpace setup completed successfully!"
echo "=========================================="
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"
echo
echo "Quick Commands:"
echo "==============="
echo "  Start:     ./scripts/quick-start.sh"
echo "  Stop:      ./scripts/quick-stop.sh"
echo "  Restart:   ./scripts/quick-restart.sh"
echo "  Status:    ./scripts/quick-status.sh"
echo "  Logs:      ./scripts/quick-logs.sh"
echo "  Monitor:   ./scripts/quick-monitor.sh"
echo "  Help:      ./scripts/quick-help.sh"
echo
echo "For advanced management, see scripts/README.md"