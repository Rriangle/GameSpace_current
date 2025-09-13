#!/bin/bash

# Start script for GameSpace
# Usage: ./scripts/start.sh [environment]

set -e

ENVIRONMENT=${1:-development}

echo "Starting GameSpace for $ENVIRONMENT environment..."

# Check if environment file exists
if [ -f ".env.$ENVIRONMENT" ]; then
    echo "Loading environment variables from .env.$ENVIRONMENT"
    export $(cat .env.$ENVIRONMENT | grep -v '^#' | xargs)
fi

# Create necessary directories
echo "Creating necessary directories..."
mkdir -p logs
mkdir -p backups
mkdir -p ssl

# Start services
echo "Starting services..."
docker-compose -f docker-compose.$ENVIRONMENT.yml up -d

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 30

# Run health check
echo "Running health check..."
./scripts/health-check.sh $ENVIRONMENT

echo "🎉 GameSpace started successfully!"
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"