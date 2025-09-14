#!/bin/bash

# Restart script for GameSpace
# Usage: ./scripts/restart.sh [environment]

set -e

ENVIRONMENT=${1:-development}

echo "Restarting GameSpace for $ENVIRONMENT environment..."

# Stop services
echo "1. Stopping services..."
./scripts/stop.sh $ENVIRONMENT

# Wait a moment
echo "2. Waiting..."
sleep 5

# Start services
echo "3. Starting services..."
./scripts/start.sh $ENVIRONMENT

echo "🎉 GameSpace restarted successfully!"