#!/bin/bash

# Log viewing script for GameSpace
# Usage: ./scripts/logs.sh [service] [lines]

set -e

SERVICE=${1:-gamespace-app}
LINES=${2:-100}

echo "Viewing logs for $SERVICE (last $LINES lines)..."

case $SERVICE in
    "app"|"gamespace-app")
        docker-compose logs --tail=$LINES -f gamespace-app
        ;;
    "db"|"sqlserver")
        docker-compose logs --tail=$LINES -f sqlserver
        ;;
    "redis")
        docker-compose logs --tail=$LINES -f redis
        ;;
    "nginx")
        docker-compose logs --tail=$LINES -f nginx
        ;;
    "all")
        docker-compose logs --tail=$LINES -f
        ;;
    *)
        echo "Available services: app, db, redis, nginx, all"
        echo "Usage: $0 [service] [lines]"
        exit 1
        ;;
esac