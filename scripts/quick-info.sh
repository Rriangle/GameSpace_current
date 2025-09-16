#!/bin/bash

# Quick info script for GameSpace
# Usage: ./scripts/quick-info.sh

echo "ℹ️  GameSpace Quick Info"
echo "======================="
echo

# System information
echo "System Information:"
echo "==================="
echo "OS: $(uname -s)"
echo "Architecture: $(uname -m)"
echo "Docker Version: $(docker --version 2>/dev/null || echo 'Not installed')"
echo "Docker Compose Version: $(docker-compose --version 2>/dev/null || echo 'Not installed')"
echo

# Project information
echo "Project Information:"
echo "==================="
echo "Project Name: GameSpace"
echo "Project Directory: $(pwd)"
echo "Git Branch: $(git branch --show-current 2>/dev/null || echo 'Not a git repository')"
echo "Git Commit: $(git rev-parse --short HEAD 2>/dev/null || echo 'Not a git repository')"
echo

# Service information
echo "Service Information:"
echo "==================="
if docker-compose ps | grep -q "Up"; then
    echo "Status: Running"
    echo "Services:"
    docker-compose ps
else
    echo "Status: Stopped"
fi
echo

# Port information
echo "Port Information:"
echo "================="
echo "Application: http://localhost"
echo "Admin: http://localhost/admin"
echo "API Health: http://localhost/api/health"
echo

# Directory information
echo "Directory Information:"
echo "====================="
echo "Logs: ./logs/"
echo "Backups: ./backups/"
echo "SSL: ./ssl/"
echo "Scripts: ./scripts/"
echo

# Quick commands
echo "Quick Commands:"
echo "==============="
echo "  Start:     ./scripts/quick-start.sh"
echo "  Stop:      ./scripts/quick-stop.sh"
echo "  Restart:   ./scripts/quick-restart.sh"
echo "  Status:    ./scripts/quick-status.sh"
echo "  Logs:      ./scripts/quick-logs.sh"
echo "  Monitor:   ./scripts/quick-monitor.sh"
echo "  Test:      ./scripts/quick-test.sh"
echo "  Help:      ./scripts/quick-help.sh"