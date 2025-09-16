#!/bin/bash

# Quick monitor script for GameSpace
# Usage: ./scripts/quick-monitor.sh

set -e

echo "📊 GameSpace Quick Monitor"
echo "=========================="
echo

# Function to check service status
check_service() {
    local service=$1
    local url=$2
    
    if curl -f -s "$url" > /dev/null; then
        echo "✅ $service is healthy"
        return 0
    else
        echo "❌ $service is unhealthy"
        return 1
    fi
}

# Check services
echo "Service Health:"
echo "==============="
check_service "Application" "http://localhost/api/health"
check_service "Database" "http://localhost/api/health/detailed"
echo

# Get container stats
echo "Container Statistics:"
echo "===================="
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"
echo

# Get system resources
echo "System Resources:"
echo "================="
echo "Memory Usage:"
free -h
echo
echo "Disk Usage:"
df -h
echo

echo "For continuous monitoring, run: ./scripts/monitor.sh"