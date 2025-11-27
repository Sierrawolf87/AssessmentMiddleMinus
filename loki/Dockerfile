FROM grafana/loki:latest

# Copy custom configuration
COPY loki-config.yml /etc/loki/loki-config.yml

# Use custom config file
CMD ["-config.file=/etc/loki/loki-config.yml"]

