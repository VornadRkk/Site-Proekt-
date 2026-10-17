# Production deployment

Frontend is served by nginx. `/api/` and `/health` are forwarded to the ASP.NET Core API on `127.0.0.1:5080`. MySQL stays on the existing server. The API runs as the dedicated `dateandtime` account; CI signs in as `dateandtime-deploy` and can invoke only the installed deployment wrapper with sudo.

Releases live in `/var/www/dateandtime/releases/`. Deployment switches the `current` symlink, restarts the API, and checks both nginx and database-backed `/api/services`. A failure restores the previous release automatically. Code rollback does **not** reverse database migrations. Make future schema migrations compatible with the previous release and back up MySQL before schema changes.

## Initial server setup

These scripts target a Debian/Ubuntu x86-64 VPS with systemd. Inspect the existing nginx virtual hosts and MySQL databases before making changes. The provisioning script does not replace the distro nginx default or other sites.

Install `nginx`, `curl`, `python3`, `sudo`, `tar`, and `util-linux`, plus the native dependencies needed for .NET 8 self-contained applications (including ICU and OpenSSL for the server's distribution). No Node.js or .NET SDK is needed on the VPS: GitHub builds the artifacts.

Create a dedicated MySQL database and account with privileges limited to that database. The API applies EF migrations on startup, so the account must have schema-change privileges in its own database. Do not use the MySQL root account.

Use the server's actual numeric version in `Database__ServerVersion` (for example `5.7.44` for MySQL 5.7, without distribution suffixes). The initial migration uses types and indexes available in MySQL 5.7; it does not require MySQL 8-specific collations, JSON functions, or generated columns. [Pomelo's compatibility notes](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#supported-database-servers-and-versions) describe older servers such as 5.7 as generally compatible, but do not include them in current CI, so verify migrations and saving an application against the actual server before accepting production traffic. The existing MySQL installation is not upgraded by these scripts.

Create `/etc/dateandtime/api.env` from [api.env.example](api.env.example), replacing the connection string and MySQL version. Keep it root-owned with permissions `600`. Passwords containing connection-string or EnvironmentFile quoting characters must be escaped according to their respective formats.

Also set `Jwt__Key` to a cryptographically random secret of at least 32 characters, and set `Admin__Email` / `Admin__Password` to the initial administrator credentials. These values are read only when the users table is empty; changing the environment password later does not reset the stored password hash. Do not use the development-only credentials from `appsettings.Development.json` on a shared server.

Generate an Ed25519 key dedicated to Actions. Keep the private key outside the repository, and copy only its public key to the VPS. Upload the `deploy/` directory, then run as root:

```bash
bash deploy/provision.sh 194.226.125.251 /root/dateandtime-actions.pub
```

The public key receives the OpenSSH `restrict` option, disabling forwarding and interactive terminals. SFTP/SCP and the deployment command remain available. Running provision again updates this project's files and preserves existing authorized keys.

The nginx template initially uses HTTP. When a domain is available, point its DNS to the VPS, provision with that hostname, and enable HTTPS with the site's certificate. Check the firewall permits TCP 80 (and 443 for HTTPS); leave the API and MySQL ports private.

## GitHub Actions

The workflow runs after a push to `main` and through **Actions → Deploy production → Run workflow**. It builds with `VITE_USE_MOCKS=false` and an empty `VITE_API_URL`, so the browser calls the same site's API. Build and test checks run before an upload. Deployments are serialized.

In repository **Settings → Secrets and variables → Actions**, add these repository secrets:

| Secret | Value |
| --- | --- |
| `DEPLOY_HOST` | `194.226.125.251` or the VPS hostname |
| `DEPLOY_USER` | `dateandtime-deploy` |
| `DEPLOY_SSH_KEY` | The dedicated private Ed25519 key, including its header and footer |
| `DEPLOY_KNOWN_HOSTS` | The VPS SSH host-key entry, verified against the server's fingerprint |

For a nonstandard SSH port, set the repository variable `DEPLOY_PORT`; otherwise port 22 is used. For nonstandard ports the known_hosts entry must use `[host]:port`. Read the fingerprint on the VPS with `ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub`, and verify it before saving the corresponding host-key entry. The workflow enforces strict host-key verification.

No database password is stored in GitHub: it remains only in `/etc/dateandtime/api.env` on the VPS. Do not upload the SSH root password, MySQL root password, or `.env` files into the repository.

## Manual first release

Build in Linux/CI with the same commands as the workflow. The archive must contain `frontend/index.html` and the self-contained `backend/DateAndTime.Api` binary at its root. Upload `RELEASE_ID.tar.gz` to `/var/lib/dateandtime-deploy/incoming/`, then execute:

```bash
sudo /usr/local/sbin/dateandtime-deploy RELEASE_ID
```

Each release ID must be unique. Existing releases are retained for recovery; remove old releases separately after verifying which one `current` points to.

For troubleshooting:

```bash
systemctl status dateandtime-api --no-pager
journalctl -u dateandtime-api -n 80 --no-pager
nginx -t
curl --fail http://127.0.0.1:5080/health
curl --fail -H 'Host: 194.226.125.251' http://127.0.0.1/api/services
```

Official references: [ASP.NET Core with nginx and systemd](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-8.0), [GitHub Actions secrets](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets), [workflow concurrency](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency).
