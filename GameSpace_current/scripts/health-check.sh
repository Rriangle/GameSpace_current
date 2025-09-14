#!/bin/bash

# Health check script for GameSpace
# Usage: ./scripts/health-check.sh [environment]

set -e

ENVIRONMENT=${1:-production}
BASE_URL="http://localhost"

echo "Performing health check for $ENVIRONMENT environment..."

# Check if application is running
echo "1. Checking application status..."
if curl -f -s "$BASE_URL/api/health" > /dev/null; then
    echo "✅ Application is running"
else
    echo "❌ Application is not responding"
    exit 1
fi

# Check database connectivity
echo "2. Checking database connectivity..."
if curl -f -s "$BASE_URL/api/health/detailed" | grep -q "Database.*Healthy"; then
    echo "✅ Database is healthy"
else
    echo "❌ Database is not healthy"
    exit 1
fi

# Check Redis connectivity (if configured)
echo "3. Checking Redis connectivity..."
if curl -f -s "$BASE_URL/api/health/detailed" | grep -q "Redis.*Healthy"; then
    echo "✅ Redis is healthy"
else
    echo "⚠️  Redis is not configured or not healthy"
fi

# Check container status
echo "4. Checking container status..."
if docker ps | grep -q "gamespace-main"; then
    echo "✅ GameSpace containers are running"
else
    echo "❌ GameSpace containers are not running"
    exit 1
fi

echo "🎉 All health checks passed!"