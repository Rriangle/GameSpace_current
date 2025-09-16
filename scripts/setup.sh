#!/bin/bash

# Setup script for GameSpace
# Usage: ./scripts/setup.sh [environment]

set -e

ENVIRONMENT=${1:-development}

echo "Setting up GameSpace for $ENVIRONMENT environment..."
echo "=================================================="

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create necessary directories
echo "1. Creating necessary directories..."
mkdir -p logs
mkdir -p backups
mkdir -p ssl
mkdir -p nginx

# Set up environment file
echo "2. Setting up environment file..."
if [ ! -f ".env.$ENVIRONMENT" ]; then
    cat > .env.$ENVIRONMENT << EOF
# GameSpace Environment Configuration
ENVIRONMENT=$ENVIRONMENT

# Database Configuration
DB_PASSWORD=YourStrong@Password
DB_SA_PASSWORD=YourStrong@Password

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256
JWT_ISSUER=GameSpaceIssuer
JWT_AUDIENCE=GameSpaceAudience

# Redis Configuration
REDIS_PASSWORD=

# SSL Configuration (for production)
SSL_CERT_PATH=./ssl/fullchain.pem
SSL_KEY_PATH=./ssl/privkey.pem
DOMAIN=localhost

# Backup Configuration
BACKUP_RETENTION_DAYS=7
BACKUP_SCHEDULE="0 2 * * *"

# Monitoring Configuration
MONITORING_ENABLED=true
LOG_LEVEL=Information
EOF
    echo "✅ Environment file created: .env.$ENVIRONMENT"
else
    echo "✅ Environment file already exists: .env.$ENVIRONMENT"
fi

# Load environment variables
export $(cat .env.$ENVIRONMENT | grep -v '^#' | xargs)

# Build Docker images
echo "3. Building Docker images..."
docker-compose -f docker-compose.$ENVIRONMENT.yml build

# Start services
echo "4. Starting services..."
docker-compose -f docker-compose.$ENVIRONMENT.yml up -d

# Wait for services to be ready
echo "5. Waiting for services to be ready..."
sleep 30

# Run database migrations
echo "6. Running database migrations..."
docker-compose -f docker-compose.$ENVIRONMENT.yml exec gamespace-app dotnet ef database update

# Run health check
echo "7. Running health check..."
./scripts/health-check.sh $ENVIRONMENT

echo
echo "🎉 GameSpace setup completed successfully!"
echo "=========================================="
echo "Environment: $ENVIRONMENT"
echo "Application URL: http://localhost"
echo "Admin URL: http://localhost/admin"
echo "API Health: http://localhost/api/health"
echo
echo "Useful commands:"
echo "  Start: ./scripts/start.sh $ENVIRONMENT"
echo "  Stop: ./scripts/stop.sh $ENVIRONMENT"
echo "  Restart: ./scripts/restart.sh $ENVIRONMENT"
echo "  Status: ./scripts/status.sh $ENVIRONMENT"
echo "  Logs: ./scripts/logs.sh app"
echo "  Monitor: ./scripts/monitor.sh $ENVIRONMENT"