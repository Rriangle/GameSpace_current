#!/bin/bash

# SSL certificate setup script for GameSpace
# Usage: ./scripts/ssl-setup.sh [domain] [email]

set -e

DOMAIN=${1}
EMAIL=${2}

if [ -z "$DOMAIN" ] || [ -z "$EMAIL" ]; then
    echo "Usage: $0 <domain> <email>"
    echo "Example: $0 gamespace.example.com admin@example.com"
    exit 1
fi

echo "Setting up SSL certificate for $DOMAIN..."

# Create SSL directory
mkdir -p ssl

# Install certbot if not already installed
if ! command -v certbot &> /dev/null; then
    echo "Installing certbot..."
    sudo apt-get update
    sudo apt-get install -y certbot
fi

# Generate SSL certificate
echo "Generating SSL certificate..."
sudo certbot certonly --standalone -d $DOMAIN --email $EMAIL --agree-tos --non-interactive

# Copy certificates to ssl directory
echo "Copying certificates..."
sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem ssl/
sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem ssl/
sudo chown $USER:$USER ssl/*.pem

echo "SSL certificate setup completed!"
echo "Certificates are available in ./ssl/ directory"
echo "Remember to update your nginx configuration to use these certificates"