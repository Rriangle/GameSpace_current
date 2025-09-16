#!/bin/bash

# Quick validate script for GameSpace
# Usage: ./scripts/quick-validate.sh

set -e

echo "✅ GameSpace Quick Validation"
echo "============================="
echo

# Validate project structure
echo "1. Validating project structure..."
echo "=================================="

# Check required files
required_files=(
    "docker-compose.yml"
    "GameSpace/GameSpace.csproj"
    "GameSpace/Program.cs"
    "GameSpace/Startup.cs"
    "GameSpace/Data/GameSpaceDbContext.cs"
    "GameSpace/Controllers/HomeController.cs"
    "GameSpace/Areas/Admin/Controllers/AdminController.cs"
)

for file in "${required_files[@]}"; do
    if [ -f "$file" ]; then
        echo "✅ $file"
    else
        echo "❌ $file (missing)"
        exit 1
    fi
done
echo

# Validate Docker configuration
echo "2. Validating Docker configuration..."
echo "===================================="

# Check docker-compose.yml syntax
if docker-compose config > /dev/null 2>&1; then
    echo "✅ docker-compose.yml syntax is valid"
else
    echo "❌ docker-compose.yml syntax is invalid"
    exit 1
fi

# Check Dockerfile
if [ -f "Dockerfile" ]; then
    echo "✅ Dockerfile found"
else
    echo "❌ Dockerfile not found"
    exit 1
fi
echo

# Validate environment configuration
echo "3. Validating environment configuration..."
echo "=========================================="

# Check if .env files exist
if [ -f ".env.development" ]; then
    echo "✅ .env.development found"
else
    echo "⚠️  .env.development not found (optional)"
fi

if [ -f ".env.production" ]; then
    echo "✅ .env.production found"
else
    echo "⚠️  .env.production not found (optional)"
fi
echo

# Validate scripts
echo "4. Validating scripts..."
echo "======================="

# Check if all scripts are executable
script_files=(
    "scripts/quick-start.sh"
    "scripts/quick-stop.sh"
    "scripts/quick-restart.sh"
    "scripts/quick-status.sh"
    "scripts/quick-logs.sh"
    "scripts/quick-monitor.sh"
    "scripts/quick-backup.sh"
    "scripts/quick-cleanup.sh"
    "scripts/quick-help.sh"
    "scripts/quick-setup.sh"
    "scripts/quick-update.sh"
    "scripts/quick-deploy.sh"
    "scripts/quick-test.sh"
    "scripts/quick-reset.sh"
    "scripts/quick-info.sh"
    "scripts/quick-version.sh"
    "scripts/quick-debug.sh"
    "scripts/quick-validate.sh"
)

for script in "${script_files[@]}"; do
    if [ -f "$script" ] && [ -x "$script" ]; then
        echo "✅ $script"
    else
        echo "❌ $script (missing or not executable)"
        exit 1
    fi
done
echo

# Validate directories
echo "5. Validating directories..."
echo "============================"

# Check if required directories exist
required_dirs=(
    "logs"
    "backups"
    "ssl"
    "nginx"
    "scripts"
    "GameSpace"
    "GameSpace/Data"
    "GameSpace/Controllers"
    "GameSpace/Areas/Admin/Controllers"
    "GameSpace/Services"
    "GameSpace/Middleware"
)

for dir in "${required_dirs[@]}"; do
    if [ -d "$dir" ]; then
        echo "✅ $dir"
    else
        echo "❌ $dir (missing)"
        exit 1
    fi
done
echo

# Validate Git configuration
echo "6. Validating Git configuration..."
echo "=================================="

if [ -d ".git" ]; then
    echo "✅ Git repository initialized"
    echo "Branch: $(git branch --show-current)"
    echo "Commit: $(git rev-parse --short HEAD)"
else
    echo "⚠️  Git repository not initialized"
fi
echo

# Validate Docker images
echo "7. Validating Docker images..."
echo "=============================="

# Check if Docker images can be built
if docker-compose build --no-cache > /dev/null 2>&1; then
    echo "✅ Docker images can be built"
else
    echo "❌ Docker images cannot be built"
    exit 1
fi
echo

# Validate services
echo "8. Validating services..."
echo "========================"

# Check if services can be started
if docker-compose up -d > /dev/null 2>&1; then
    echo "✅ Services can be started"
    
    # Wait for services to be ready
    sleep 30
    
    # Check if application is responding
    if curl -f -s "http://localhost/api/health" > /dev/null; then
        echo "✅ Application is responding"
    else
        echo "❌ Application is not responding"
        exit 1
    fi
    
    # Stop services
    docker-compose down
else
    echo "❌ Services cannot be started"
    exit 1
fi
echo

echo "🎉 All validations passed successfully!"
echo "GameSpace is ready for deployment."