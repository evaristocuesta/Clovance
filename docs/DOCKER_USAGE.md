# Using Clovance Docker Images

This document shows how to use the Docker images published to GitHub Container Registry (GHCR).

## 🐳 Docker Compose

Create a `docker-compose.yml` file to run the complete application:

```yaml
services:
  postgres:
    image: postgres:18.3
	environment:
	  POSTGRES_DB: clovance-database
	  POSTGRES_USER: postgres
	  POSTGRES_PASSWORD: your_secure_password
	volumes:
	  - postgres-data:/var/lib/postgresql/data
	ports:
	  - "5432:5432"
	healthcheck:
	  test: ["CMD-SHELL", "pg_isready -U postgres"]
	  interval: 10s
	  timeout: 5s
	  retries: 5

  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:latest
	environment:
	  ASPNETCORE_ENVIRONMENT: Production
	  ConnectionStrings__DefaultConnection: "Host=postgres;Database=clovance-database;Username=postgres;Password=your_secure_password"
	  JWT__Secret: "your-super-secret-jwt-key-change-this-in-production"
	  JWT__Issuer: "https://api.clovance.com"
	  JWT__Audience: "https://clovance.com"
	ports:
	  - "8080:8080"
	depends_on:
	  postgres:
		condition: service_healthy
	healthcheck:
	  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
	  interval: 30s
	  timeout: 10s
	  retries: 3

  frontend:
	image: ghcr.io/evaristocuesta/clovance/clovance-frontend:latest
	environment:
	  API_URL: http://api:8080
	ports:
	  - "8000:8000"
	depends_on:
	  - api

volumes:
  postgres-data:
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