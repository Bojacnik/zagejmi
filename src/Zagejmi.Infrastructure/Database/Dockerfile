FROM postgres:latest

ENV POSTGRES_DB=mydatabase
ENV POSTGRES_USER=myuser
ENV POSTGRES_PASSWORD=mypassword

# Copy initialization scripts
COPY ./initdb/init.sh /docker-entrypoint-initdb.d/

# Switch to postgres user before exposing port
USER postgres

EXPOSE 5432