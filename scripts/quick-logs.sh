#!/bin/bash

# Quick logs script for GameSpace
# Usage: ./scripts/quick-logs.sh [service] [lines]

set -e

SERVICE=${1:-app}
LINES=${2:-100}

echo "📋 GameSpace Quick Logs"
echo "======================="
echo

case $SERVICE in
    "app"|"gamespace-app")
        echo "Application logs (last $LINES lines):"
        docker-compose logs --tail=$LINES -f gamespace-app
        ;;
    "db"|"sqlserver")
        echo "Database logs (last $LINES lines):"
        docker-compose logs --tail=$LINES -f sqlserver
        ;;
    "redis")
        echo "Redis logs (last $LINES lines):"
        docker-compose logs --tail=$LINES -f redis
        ;;
    "nginx")
        echo "Nginx logs (last $LINES lines):"
        docker-compose logs --tail=$LINES -f nginx
        ;;
    "all")
        echo "All logs (last $LINES lines):"
        docker-compose logs --tail=$LINES -f
        ;;
    *)
        echo "Available services: app, db, redis, nginx, all"
        echo "Usage: $0 [service] [lines]"
        echo "Example: $0 app 50"
        exit 1
        ;;
esac