#!/bin/bash
set -e

# Create the initial config files
mkdir -p /var/lib/postgresql/data
chmod 700 /var/lib/postgresql/data

cat > /var/lib/postgresql/data/postgresql.conf << EOF
listen_addresses = '*'
port = 5432
max_connections = 100
shared_buffers = 128MB
logging_collector = on
log_directory = 'pg_log'
EOF

chown -R postgres:postgres /var/lib/postgresql/data
chmod 700 /var/lib/postgresql/data