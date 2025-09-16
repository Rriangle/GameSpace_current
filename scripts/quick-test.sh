#!/bin/bash

# Quick test script for GameSpace
# Usage: ./scripts/quick-test.sh

set -e

echo "🧪 GameSpace Quick Test"
echo "======================="
echo

# Test application health
echo "1. Testing application health..."
if curl -f -s "http://localhost/api/health" > /dev/null; then
    echo "✅ Application health check passed"
else
    echo "❌ Application health check failed"
    exit 1
fi

# Test database connectivity
echo "2. Testing database connectivity..."
if curl -f -s "http://localhost/api/health/detailed" | grep -q "Database.*Healthy"; then
    echo "✅ Database connectivity test passed"
else
    echo "❌ Database connectivity test failed"
    exit 1
fi

# Test Redis connectivity (if configured)
echo "3. Testing Redis connectivity..."
if curl -f -s "http://localhost/api/health/detailed" | grep -q "Redis.*Healthy"; then
    echo "✅ Redis connectivity test passed"
else
    echo "⚠️  Redis is not configured or not healthy"
fi

# Test main pages
echo "4. Testing main pages..."
if curl -f -s "http://localhost" > /dev/null; then
    echo "✅ Main page test passed"
else
    echo "❌ Main page test failed"
    exit 1
fi

# Test admin page
echo "5. Testing admin page..."
if curl -f -s "http://localhost/admin" > /dev/null; then
    echo "✅ Admin page test passed"
else
    echo "❌ Admin page test failed"
    exit 1
fi

# Test API endpoints
echo "6. Testing API endpoints..."
if curl -f -s "http://localhost/api/health/detailed" > /dev/null; then
    echo "✅ API endpoints test passed"
else
    echo "❌ API endpoints test failed"
    exit 1
fi

echo
echo "🎉 All tests passed successfully!"
echo "GameSpace is working correctly."