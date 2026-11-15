#!/usr/bin/env bash
# Switches where the app sends its mail. Run on the server as the deploy user, from /opt/zagejmi:
#   bash mail/switch-smtp.sh own     # our own mail server (after setup-mail.sh), as noreply@<DOMAIN>
#   bash mail/switch-smtp.sh relay   # back to the SMTP relay (Brevo) that was set before the first switch
#   bash mail/switch-smtp.sh status  # which one is in use (no passwords shown)
# Only the SMTP_* lines in .env change, then the app container restarts (a few seconds). Nothing else is touched, so
# switching back and forth is safe. The relay settings are kept in .env.smtp-relay.
set -euo pipefail
umask 077 # .env and its copies hold secrets

cd "$(dirname "$0")/.."
[ -f .env ] || { echo "Run from /opt/zagejmi (no .env here)." >&2; exit 1; }

KEYS="SMTP_HOST SMTP_PORT SMTP_SECURITY SMTP_ENABLE_SSL SMTP_USE_AUTHENTICATION SMTP_USERNAME SMTP_PASSWORD SMTP_FROM_EMAIL"

env_get() { sed -n "s/^$1=//p" "${2:-.env}" | tail -n 1; }

# Sets KEY=value in .env (value from $V), keeping the file's owner and permissions.
env_set() {
    V="$V" awk -v k="$1" 'BEGIN { v = ENVIRON["V"]; done = 0 }
        $0 ~ "^" k "=" { print k "=" v; done = 1; next } { print }
        END { if (!done) print k "=" v }' .env > .env.tmp
    cat .env.tmp > .env
    rm -f .env.tmp
}

DOMAIN=$(env_get DOMAIN)
MAIL_HOSTNAME=$(env_get MAIL_HOSTNAME)

status() {
    echo "SMTP: $(env_get SMTP_HOST):$(env_get SMTP_PORT) (security $(v=$(env_get SMTP_SECURITY); echo "${v:-Auto}")), from $(env_get SMTP_FROM_EMAIL)"
    if [ -n "$MAIL_HOSTNAME" ] && [ "$(env_get SMTP_HOST)" = "$MAIL_HOSTNAME" ]; then
        echo "= our own mail server"
    else
        echo "= relay"
    fi
}

case "${1:-}" in
    own)
        [ -n "$MAIL_HOSTNAME" ] && [ -n "$(env_get MAIL_NOREPLY_PASSWORD)" ] \
            || { echo "The mail server is not set up yet: run  bash mail/setup-mail.sh  first." >&2; exit 1; }
        if [ "$(env_get SMTP_HOST)" != "$MAIL_HOSTNAME" ]; then
            # Remember the relay, so "relay" can bring it back.
            for k in $KEYS; do printf '%s=%s\n' "$k" "$(env_get "$k")"; done > .env.smtp-relay
        fi
        V="$MAIL_HOSTNAME" env_set SMTP_HOST
        V=465 env_set SMTP_PORT
        V=SslOnConnect env_set SMTP_SECURITY
        V=true env_set SMTP_ENABLE_SSL
        V=true env_set SMTP_USE_AUTHENTICATION
        V="noreply@$DOMAIN" env_set SMTP_USERNAME
        V="$(env_get MAIL_NOREPLY_PASSWORD)" env_set SMTP_PASSWORD
        V="noreply@$DOMAIN" env_set SMTP_FROM_EMAIL
        ;;
    relay)
        [ -f .env.smtp-relay ] || { echo "No saved relay settings (.env.smtp-relay); already on the relay?" >&2; exit 1; }
        for k in $KEYS; do V="$(env_get "$k" .env.smtp-relay)" env_set "$k"; done
        ;;
    status)
        status
        exit 0
        ;;
    *)
        echo "Usage: bash mail/switch-smtp.sh own|relay|status" >&2
        exit 2
        ;;
esac

# Recreates only the app container with the new settings, on the image that runs now (not whatever IMAGE_TAG defaults to).
running=$(docker inspect -f '{{.Config.Image}}' "$(docker compose ps -q api)")
IMAGE_TAG="${running##*:}" docker compose up -d --no-deps api
for _ in $(seq 1 30); do
    curl -fsS --max-time 5 http://127.0.0.1:3000/healthz 2> /dev/null | grep -q Healthy && break
    sleep 2
done
status
