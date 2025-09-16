#!/bin/bash

# Monitoring script for GameSpace
# Usage: ./scripts/monitor.sh [environment]

set -e

ENVIRONMENT=${1:-production}

echo "Starting monitoring for $ENVIRONMENT environment..."

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

# Function to get container stats
get_container_stats() {
    echo "Container Statistics:"
    echo "===================="
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"
    echo
}

# Function to get disk usage
get_disk_usage() {
    echo "Disk Usage:"
    echo "==========="
    df -h
    echo
}

# Function to get memory usage
get_memory_usage() {
    echo "Memory Usage:"
    echo "============="
    free -h
    echo
}

# Main monitoring loop
while true; do
    clear
    echo "GameSpace Monitoring Dashboard - $(date)"
    echo "========================================"
    echo
    
    # Check services
    echo "Service Health:"
    echo "==============="
    check_service "Application" "http://localhost/api/health"
    check_service "Database" "http://localhost/api/health/detailed"
    echo
    
    # Get system information
    get_container_stats
    get_disk_usage
    get_memory_usage
    
    # Wait before next check
    echo "Next check in 30 seconds... (Press Ctrl+C to exit)"
    sleep 30
done