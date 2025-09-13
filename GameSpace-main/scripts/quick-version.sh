#!/bin/bash

# Quick version script for GameSpace
# Usage: ./scripts/quick-version.sh

echo "📋 GameSpace Version Information"
echo "================================"
echo

# Application version
echo "Application Version:"
echo "==================="
if [ -f "GameSpace/GameSpace.csproj" ]; then
    echo "Project File: GameSpace/GameSpace.csproj"
    echo "Target Framework: $(grep -o '<TargetFramework>.*</TargetFramework>' GameSpace/GameSpace.csproj | sed 's/<TargetFramework>//g' | sed 's/<\/TargetFramework>//g')"
else
    echo "Project file not found"
fi
echo

# Docker version
echo "Docker Version:"
echo "==============="
echo "Docker: $(docker --version 2>/dev/null || echo 'Not installed')"
echo "Docker Compose: $(docker-compose --version 2>/dev/null || echo 'Not installed')"
echo

# Git version
echo "Git Version:"
echo "============"
echo "Git: $(git --version 2>/dev/null || echo 'Not installed')"
echo "Branch: $(git branch --show-current 2>/dev/null || echo 'Not a git repository')"
echo "Commit: $(git rev-parse --short HEAD 2>/dev/null || echo 'Not a git repository')"
echo "Last Commit: $(git log -1 --format=%ci 2>/dev/null || echo 'Not a git repository')"
echo

# Service version
echo "Service Version:"
echo "================"
if docker-compose ps | grep -q "Up"; then
    echo "Status: Running"
    echo "Containers:"
    docker-compose ps
else
    echo "Status: Stopped"
fi
echo

# Environment information
echo "Environment Information:"
echo "======================="
echo "Environment: ${ENVIRONMENT:-development}"
echo "Working Directory: $(pwd)"
echo "User: $(whoami)"
echo "Hostname: $(hostname)"
echo