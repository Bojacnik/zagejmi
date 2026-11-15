#!/usr/bin/env bash
# Copies the host Caddy's TLS certificate for the mail host name (mx.zagejmi.cz) to /opt/zagejmi/mail-tls, where the
# mail server reads it, and tells the mail server to reload it. Caddy renews the certificate by itself (~every 60 days);
# install-host.sh installs this script as /usr/local/sbin/zagejmi-mail-cert-sync with a systemd timer that runs it
# daily. Runs as root (Caddy's files are readable only by the caddy user). Does nothing when nothing changed.
set -euo pipefail

APP_DIR=${ZAGEJMI_DIR:-/opt/zagejmi}
CADDY_CERTS=${CADDY_CERTS:-/var/lib/caddy/.local/share/caddy/certificates}
MAIL_UID=2000 # the stalwart user in the mail server image

host=$(sed -n 's/^MAIL_HOSTNAME=//p' "$APP_DIR/.env" | tail -n 1)
host=${host:-mx.$(sed -n 's/^DOMAIN=//p' "$APP_DIR/.env" | tail -n 1)}

# Caddy keeps one directory per issuer (Let's Encrypt, ZeroSSL); take the newest certificate.
crt=$(find "$CADDY_CERTS" -path "*/$host/$host.crt" -printf '%T@ %p\n' 2> /dev/null | sort -n | tail -n 1 | cut -d' ' -f2-)
if [ -z "$crt" ]; then
    echo "Caddy has no certificate for $host yet (is it in /etc/caddy/Caddyfile and does DNS point here?)." >&2
    exit 1
fi
key=${crt%.crt}.key

dst=$APP_DIR/mail-tls
install -d -m 750 -o "$MAIL_UID" -g "$MAIL_UID" "$dst"
if cmp -s "$crt" "$dst/fullchain.pem" && cmp -s "$key" "$dst/privkey.pem"; then
    exit 0
fi
install -m 644 -o "$MAIL_UID" -g "$MAIL_UID" "$crt" "$dst/fullchain.pem"
install -m 600 -o "$MAIL_UID" -g "$MAIL_UID" "$key" "$dst/privkey.pem"
echo "Certificate for $host updated ($(openssl x509 -in "$dst/fullchain.pem" -noout -enddate))."

# Reload it in the running mail server (no restart, connections stay open). Not running yet = it loads it on start.
cd "$APP_DIR"
container=$(docker compose ps -q mail 2> /dev/null || true)
admin=$(sed -n 's/^MAIL_ADMIN=//p' .env | tail -n 1)
if [ -n "$container" ] && [ -n "$admin" ]; then
    STALWART_PASSWORD="${admin#*:}" docker run --rm --network "container:$container" \
        -e STALWART_URL=http://127.0.0.1:8080 -e STALWART_USER="${admin%%:*}" -e STALWART_PASSWORD \
        stalwartlabs/cli:1.0.14 --no-color create Action/ReloadTlsCertificates > /dev/null
    echo "Mail server reloaded the certificate."
fi
