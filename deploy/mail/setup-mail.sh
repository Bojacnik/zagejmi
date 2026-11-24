#!/usr/bin/env bash
# Turns on and configures our own mail server (Stalwart, the "mail" service in compose.yaml). Run on the server as the
# deploy user, from /opt/zagejmi:  bash mail/setup-mail.sh
# Safe to run again: it only adds what is missing. Step by step in deploy/MAIL.md (this is step 4 there).
#
# What it does:
#   1. .env: adds COMPOSE_PROFILES=mail (so deploys keep the mail server running), MAIL_HOSTNAME and the generated
#      passwords (MAIL_ADMIN, MAIL_NOREPLY_PASSWORD, MAIL_POSTMASTER_PASSWORD, and MAIL_NOREPLY_PASSWORD_<NAME> for
#      each extra domain) if they are not there yet;
#   2. starts the mail server and, on its first start, runs Stalwart's setup (host name, domain, DKIM keys, log to
#      the console instead of files);
#   3. adds the TLS certificate from ./mail-tls (skipped with a warning until install-host.sh has put it there), the
#      587 listener, trusts X-Forwarded-For from Caddy, and creates the mailboxes noreply@ (the app sends from it) and
#      postmaster@ (DMARC/TLS reports, bounces);
#   4. extra domains from MAIL_EXTRA_DOMAINS in .env (space separated, e.g. "kaimo.zagejmi.cz vutregbot.zagejmi.cz"
#      for the other projects on this server): creates each domain with its DKIM keys, a mailbox noreply@<domain> and
#      postmaster@<domain> as an alias of postmaster@$DOMAIN;
#   5. turns off automatic DKIM key rotation (the DNS records are kept by hand, so a rotated key would be unpublished
#      and mail would fail DKIM) and prints the DNS records to add at the DNS provider.
# It never prints passwords; they stay in .env (readable only by the deploy user).
set -euo pipefail
umask 077 # .env and its temporary copy hold secrets

cd "$(dirname "$0")/.."
[ -f .env ] || { echo "Run from /opt/zagejmi (no .env here)." >&2; exit 1; }

CLI_IMAGE=stalwartlabs/cli:1.0.14

env_get() { sed -n "s/^$1=//p" .env | tail -n 1; }

# Sets KEY=value in .env (value from $V), keeping the file's owner and permissions.
env_set() {
    V="$V" awk -v k="$1" 'BEGIN { v = ENVIRON["V"]; done = 0 }
        $0 ~ "^" k "=" { print k "=" v; done = 1; next } { print }
        END { if (!done) print k "=" v }' .env > .env.tmp
    cat .env.tmp > .env
    rm -f .env.tmp
}

DOMAIN=$(env_get DOMAIN)
[ -n "$DOMAIN" ] || { echo "DOMAIN is not set in .env." >&2; exit 1; }

# 1. .env
[ -n "$(env_get MAIL_HOSTNAME)" ] || V="mx.$DOMAIN" env_set MAIL_HOSTNAME
MAIL_HOSTNAME=$(env_get MAIL_HOSTNAME)
[ -n "$(env_get MAIL_ADMIN)" ] || V="admin:$(openssl rand -hex 24)" env_set MAIL_ADMIN
[ -n "$(env_get MAIL_NOREPLY_PASSWORD)" ] || V="$(openssl rand -hex 24)" env_set MAIL_NOREPLY_PASSWORD
[ -n "$(env_get MAIL_POSTMASTER_PASSWORD)" ] || V="$(openssl rand -hex 24)" env_set MAIL_POSTMASTER_PASSWORD
EXTRA_DOMAINS=$(env_get MAIL_EXTRA_DOMAINS)
# .env key with the password of noreply@<extra domain>: kaimo.zagejmi.cz -> MAIL_NOREPLY_PASSWORD_KAIMO
noreply_key() { echo "MAIL_NOREPLY_PASSWORD_$(echo "${1%%.*}" | tr 'a-z-' 'A-Z_')"; }
for d in $EXTRA_DOMAINS; do
    [ -n "$(env_get "$(noreply_key "$d")")" ] || V="$(openssl rand -hex 24)" env_set "$(noreply_key "$d")"
done
case ",$(env_get COMPOSE_PROFILES)," in
    *,mail,*) ;;
    ,,) V=mail env_set COMPOSE_PROFILES ;;
    *) V="$(env_get COMPOSE_PROFILES),mail" env_set COMPOSE_PROFILES ;;
esac

have_cert=true
if [ ! -s mail-tls/fullchain.pem ] || [ ! -s mail-tls/privkey.pem ]; then
    have_cert=false
    echo "WARNING: no TLS certificate in /opt/zagejmi/mail-tls yet; the mail server runs with a self-signed one." >&2
    echo "Run deploy/mail/install-host.sh as root (MAIL.md step 3) and then this script again." >&2
fi

# stalwart-cli in a container sharing the mail server's network, signed in as the fixed administrator. The password
# goes in through the environment, not the command line.
cli() {
    STALWART_PASSWORD="${MAIL_ADMIN#*:}" docker run --rm -i --network "container:$(docker compose ps -q mail)" \
        -e STALWART_URL=http://127.0.0.1:8080 -e STALWART_USER="${MAIL_ADMIN%%:*}" -e STALWART_PASSWORD \
        "$CLI_IMAGE" --no-color "$@"
}

wait_for_mail() {
    for _ in $(seq 1 60); do
        if docker compose exec -T mail curl -fsS -o /dev/null http://127.0.0.1:8080/healthz/live 2>/dev/null; then
            return 0
        fi
        sleep 2
    done
    echo "The mail server did not come up; see: docker compose logs mail" >&2
    exit 1
}

MAIL_ADMIN=$(env_get MAIL_ADMIN)

# 2. Start; first-time setup.
docker compose up -d mail
wait_for_mail
if ! docker compose exec -T mail test -f /etc/stalwart/config.json; then
    echo "First start: running Stalwart's setup for $MAIL_HOSTNAME / $DOMAIN"
    cli update Bootstrap --json "{\"defaultHostname\":\"$MAIL_HOSTNAME\",\"defaultDomain\":\"$DOMAIN\",
        \"requestTlsCertificate\":false,\"generateDkimKeys\":true,
        \"tracer\":{\"@type\":\"Stdout\",\"ansi\":false,\"multiline\":false,\"enable\":true,\"level\":\"info\",
                    \"lossy\":false,\"events\":{},\"eventsPolicy\":\"exclude\"}}" > /dev/null
    docker compose restart mail
    wait_for_mail
fi

# 3. Certificate, listener, proxy, mailboxes. Listeners and HTTP settings need a restart, so remember whether anything
# changed.
changed=false
if [ "$have_cert" = true ] && ! cli query Certificate | grep -q "$MAIL_HOSTNAME"; then
    cert_id=$(cli create Certificate --json '{"certificate":{"@type":"File","filePath":"/etc/stalwart/tls/fullchain.pem"},
        "privateKey":{"@type":"File","filePath":"/etc/stalwart/tls/privkey.pem"}}' | awk '/Created Certificate/ { print $3 }')
    cli update SystemSettings --json "{\"defaultCertificateId\":\"$cert_id\"}" > /dev/null
    echo "Certificate for $MAIL_HOSTNAME added."
    changed=true
fi
if ! cli query NetworkListener | grep -q ':587'; then
    cli create NetworkListener --json '{"name":"submission","bind":{"[::]:587":true},"protocol":"smtp",
        "useTls":true,"implicitTls":false}' > /dev/null
    echo "Listener 587 (STARTTLS) added."
    changed=true
fi
# The web part is only reachable through Caddy (127.0.0.1:8090), so the client IP from Caddy can be trusted; without
# this every web client would look like the Docker gateway and one ban would lock everybody out.
if ! cli get Http --json | grep -q '"clientIpSource":"xForwardedFor"'; then
    cli update Http --json '{"clientIpSource":"xForwardedFor","forwardedHops":1}' > /dev/null
    changed=true
fi

domain_id_of() { cli query Domain | awk -v d="$1" '$2 == d { print $1 }'; }
domain_id=$(domain_id_of "$DOMAIN")
create_mailbox() { # name, domain, password, description
    if ! cli query Account | grep -q " $1@$2 "; then
        cli create Account/User --json "{\"name\":\"$1\",\"domainId\":\"$(domain_id_of "$2")\",\"description\":\"$4\",
            \"credentials\":{\"0\":{\"@type\":\"Password\",\"secret\":\"$3\"}}}" > /dev/null
        echo "Mailbox $1@$2 created."
        changed=true
    fi
}
create_mailbox noreply "$DOMAIN" "$(env_get MAIL_NOREPLY_PASSWORD)" "Zagejmi app (outgoing mail)"
create_mailbox postmaster "$DOMAIN" "$(env_get MAIL_POSTMASTER_PASSWORD)" "Postmaster (bounces, DMARC and TLS reports)"

# 4. Extra domains (the other projects on this server).
postmaster_id=$(cli query Account | awk -v a="postmaster@$DOMAIN" '$2 == a { print $1 }')
for d in $EXTRA_DOMAINS; do
    if [ -z "$(domain_id_of "$d")" ]; then
        cli create Domain --json "{\"name\":\"$d\"}" > /dev/null # Stalwart generates its DKIM keys
        echo "Domain $d created."
        changed=true
    fi
    create_mailbox noreply "$d" "$(env_get "$(noreply_key "$d")")" "${d%%.*} app (outgoing mail)"
    # postmaster@<d> (RFC 5321, DMARC/TLS reports of <d>) is an alias of postmaster@$DOMAIN.
    d_id=$(domain_id_of "$d")
    aliases=$(cli get Account "$postmaster_id" --json | D="$d_id" python3 -c '
import json, os, sys
aliases = json.load(sys.stdin).get("aliases") or {}
if not any(a["name"] == "postmaster" and a["domainId"] == os.environ["D"] for a in aliases.values()):
    aliases[str(len(aliases))] = {"name": "postmaster", "domainId": os.environ["D"]}
    print(json.dumps({"aliases": aliases}))')
    if [ -n "$aliases" ]; then
        cli update Account "$postmaster_id" --json "$aliases" > /dev/null
        echo "Alias postmaster@$d -> postmaster@$DOMAIN added."
    fi
done

# The DNS zone is kept by hand, so Stalwart must not rotate DKIM keys by itself (a new key would sign mail before
# anyone published it and the mail would fail DKIM). The current keys stay; rotate by hand if ever needed.
for d in "$DOMAIN" $EXTRA_DOMAINS; do
    d_id=$(domain_id_of "$d")
    if [ "$(cli get Domain "$d_id" --json | python3 -c 'import json, sys; print(json.load(sys.stdin)["dkimManagement"]["@type"])')" != Manual ]; then
        cli update Domain "$d_id" --json '{"dkimManagement":{"@type":"Manual"}}' > /dev/null
        echo "DKIM keys of $d: automatic rotation turned off."
    fi
done

if [ "$changed" = true ]; then
    docker compose restart mail
    wait_for_mail
fi

# 5. DNS records. Left out: TLSA (the certificate key changes with every renewal and the zone has no DNSSEC) and POP3
# (port 995 is not published).
echo
echo "=== DNS records for $DOMAIN (add at the DNS provider; see deploy/MAIL.md step 5) ==="
cli get Domain "$domain_id" --fields dnsZoneFile --json \
    | python3 -c 'import json, sys; print(json.load(sys.stdin)["dnsZoneFile"])' \
    | grep -v -e ' TLSA ' -e '^_pop3s\.'
# Extra domains: only what mail needs (DKIM, SPF, MX, DMARC, TLS reports). Their mail clients use $MAIL_HOSTNAME
# directly, so no SRV/autoconfig/MTA-STS names (those would need more certificates in Caddy).
for d in $EXTRA_DOMAINS; do
    echo
    echo "=== DNS records for $d ==="
    cli get Domain "$(domain_id_of "$d")" --fields dnsZoneFile --json \
        | python3 -c 'import json, sys; print(json.load(sys.stdin)["dnsZoneFile"])' \
        | awk '/^[^ ]+\._domainkey\./ { keep = 1 } /^[^ ]+\. IN (TXT "v=spf1|MX)/ || /^_dmarc\./ || /^_smtp\._tls\./ { keep = 1 }
               keep { print } /^[^ (]/ && !/\($/ || /^\)/ { keep = 0 }'
done
echo
echo "Before adding them: if anything else still sends as @$DOMAIN through Brevo (vutregbot), keep Brevo in SPF:"
echo "  $DOMAIN. IN TXT \"v=spf1 mx include:spf.brevo.com ~all\"   (instead of the SPF line above),"
echo "start DMARC with p=none and keep Brevo's brevo1/brevo2 DKIM records. Details in deploy/MAIL.md."
echo "Done. The app still sends through the relay; switch with:  bash mail/switch-smtp.sh own"
