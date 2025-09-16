#!/bin/bash

# Status script for GameSpace
# Usage: ./scripts/status.sh [environment]

set -e

ENVIRONMENT=${1:-development}

echo "GameSpace Status for $ENVIRONMENT environment"
echo "============================================="
echo

# Check if services are running
echo "Service Status:"
echo "==============="
if docker-compose -f docker-compose.$ENVIRONMENT.yml ps | grep -q "Up"; then
    echo "✅ Services are running"
    docker-compose -f docker-compose.$ENVIRONMENT.yml ps
else
    echo "❌ Services are not running"
fi
echo

# Check application health
echo "Application Health:"
echo "==================="
if curl -f -s "http://localhost/api/health" > /dev/null; then
    echo "✅ Application is healthy"
    echo "Application URL: http://localhost"
    echo "Admin URL: http://localhost/admin"
    echo "API Health: http://localhost/api/health"
else
    echo "❌ Application is not responding"
fi
echo

# Check database health
echo "Database Health:"
echo "================"
if curl -f -s "http://localhost/api/health/detailed" | grep -q "Database.*Healthy"; then
    echo "✅ Database is healthy"
else
    echo "❌ Database is not healthy"
fi
echo

# Check Redis health
echo "Redis Health:"
echo "============="
if curl -f -s "http://localhost/api/health/detailed" | grep -q "Redis.*Healthy"; then
    echo "✅ Redis is healthy"
else
    echo "⚠️  Redis is not configured or not healthy"
fi
echo

# System resources
echo "System Resources:"
echo "================="
echo "Memory Usage:"
free -h
echo
echo "Disk Usage:"
df -h
echo
echo "Container Stats:"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"