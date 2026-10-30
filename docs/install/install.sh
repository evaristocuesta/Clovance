#!/usr/bin/env bash
# Clovance installer (Linux/macOS)
# Usage:
#   ./install.sh            (install)
#   ./install.sh install
#   ./install.sh uninstall
#   ./install.sh update
set -euo pipefail

REPO_RAW="https://raw.githubusercontent.com/evaristocuesta/Clovance/main"
PAGES_BASE="https://evaristocuesta.github.io/clovance"

DEFAULT_INSTALL_DIR="$HOME/clovance"
DEFAULT_VERSION="latest"
DEFAULT_FRONTEND_PORT="7000"
DEFAULT_API_PORT="8080"
DEFAULT_BIND="0.0.0.0"
DEFAULT_DASHBOARD_BIND="127.0.0.1"

# ---------- helpers ----------
log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
warn() { printf '\033[1;33m!!\033[0m %s\n' "$*" >&2; }
err() { printf '\033[1;31mxx\033[0m %s\n' "$*" >&2; }

prompt() {
  # prompt <varname> <default> <message>
  local var="$1" def="$2" msg="$3" val
  if [ -t 0 ]; then
    read -r -p "$msg [$def]: " val
    val="${val:-$def}"
  else
    val="$def"
  fi
  printf -v "$var" '%s' "$val"
}

prompt_secret() {
  local var="$1" msg="$2" val
  if [ -t 0 ]; then
    read -r -s -p "$msg: " val; echo
  else
    val=""
  fi
  printf -v "$var" '%s' "$val"
}

gen_password() {
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 18 | tr -d '\n' | tr '/+' '_-'
  else
    head -c 24 /dev/urandom | base64 | tr -d '\n' | tr '/+' '_-'
  fi
}

require() {
  command -v "$1" >/dev/null 2>&1 || { err "Missing required command: $1"; exit 1; }
}

check_docker() {
  require docker
  docker info >/dev/null 2>&1 || { err "Docker daemon is not running. Start Docker and retry."; exit 1; }
  docker compose version >/dev/null 2>&1 || { err "Docker Compose plugin (v2) is required."; exit 1; }
}

compose_file() { echo "$INSTALL_DIR/docker-compose.yml"; }
env_file() { echo "$INSTALL_DIR/.env"; }

write_compose() {
  cat > "$(compose_file)" <<'YAML'
services:
  env-dashboard:
    image: "mcr.microsoft.com/dotnet/nightly/aspire-dashboard:13.6"
    ports:
      - "${CLOVANCE_DASHBOARD_BIND_ADDRESS:-0.0.0.0}::18888"
    expose:
      - "18889"
      - "18890"
    networks:
      - "aspire"
    restart: "always"
  clovance-postgres:
    image: "docker.io/library/postgres:18.3"
    environment:
      POSTGRES_HOST_AUTH_METHOD: "scram-sha-256"
      POSTGRES_INITDB_ARGS: "--auth-host=scram-sha-256 --auth-local=scram-sha-256"
      POSTGRES_USER: "${POSTGRES_USERNAME}"
      POSTGRES_PASSWORD: "${POSTGRES_PASSWORD}"
      POSTGRES_DB: "clovance-database"
    expose:
      - "5432"
    volumes:
      - type: "bind"
        target: "/var/lib/postgresql"
        source: "${VOLUME_SOURCE_PATH}/postgres"
        read_only: false
    networks:
      - "aspire"
    restart: "unless-stopped"
  clovance-apiservice:
    image: "${CLOVANCE_APISERVICE_IMAGE}"
    environment:
      OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY: "in_memory"
      ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
      HTTP_PORTS: "${CLOVANCE_APISERVICE_PORT}"
      ConnectionStrings__clovance-database: "Host=clovance-postgres;Port=5432;Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};Database=clovance-database"
      CLOVANCE_DATABASE_HOST: "clovance-postgres"
      CLOVANCE_DATABASE_PORT: "5432"
      CLOVANCE_DATABASE_USERNAME: "${POSTGRES_USERNAME}"
      CLOVANCE_DATABASE_PASSWORD: "${POSTGRES_PASSWORD}"
      CLOVANCE_DATABASE_URI: "postgresql://${POSTGRES_USERNAME}:${POSTGRES_PASSWORD}@clovance-postgres:5432/clovance-database"
      CLOVANCE_DATABASE_JDBCCONNECTIONSTRING: "jdbc:postgresql://clovance-postgres:5432/clovance-database"
      CLOVANCE_DATABASE_DATABASENAME: "clovance-database"
      ASPNETCORE_ENVIRONMENT: "Production"
      Jwt__KeyFilePath: "/home/app/jwt.key"
      Smtp__Host: "${SMTP_HOST}"
      Smtp__Port: "${SMTP_PORT}"
      Smtp__Username: "${SMTP_USERNAME}"
      Smtp__Password: "${SMTP_PASSWORD}"
      Smtp__FromAddress: "${SMTP_FROM_ADDRESS}"
      Smtp__FromName: "${SMTP_FROM_NAME}"
      Frontend__BaseUrl: "${FRONTEND_BASE_URL}"
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-apiservice"
    expose:
      - "${CLOVANCE_APISERVICE_PORT}"
    volumes:
      - type: "bind"
        target: "/home/app"
        source: "${VOLUME_SOURCE_PATH}/jwt"
        read_only: false
    depends_on:
      clovance-postgres:
        condition: "service_started"
    networks:
      - "aspire"
    restart: "unless-stopped"
  clovance-frontend:
    image: "${CLOVANCE_FRONTEND_IMAGE}"
    command:
      - "nginx"
      - "-g"
      - "daemon off;"
    entrypoint:
      - "/docker-entrypoint.sh"
    environment:
      NODE_ENV: "production"
      CLOVANCE_APISERVICE_HTTP: "http://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      services__clovance-apiservice__http__0: "http://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      CLOVANCE_APISERVICE_HTTPS: "https://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      PORT: "7000"
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-frontend"
    ports:
      - "${CLOVANCE_BIND_ADDRESS:-0.0.0.0}:7000:7000"
    depends_on:
      clovance-apiservice:
        condition: "service_started"
    networks:
      - "aspire"
    restart: "unless-stopped"
networks:
  aspire:
    driver: "bridge"
YAML
}

do_install() {
  check_docker
  prompt INSTALL_DIR "$DEFAULT_INSTALL_DIR" "Install directory"
  prompt VERSION "$DEFAULT_VERSION" "Image version tag (e.g. latest, 1.0.0)"
  prompt FRONTEND_PORT "$DEFAULT_FRONTEND_PORT" "Frontend host port"
  prompt API_PORT "$DEFAULT_API_PORT" "API container port"
  prompt BIND "$DEFAULT_BIND" "Bind address for frontend"
  prompt DASHBOARD_BIND "$DEFAULT_DASHBOARD_BIND" "Bind address for Aspire dashboard"
  prompt PG_USER "clovance" "PostgreSQL username"
  prompt_secret PG_PASS "PostgreSQL password (empty = auto-generate)"
  [ -z "$PG_PASS" ] && PG_PASS="$(gen_password)" && log "Generated PostgreSQL password."
  prompt SMTP_HOST "smtp.example.com" "SMTP host"
  prompt SMTP_PORT "587" "SMTP port"
  prompt SMTP_USER "your_smtp_username" "SMTP username"
  prompt_secret SMTP_PASS "SMTP password"
  prompt SMTP_FROM "your_smtp_from_address@example.com" "SMTP from address"
  prompt SMTP_FROM_NAME "Clovance" "SMTP from name"
  prompt FRONTEND_URL "http://localhost:${FRONTEND_PORT}" "Frontend base URL"

  mkdir -p "$INSTALL_DIR"
  VOLUME_PATH="$INSTALL_DIR/data"
  mkdir -p "$VOLUME_PATH/postgres" "$VOLUME_PATH/jwt"

  API_IMAGE="ghcr.io/evaristocuesta/clovance/clovance-api:${VERSION}"
  FE_IMAGE="ghcr.io/evaristocuesta/clovance/clovance-frontend:${VERSION}"

  cat > "$(env_file)" <<EOF
CLOVANCE_APISERVICE_IMAGE=${API_IMAGE}
CLOVANCE_APISERVICE_PORT=${API_PORT}
CLOVANCE_FRONTEND_IMAGE=${FE_IMAGE}
FRONTEND_BASE_URL=${FRONTEND_URL}
POSTGRES_PASSWORD=${PG_PASS}
POSTGRES_USERNAME=${PG_USER}
SMTP_FROM_ADDRESS=${SMTP_FROM}
SMTP_FROM_NAME=${SMTP_FROM_NAME}
SMTP_HOST=${SMTP_HOST}
SMTP_PASSWORD=${SMTP_PASS}
SMTP_PORT=${SMTP_PORT}
SMTP_USERNAME=${SMTP_USER}
VOLUME_SOURCE_PATH=${VOLUME_PATH}
CLOVANCE_BIND_ADDRESS=${BIND}
CLOVANCE_DASHBOARD_BIND_ADDRESS=${DASHBOARD_BIND}
EOF

  write_compose
  log "Configuration written to $INSTALL_DIR"

  cd "$INSTALL_DIR"
  log "Pulling images..."
  docker compose pull
  log "Starting Clovance..."
  docker compose up -d

  echo
  log "Clovance is running!"
  echo "   Frontend:  http://localhost:${FRONTEND_PORT}"
  echo "   Data dir:  ${VOLUME_PATH}"
  echo
  echo "   Aspire dashboard token:"
  echo "     docker logs \$(docker compose ps -q env-dashboard) 2>&1 | grep -i token"
  echo
  echo "   Stop:    cd $INSTALL_DIR && docker compose down"
  echo "   Update:  $0 update"
}

do_uninstall() {
  check_docker
  prompt INSTALL_DIR "$DEFAULT_INSTALL_DIR" "Install directory"
  if [ ! -f "$(compose_file)" ]; then
    err "No docker-compose.yml found in $INSTALL_DIR"; exit 1
  fi
  cd "$INSTALL_DIR"
  log "Stopping containers..."
  docker compose down
  if [ -t 0 ]; then
    read -r -p "Remove data volumes (postgres + jwt)? [y/N]: " rmvol
    rmvol="${rmvol:-N}"
  else
    rmvol="N"
  fi
  if [[ "$rmvol" =~ ^[Yy]$ ]]; then
    if rm -rf "$INSTALL_DIR/data" 2>/dev/null; then
      log "Data removed."
    elif command -v sudo >/dev/null 2>&1 && sudo rm -rf "$INSTALL_DIR/data" 2>/dev/null; then
      log "Data removed (with sudo)."
    else
      warn "Could not remove $INSTALL_DIR/data (permission denied)."
      warn "Run manually: sudo rm -rf $INSTALL_DIR/data"
    fi
  fi
  read -r -p "Remove generated config files (.env, docker-compose.yml)? [y/N]: " rmcfg 2>/dev/null || rmcfg="N"
  rmcfg="${rmcfg:-N}"
  if [[ "$rmcfg" =~ ^[Yy]$ ]]; then
    rm -f "$INSTALL_DIR/.env" "$INSTALL_DIR/docker-compose.yml"
    log "Config removed."
  fi
  log "Uninstall complete."
}

do_update() {
  check_docker
  prompt INSTALL_DIR "$DEFAULT_INSTALL_DIR" "Install directory"
  if [ ! -f "$(env_file)" ]; then
    err "No .env found in $INSTALL_DIR"; exit 1
  fi
  prompt VERSION "$DEFAULT_VERSION" "New image version tag"
  cp "$(env_file)" "$(env_file).bak"
  # update image lines
  sed -i.bak "s|^CLOVANCE_APISERVICE_IMAGE=.*|CLOVANCE_APISERVICE_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-api:${VERSION}|" "$(env_file)"
  sed -i.bak "s|^CLOVANCE_FRONTEND_IMAGE=.*|CLOVANCE_FRONTEND_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-frontend:${VERSION}|" "$(env_file)"
  rm -f "$(env_file).bak"
  cd "$INSTALL_DIR"
  log "Pulling new images..."
  docker compose pull
  log "Recreating containers..."
  docker compose up -d
  log "Update complete."
}

CMD="${1:-install}"
case "$CMD" in
  install) do_install ;;
  uninstall) do_uninstall ;;
  update) do_update ;;
  *)
    echo "Usage: $0 [install|uninstall|update]"
    exit 1
    ;;
esac