#!/bin/bash

# Update script for GameSpace
# Usage: ./scripts/update.sh [environment]

set -e

ENVIRONMENT=${1:-production}

echo "Updating GameSpace for $ENVIRONMENT environment..."

# Pull latest changes
echo "1. Pulling latest changes..."
git pull origin main

# Build new image
echo "2. Building new Docker image..."
docker-compose -f docker-compose.$ENVIRONMENT.yml build

# Stop current containers
echo "3. Stopping current containers..."
docker-compose -f docker-compose.$ENVIRONMENT.yml down

# Start updated containers
echo "4. Starting updated containers..."
docker-compose -f docker-compose.$ENVIRONMENT.yml up -d

# Wait for services to be ready
echo "5. Waiting for services to be ready..."
sleep 30

# Run health check
echo "6. Running health check..."
./scripts/health-check.sh $ENVIRONMENT

echo "🎉 Update completed successfully!"