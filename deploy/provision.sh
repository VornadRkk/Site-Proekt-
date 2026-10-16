#!/usr/bin/env bash
# Run once as root on a Debian/Ubuntu VPS after installing nginx and prerequisites.
set -euo pipefail

[[ $EUID -eq 0 ]] || { echo 'Run provision.sh as root.' >&2; exit 1; }
SERVER_NAME=${1:-194.226.125.251}
PUBLIC_KEY_FILE=${2:-}
[[ "$SERVER_NAME" =~ ^[A-Za-z0-9][A-Za-z0-9.-]*$ ]] || { echo 'Pass one IP address or hostname.' >&2; exit 1; }
for command in nginx python3 curl systemctl tar flock sudo useradd install visudo; do
    command -v "$command" >/dev/null || { echo "Install prerequisite: $command" >&2; exit 1; }
done
[[ -f /etc/dateandtime/api.env ]] || { echo 'Create /etc/dateandtime/api.env from api.env.example first.' >&2; exit 1; }
if [[ -n "$PUBLIC_KEY_FILE" ]]; then
    [[ -f "$PUBLIC_KEY_FILE" ]] || { echo 'Public key file does not exist.' >&2; exit 1; }
    ssh-keygen -lf "$PUBLIC_KEY_FILE" >/dev/null
fi

SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
id dateandtime >/dev/null 2>&1 || useradd --system --home-dir /var/lib/dateandtime --shell /usr/sbin/nologin dateandtime
id dateandtime-deploy >/dev/null 2>&1 || useradd --create-home --shell /bin/bash dateandtime-deploy
install -d -m 755 /var/www/dateandtime /var/www/dateandtime/releases /var/lib/dateandtime-deploy
install -d -o dateandtime-deploy -g dateandtime-deploy -m 700 /var/lib/dateandtime-deploy/incoming
chown root:root /etc/dateandtime/api.env
chmod 600 /etc/dateandtime/api.env
install -m 755 "$SCRIPT_DIR/deploy.sh" /usr/local/sbin/dateandtime-deploy
install -m 644 "$SCRIPT_DIR/dateandtime-api.service" /etc/systemd/system/dateandtime-api.service
printf '%s\n' "$SERVER_NAME" > /etc/dateandtime/server-name
chmod 644 /etc/dateandtime/server-name

SUDOERS_FILE=$(mktemp)
trap 'rm -f "$SUDOERS_FILE"' EXIT
printf '%s\n' 'dateandtime-deploy ALL=(root) NOPASSWD: /usr/local/sbin/dateandtime-deploy *' > "$SUDOERS_FILE"
visudo -cf "$SUDOERS_FILE"
install -m 440 "$SUDOERS_FILE" /etc/sudoers.d/dateandtime-deploy

if [[ -n "$PUBLIC_KEY_FILE" ]]; then
    DEPLOY_HOME=$(getent passwd dateandtime-deploy | cut -d: -f6)
    install -d -o dateandtime-deploy -g dateandtime-deploy -m 700 "$DEPLOY_HOME/.ssh"
    touch "$DEPLOY_HOME/.ssh/authorized_keys"
    PUBLIC_KEY=$(cat -- "$PUBLIC_KEY_FILE")
    grep -qF -- "$PUBLIC_KEY" "$DEPLOY_HOME/.ssh/authorized_keys" || printf 'restrict %s\n' "$PUBLIC_KEY" >> "$DEPLOY_HOME/.ssh/authorized_keys"
    chown dateandtime-deploy:dateandtime-deploy "$DEPLOY_HOME/.ssh/authorized_keys"
    chmod 600 "$DEPLOY_HOME/.ssh/authorized_keys"
fi

# Keep other virtual hosts, including the distro default, untouched.
NGINX_CANDIDATE=$(mktemp)
sed "s/__SERVER_NAME__/$SERVER_NAME/g" "$SCRIPT_DIR/nginx.conf" > "$NGINX_CANDIDATE"
if [[ -f /etc/nginx/sites-available/dateandtime ]]; then
    cp -a /etc/nginx/sites-available/dateandtime /etc/nginx/sites-available/dateandtime.previous
fi
install -m 644 "$NGINX_CANDIDATE" /etc/nginx/sites-available/dateandtime
rm -f "$NGINX_CANDIDATE"
ln -sfn /etc/nginx/sites-available/dateandtime /etc/nginx/sites-enabled/dateandtime
if ! nginx -t; then
    if [[ -f /etc/nginx/sites-available/dateandtime.previous ]]; then
        mv -f /etc/nginx/sites-available/dateandtime.previous /etc/nginx/sites-available/dateandtime
    else
        rm -f /etc/nginx/sites-enabled/dateandtime /etc/nginx/sites-available/dateandtime
    fi
    exit 1
fi
systemctl daemon-reload
systemctl enable dateandtime-api nginx
systemctl reload-or-restart nginx
echo 'Provisioned. Upload a release, then run: sudo /usr/local/sbin/dateandtime-deploy RELEASE_ID'
