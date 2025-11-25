# Stage 1: Build the Angular application
FROM node:20-alpine AS builder

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application for production
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx
FROM nginx:alpine

# Install gettext for envsubst
RUN apk add --no-cache gettext

# Copy built application from builder stage
COPY --from=builder /app/dist/frontend /usr/share/nginx/html

# Copy config template
COPY --from=builder /app/src/assets/config.template.json /usr/share/nginx/html/browser/assets/config.template.json

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Configure Nginx
RUN echo 'server { listen 80; server_name localhost; location / { root /usr/share/nginx/html/browser/; index index.html; try_files $uri /index.html; } }' > /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Use custom entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]
