# Installing Clovance

Clovance ships with cross-platform installers for Windows, Linux, and macOS. They check Docker, ask a few questions, generate the configuration, pull the images, and start the app.

## 🚀 Quick install

**Linux / macOS:**

```bash
curl -fsSL https://evaristocuesta.github.io/clovance/install/install.sh | sh
```

**Windows (PowerShell):**

```powershell
irm https://evaristocuesta.github.io/clovance/install/install.ps1 | iex
```

You can also run the scripts directly from a local checkout:

```bash
# Linux / macOS
./docs/install/install.sh

# Windows
.\docs\install\install.ps1
```

## 📋 Prerequisites

- Docker (Docker Desktop on Windows/macOS, or Docker Engine on Linux).
- Docker Compose plugin v2 (`docker compose`, not the legacy `docker-compose`).
- The Docker daemon must be running.

The installer verifies these before doing anything else.

## ⚙️ Install parameters

During installation you will be prompted for the following values (defaults shown in brackets):

| Prompt | Default | Description |
|---|---|---|
| Install directory | `~/clovance` (Linux/macOS), `%USERPROFILE%\clovance` (Windows) | Where `.env`, `docker-compose.yml`, and data are stored. |
| Image version tag | `latest` | Docker image tag to pull (e.g. `1.0.0`). Note: tags do **not** include a leading `v`. |
| Frontend host port | `7000` | Host port exposed for the Clovance UI. |
| API container port | `8080` | Internal port of the API service. |
| Bind address (frontend) | `0.0.0.0` | Interface the frontend binds to. Use `127.0.0.1` to restrict to localhost. |
| Bind address (dashboard) | `127.0.0.1` | Interface the Aspire dashboard binds to. |
| PostgreSQL username | `clovance` | Database user. |
| PostgreSQL password | *(auto-generated)* | Leave empty to generate a random password. |
| SMTP host | `smtp.example.com` | Mail server used for invitations and password resets. |
| SMTP port | `587` | Mail server port. |
| SMTP username | `your_smtp_username` | Mail server username. |
| SMTP password | *(empty)* | Mail server password. |
| SMTP from address | `your_smtp_from_address@example.com` | Sender address. |
| SMTP from name | `Clovance` | Sender display name. |
| Frontend base URL | `http://localhost:7000` | Public URL of the frontend (used by the API). |

Data is stored under `<install-dir>/data` (Postgres data and the JWT key).

## 🔄 Update

To upgrade to a new version:

```bash
# Linux / macOS
./install.sh update

# Windows
.\install.ps1 update
```

You will be asked for the install directory and the new image version tag. The script backs up `.env` to `.env.bak`, updates the image tags, pulls the new images, and recreates the containers.

## 🗑️ Uninstall

```bash
# Linux / macOS
./install.sh uninstall

# Windows
.\install.ps1 uninstall
```

The script stops and removes the containers, then asks:

1. **Remove data volumes (postgres + jwt)?** — answer `y` to delete the database and JWT key.
2. **Remove generated config files (.env, docker-compose.yml)?** — answer `y` to clean up the configuration.

On Linux/macOS, if the data directory was created by the container (root-owned), the script will try `sudo` automatically; if that fails, run `sudo rm -rf <install-dir>/data` manually.

## 🛠️ Troubleshooting

- **Docker not running**: start Docker Desktop / the Docker daemon and retry.
- **Compose v2 missing**: upgrade Docker or install the Compose plugin.
- **Permission denied on data removal (Linux)**: use `sudo rm -rf <install-dir>/data`.
- **Port already in use**: choose a different frontend port during install, or stop the conflicting service.

## 📚 Related documentation

- [Docker Usage Guide](./DOCKER_USAGE.md) - Manual Docker Compose setup, ports, reverse proxy.
- [Docker Image Publishing](./DOCKER_PUBLISH.md) - Publishing and deployment guide.