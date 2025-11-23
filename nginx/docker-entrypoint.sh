#!/bin/sh

# Determine which nginx configuration to use based on environment
if [ "$NGINX_ENVIRONMENT" = "production" ]; then
    echo "Starting nginx in PRODUCTION mode (with SSL)"
    cp /etc/nginx/nginx.production.conf /etc/nginx/nginx.conf
else
    echo "Starting nginx in DEVELOPMENT mode (without SSL)"
    cp /etc/nginx/nginx.development.conf /etc/nginx/nginx.conf
fi

# Start nginx
exec nginx -g 'daemon off;'