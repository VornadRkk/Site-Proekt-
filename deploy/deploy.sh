#!/usr/bin/env bash
# Installed root-owned by provision.sh. The SSH user may invoke only this wrapper.
set -euo pipefail

[[ $EUID -eq 0 ]] || { echo 'Run this wrapper with sudo.' >&2; exit 1; }
[[ $# -eq 1 && "$1" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,99}$ ]] || { echo 'Usage: dateandtime-deploy RELEASE_ID' >&2; exit 1; }
RELEASE_ID=$1
BASE=/var/www/dateandtime
RELEASE="$BASE/releases/$RELEASE_ID"
INCOMING="/var/lib/dateandtime-deploy/incoming/$RELEASE_ID.tar.gz"
[[ -f "$INCOMING" && ! -L "$INCOMING" ]] || { echo 'Uploaded release archive is missing.' >&2; exit 1; }
[[ -f /etc/dateandtime/api.env && -f /etc/dateandtime/server-name ]] || { echo 'Run provision.sh first.' >&2; exit 1; }
[[ ! -e "$RELEASE" ]] || { echo 'Release already exists; use a new release ID.' >&2; exit 1; }

exec 9>/run/lock/dateandtime-deploy.lock
flock -w 300 9 || { echo 'Another deployment holds the lock.' >&2; exit 1; }
PREVIOUS=''
if [[ -L "$BASE/current" ]]; then
    PREVIOUS=$(readlink -f "$BASE/current")
fi
if [[ -e "$BASE/current" && ! -L "$BASE/current" ]]; then
    echo 'The current release must be a symbolic link.' >&2
    exit 1
fi
if [[ -n "$PREVIOUS" && "$PREVIOUS" != "$BASE"/releases/* ]]; then
    echo 'Current release points outside the release directory.' >&2
    exit 1
fi
SERVER_NAME=$(cat /etc/dateandtime/server-name)
ARCHIVE_COPY=$(mktemp /var/lib/dateandtime-deploy/archive.XXXXXXXX.tar.gz)
SWITCHED=false
COMMITTED=false

finish() {
    local status=$?
    trap - EXIT INT TERM
    set +e
    if [[ "$SWITCHED" == true && "$COMMITTED" != true ]]; then
        echo 'Health verification failed. Restoring the previous release.' >&2
        if [[ -n "$PREVIOUS" && -d "$PREVIOUS" ]]; then
            ln -sfn "$PREVIOUS" "$BASE/.current-next"
            mv -Tf "$BASE/.current-next" "$BASE/current"
            systemctl reset-failed dateandtime-api || true
            systemctl restart dateandtime-api || true
        else
            systemctl stop dateandtime-api || true
            rm -f "$BASE/current"
        fi
        status=1
    fi
    rm -f "$ARCHIVE_COPY"
    exit "$status"
}
trap finish EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

# Copy into a root-only file before validation/extraction to avoid upload races.
cp -- "$INCOMING" "$ARCHIVE_COPY"
chmod 600 "$ARCHIVE_COPY"
python3 - "$ARCHIVE_COPY" <<'PY'
import pathlib
import sys
import tarfile

with tarfile.open(sys.argv[1], "r:gz") as archive:
    total_size = 0
    for entry in archive.getmembers():
        path = pathlib.PurePosixPath(entry.name)
        if (path.is_absolute() or ".." in path.parts or not path.parts
                or path.parts[0] not in {"frontend", "backend"}
                or not (entry.isfile() or entry.isdir())):
            raise SystemExit("Archive contains an unsafe entry")
        total_size += entry.size
        if total_size > 2 * 1024 ** 3:
            raise SystemExit("Release archive is too large")
PY

install -d -m 755 "$RELEASE"
tar --extract --gzip --file "$ARCHIVE_COPY" --directory "$RELEASE" --no-same-owner --no-same-permissions
[[ -f "$RELEASE/frontend/index.html" && -f "$RELEASE/backend/DateAndTime.Api" ]] || { echo 'Archive is missing frontend or backend artifacts.' >&2; exit 1; }
chown -R root:root "$RELEASE"
chmod -R u=rwX,go=rX "$RELEASE"
chmod 755 "$RELEASE/backend/DateAndTime.Api"

ln -sfn "$RELEASE" "$BASE/.current-next"
mv -Tf "$BASE/.current-next" "$BASE/current"
SWITCHED=true
systemctl reset-failed dateandtime-api || true
systemctl restart dateandtime-api

HEALTHY=false
for attempt in {1..40}; do
    if curl --fail --silent --max-time 3 http://127.0.0.1:5080/health >/dev/null && \
       curl --fail --silent --max-time 5 http://127.0.0.1:5080/api/services | python3 -c 'import json,sys; value=json.load(sys.stdin); sys.exit(0 if isinstance(value,list) else 1)'; then
        HEALTHY=true
        break
    fi
    sleep 2
done
[[ "$HEALTHY" == true ]] || { echo 'API or database verification failed.' >&2; exit 1; }
curl --fail --silent --max-time 10 -H "Host: $SERVER_NAME" http://127.0.0.1/ >/dev/null
curl --fail --silent --max-time 10 -H "Host: $SERVER_NAME" http://127.0.0.1/health >/dev/null
curl --fail --silent --max-time 10 -H "Host: $SERVER_NAME" http://127.0.0.1/api/services | python3 -c 'import json,sys; value=json.load(sys.stdin); sys.exit(0 if isinstance(value,list) else 1)'
COMMITTED=true
rm -f -- "$INCOMING"
printf 'Deployed %s successfully.\n' "$RELEASE_ID"
