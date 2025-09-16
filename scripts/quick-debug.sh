#!/bin/bash

# Quick debug script for GameSpace
# Usage: ./scripts/quick-debug.sh

set -e

echo "🐛 GameSpace Quick Debug"
echo "========================"
echo

# Check Docker status
echo "1. Docker Status:"
echo "================="
if docker info > /dev/null 2>&1; then
    echo "✅ Docker is running"
    echo "Docker Version: $(docker --version)"
else
    echo "❌ Docker is not running or not installed"
    exit 1
fi
echo

# Check Docker Compose status
echo "2. Docker Compose Status:"
echo "========================="
if docker-compose --version > /dev/null 2>&1; then
    echo "✅ Docker Compose is available"
    echo "Docker Compose Version: $(docker-compose --version)"
else
    echo "❌ Docker Compose is not installed"
    exit 1
fi
echo

# Check project files
echo "3. Project Files:"
echo "================="
if [ -f "docker-compose.yml" ]; then
    echo "✅ docker-compose.yml found"
else
    echo "❌ docker-compose.yml not found"
    exit 1
fi

if [ -f "GameSpace/GameSpace.csproj" ]; then
    echo "✅ GameSpace.csproj found"
else
    echo "❌ GameSpace.csproj not found"
    exit 1
fi
echo

# Check service status
echo "4. Service Status:"
echo "=================="
if docker-compose ps | grep -q "Up"; then
    echo "✅ Services are running"
    echo "Services:"
    docker-compose ps
else
    echo "❌ Services are not running"
    echo "To start services, run: ./scripts/quick-start.sh"
fi
echo

# Check application health
echo "5. Application Health:"
echo "======================"
if curl -f -s "http://localhost/api/health" > /dev/null; then
    echo "✅ Application is healthy"
else
    echo "❌ Application is not responding"
    echo "Please check the logs: ./scripts/quick-logs.sh"
fi
echo

# Check logs for errors
echo "6. Recent Errors:"
echo "================="
echo "Checking for errors in the last 50 lines of logs..."
if docker-compose logs --tail=50 | grep -i error; then
    echo "⚠️  Errors found in logs"
else
    echo "✅ No errors found in recent logs"
fi
echo

# Check system resources
echo "7. System Resources:"
echo "===================="
echo "Memory Usage:"
free -h
echo
echo "Disk Usage:"
df -h
echo
echo "Container Stats:"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"
echo

# Check network connectivity
echo "8. Network Connectivity:"
echo "======================="
if ping -c 1 localhost > /dev/null 2>&1; then
    echo "✅ Localhost is reachable"
else
    echo "❌ Localhost is not reachable"
fi

if curl -f -s "http://localhost" > /dev/null; then
    echo "✅ HTTP service is responding"
else
    echo "❌ HTTP service is not responding"
fi
echo

echo "Debug completed!"
echo "If you're still experiencing issues, please check the logs: ./scripts/quick-logs.sh"