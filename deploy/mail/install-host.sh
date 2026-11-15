#!/usr/bin/env bash
# Prepares the server itself for our own mail server. Run once as root, from /opt/zagejmi:
#   bash mail/install-host.sh
# Step 3 in deploy/MAIL.md; needs the DNS A record for the mail host name (mx.zagejmi.cz) to point here already.
#
#   1. Caddy: adds a block for the mail host name (+ mta-sts/autoconfig/autodiscover) that proxies to the mail server's
#      web part on 127.0.0.1:8090 and, as a side effect, gets the TLS certificate the mail server uses. Only adds this
#      block (after a backup and `caddy validate`); other sites in the Caddyfile are not touched.
#   2. Installs sync-cert.sh as /usr/local/sbin/zagejmi-mail-cert-sync with a daily systemd timer, and runs it.
#   3. Checks what Hetzner has to do (reverse DNS, outgoing port 25) and says what is missing.
set -euo pipefail

[ "$(id -u)" = 0 ] || { echo "Run as root." >&2; exit 1; }
cd "$(dirname "$0")/.."
[ -f .env ] || { echo "Run from /opt/zagejmi (no .env here)." >&2; exit 1; }

DOMAIN=$(sed -n 's/^DOMAIN=//p' .env | tail -n 1)
MAIL_HOSTNAME=$(sed -n 's/^MAIL_HOSTNAME=//p' .env | tail -n 1)
MAIL_HOSTNAME=${MAIL_HOSTNAME:-mx.$DOMAIN}
CADDYFILE=/etc/caddy/Caddyfile

# 1. Caddy
if grep -q "^$MAIL_HOSTNAME[ ,]" "$CADDYFILE"; then
    echo "Caddy: $MAIL_HOSTNAME is already in $CADDYFILE."
else
    cp -a "$CADDYFILE" "$CADDYFILE.bak-$(date +%Y%m%d%H%M%S)"
    cat >> "$CADDYFILE" <<EOF

# Zagejmi mail server (Stalwart, /opt/zagejmi): web admin, MTA-STS policy, mail client autoconfig. Its TLS
# certificate is also copied to the mail server by zagejmi-mail-cert-sync. Added by zagejmi deploy/mail/install-host.sh.
$MAIL_HOSTNAME, mta-sts.$DOMAIN, autoconfig.$DOMAIN, autodiscover.$DOMAIN {
	reverse_proxy 127.0.0.1:8090
}
EOF
    if ! caddy validate --config "$CADDYFILE" --adapter caddyfile > /dev/null 2>&1; then
        cp -a "$(ls -t "$CADDYFILE".bak-* | head -n 1)" "$CADDYFILE"
        echo "Caddy rejected the new block; $CADDYFILE restored unchanged." >&2
        exit 1
    fi
    systemctl reload caddy
    echo "Caddy: added $MAIL_HOSTNAME and reloaded."
fi

# 2. Certificate sync
install -m 755 mail/sync-cert.sh /usr/local/sbin/zagejmi-mail-cert-sync
cat > /etc/systemd/system/zagejmi-mail-cert-sync.service <<'EOF'
[Unit]
Description=Copy Caddy's certificate for the Zagejmi mail server and reload it
After=caddy.service docker.service

[Service]
Type=oneshot
ExecStart=/usr/local/sbin/zagejmi-mail-cert-sync
EOF
cat > /etc/systemd/system/zagejmi-mail-cert-sync.timer <<'EOF'
[Unit]
Description=Daily copy of Caddy's certificate for the Zagejmi mail server

[Timer]
OnBootSec=5min
OnCalendar=daily
RandomizedDelaySec=1h
Persistent=true

[Install]
WantedBy=timers.target
EOF
systemctl daemon-reload
systemctl enable --now zagejmi-mail-cert-sync.timer > /dev/null

echo "Waiting for Caddy to get the certificate for $MAIL_HOSTNAME ..."
for _ in $(seq 1 30); do
    if /usr/local/sbin/zagejmi-mail-cert-sync 2> /dev/null; then break; fi
    sleep 5
done
if [ -s mail-tls/fullchain.pem ]; then
    echo "Certificate ready in /opt/zagejmi/mail-tls."
else
    echo "No certificate yet. Check that $MAIL_HOSTNAME points to this server and see: journalctl -u caddy -n 50" >&2
fi

# 3. What Hetzner has to do
ip=$(ip -4 route get 1.1.1.1 | awk '{ for (i = 1; i < NF; i++) if ($i == "src") print $(i + 1) }')
ptr=$(getent hosts "$ip" 2> /dev/null | awk '{ print $2 }' || true)
if [ "$ptr" = "$MAIL_HOSTNAME" ]; then
    echo "Reverse DNS: $ip -> $ptr (OK)."
else
    echo "Reverse DNS: $ip -> ${ptr:-nothing}; set it to $MAIL_HOSTNAME in the Hetzner console (server > Networking)."
fi
if timeout 8 bash -c 'exec 3<>/dev/tcp/gmail-smtp-in.l.google.com/25' 2> /dev/null; then
    echo "Outgoing port 25: open (OK)."
else
    echo "Outgoing port 25: blocked; ask Hetzner to unblock it, until then mail to other servers waits in the queue."
fi
