FROM nginx:alpine

COPY nginx.production.conf /etc/nginx/nginx.production.conf
COPY nginx.development.conf /etc/nginx/nginx.development.conf

COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/bin/sh", "/docker-entrypoint.sh"]