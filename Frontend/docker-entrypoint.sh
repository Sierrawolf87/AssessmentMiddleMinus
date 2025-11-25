#!/bin/sh
set -e

# Replace environment variables in config.json
envsubst < /usr/share/nginx/html/browser/assets/config.template.json > /usr/share/nginx/html/browser/assets/config.json

echo "Configuration file generated:"
cat /usr/share/nginx/html/browser/assets/config.json

# Start Nginx
exec nginx -g "daemon off;"
