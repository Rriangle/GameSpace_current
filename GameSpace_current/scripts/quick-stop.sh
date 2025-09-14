#!/bin/bash

# Quick stop script for GameSpace
# Usage: ./scripts/quick-stop.sh

set -e

echo "🛑 GameSpace Quick Stop"
echo "======================="
echo

# Stop services
echo "Stopping GameSpace services..."
docker-compose down

echo "✅ GameSpace stopped successfully!"
echo
echo "To start again, run: ./scripts/quick-start.sh"