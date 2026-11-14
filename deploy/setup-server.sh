#!/usr/bin/env bash
# One-time preparation of the Hetzner Cloud server (Debian 13 or Ubuntu 24.04, amd64 or arm64) for Zagejmi. Run as root:
#
#   ssh root@<server-ip> 'bash -s' < deploy/setup-server.sh "<public key of the GitHub deploy key>"
#
# Installs Docker, creates the "deploy" user that GitHub Actions logs in as (key-only, member of the docker group),
# prepares /opt/zagejmi and opens only SSH, HTTP and HTTPS in the firewall. Safe to run again.
# HTTPS itself is the Caddy already installed on the server (/etc/caddy/Caddyfile); this script does not touch it.
set -euo pipefail

DEPLOY_PUBLIC_KEY="${1:?usage: setup-server.sh \"ssh-ed25519 AAAA... github-deploy\"}"
APP_DIR=/opt/zagejmi

if [[ $EUID -ne 0 ]]; then
    echo "Run as root." >&2
    exit 1
fi

echo "==> Updating packages"
export DEBIAN_FRONTEND=noninteractive
apt-get update -q
apt-get upgrade -yq
apt-get install -yq ca-certificates curl ufw unattended-upgrades

echo "==> Installing Docker"
if ! command -v docker > /dev/null; then
    install -m 0755 -d /etc/apt/keyrings
    . /etc/os-release # ID is "debian" or "ubuntu"; Docker publishes a repository for each
    curl -fsSL "https://download.docker.com/linux/${ID}/gpg" -o /etc/apt/keyrings/docker.asc
    chmod a+r /etc/apt/keyrings/docker.asc
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/${ID} ${VERSION_CODENAME} stable" \
        > /etc/apt/sources.list.d/docker.list
    apt-get update -q
    apt-get install -yq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
fi
systemctl enable --now docker

echo "==> Creating the deploy user"
if ! id deploy > /dev/null 2>&1; then
    useradd --create-home --shell /bin/bash deploy
fi
usermod -aG docker deploy
install -d -m 700 -o deploy -g deploy /home/deploy/.ssh
touch /home/deploy/.ssh/authorized_keys
grep -qxF "$DEPLOY_PUBLIC_KEY" /home/deploy/.ssh/authorized_keys || echo "$DEPLOY_PUBLIC_KEY" >> /home/deploy/.ssh/authorized_keys
chown deploy:deploy /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys

echo "==> Preparing $APP_DIR"
install -d -m 750 -o deploy -g deploy "$APP_DIR"

echo "==> Firewall: SSH, HTTP, HTTPS only"
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp
ufw --force enable

echo "==> Automatic security updates"
dpkg-reconfigure -f noninteractive unattended-upgrades

echo
echo "Done. Next: create $APP_DIR/.env from deploy/.env.example (as the deploy user, chmod 600),"
echo "then push to the production branch."
