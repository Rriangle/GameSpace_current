#!/bin/bash

# Stop script for GameSpace
# Usage: ./scripts/stop.sh [environment]

set -e

ENVIRONMENT=${1:-development}

echo "Stopping GameSpace for $ENVIRONMENT environment..."

# Stop services
echo "Stopping services..."
docker-compose -f docker-compose.$ENVIRONMENT.yml down

echo "🎉 GameSpace stopped successfully!"