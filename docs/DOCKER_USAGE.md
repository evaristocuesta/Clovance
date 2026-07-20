# Using Clovance Docker Images

This document shows how to use the Docker images published to GitHub Container Registry (GHCR).

## 🐳 Docker Compose

Create a `.env` file by filling in the api service image, api service port, frontend image and postgres username and password. 

```
# Container image name for clovance-apiservice
CLOVANCE_APISERVICE_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0-alpha1

# Default container port for clovance-apiservice
CLOVANCE_APISERVICE_PORT=8080

# Container image name for clovance-frontend
CLOVANCE_FRONTEND_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-frontend:1.0.0-alpha1

# Parameter postgres-password
POSTGRES_PASSWORD=your_postgres_password

# Parameter postgres-username
POSTGRES_USERNAME=your_postgres_username
```

Create a `docker-compose.yml` file to run the complete application:

```yaml
services:
  env-dashboard:
    image: "mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest"
    ports:
      - "18888"
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
      - type: "volume"
        target: "/var/lib/postgresql"
        source: "clovance.apphost-f8d48ee71c-clovance-postgres-data"
        read_only: false
    networks:
      - "aspire"
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
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-apiservice"
    ports:
      - "${CLOVANCE_APISERVICE_PORT}"
    depends_on:
      clovance-postgres:
        condition: "service_started"
    networks:
      - "aspire"
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
      PORT: "8000"
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-frontend"
    ports:
      - "8000"
    depends_on:
      clovance-apiservice:
        condition: "service_started"
    networks:
      - "aspire"
networks:
  aspire:
    driver: "bridge"
volumes:
  clovance.apphost-f8d48ee71c-clovance-postgres-data:
    driver: "local"
```

Start the application:

```bash
docker compose up -d
```

Stop the application:

```bash
docker compose down
```

To also remove the database volume:

```bash
docker compose down -v
```

---

## 🎯 Pull a Specific Version

```bash
# Stable releases
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:latest

# Pre-releases
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0-beta1
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v2.0.0-rc.2
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0-alpha
```

> **Note:** Pre-release versions **do not** update the `latest`, `v1`, or `v1.0` tags.

---

## 🧪 Run Only the API

```bash
docker run -d \
  --name clovance-api \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Database=clovance;Username=postgres;Password=postgres" \
  ghcr.io/evaristocuesta/clovance/clovance-api:latest
```

---

## 🖥️ Run Only the Frontend

```bash
docker run -d \
  --name clovance-frontend \
  -p 8000:8000 \
  -e API_URL=http://localhost:8080 \
  ghcr.io/evaristocuesta/clovance/clovance-frontend:latest
```

---

## 🔍 Inspect Docker Images

Display image information:

```bash
docker inspect ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0
```

Display image layers and size:

```bash
docker history ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0
```

---

## 📊 Multi-Architecture Support

Docker images are published for multiple architectures:

- `linux/amd64` (x86_64)
- `linux/arm64` (Apple Silicon and ARM servers)

Docker automatically pulls the correct image for your platform.

---

## 🚀 GitHub Actions Example

The published Docker images can be used directly in GitHub Actions workflows.

```yaml
- name: Pull Docker images
  run: |
    docker pull ghcr.io/evaristocuesta/clovance/clovance-api:${{ github.ref_name }}
    docker pull ghcr.io/evaristocuesta/clovance/clovance-frontend:${{ github.ref_name }}
```

---

## 📚 Related Documentation

- [Publishing Docker Images](DOCKER_PUBLISH.md)