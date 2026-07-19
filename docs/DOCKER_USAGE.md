# Ejemplos de Uso de Imágenes Docker

Este documento muestra cómo usar las imágenes Docker publicadas en GHCR.

## 🐳 Docker Compose

Crea un archivo `docker-compose.yml` para ejecutar toda la aplicación:

```yaml
services:
  postgres:
	image: postgres:16
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

Ejecutar:
```bash
docker compose up -d
```

## 🎯 Usar una Versión Específica

```bash
# Versiones de producción
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0  # Versión exacta
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0    # Major.minor
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1      # Major
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:latest  # Última versión estable

# Pre-releases (beta, rc, alpha)
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0-beta1
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v2.0.0-rc.2
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0-alpha
```

**Nota:** Las pre-releases NO actualizan los tags de producción (major, minor, latest).

## ☸️ Kubernetes

### Deployment del API

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: clovance-api
spec:
  replicas: 2
  selector:
	matchLabels:
	  app: clovance-api
  template:
	metadata:
	  labels:
		app: clovance-api
	spec:
	  containers:
	  - name: api
		image: ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0
		ports:
		- containerPort: 8080
		env:
		- name: ASPNETCORE_ENVIRONMENT
		  value: "Production"
		- name: ConnectionStrings__DefaultConnection
		  valueFrom:
			secretKeyRef:
			  name: clovance-secrets
			  key: postgres-connection
		- name: JWT__Secret
		  valueFrom:
			secretKeyRef:
			  name: clovance-secrets
			  key: jwt-secret
		livenessProbe:
		  httpGet:
			path: /health
			port: 8080
		  initialDelaySeconds: 30
		  periodSeconds: 10
		readinessProbe:
		  httpGet:
			path: /health
			port: 8080
		  initialDelaySeconds: 10
		  periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: clovance-api
spec:
  selector:
	app: clovance-api
  ports:
  - port: 80
	targetPort: 8080
  type: ClusterIP
```

### Deployment del Frontend

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: clovance-frontend
spec:
  replicas: 2
  selector:
	matchLabels:
	  app: clovance-frontend
  template:
	metadata:
	  labels:
		app: clovance-frontend
	spec:
	  containers:
	  - name: frontend
		image: ghcr.io/evaristocuesta/clovance/clovance-frontend:v1.0.0
		ports:
		- containerPort: 8000
		env:
		- name: API_URL
		  value: "http://clovance-api"
---
apiVersion: v1
kind: Service
metadata:
  name: clovance-frontend
spec:
  selector:
	app: clovance-frontend
  ports:
  - port: 80
	targetPort: 8000
  type: LoadBalancer
```

## 🔐 Autenticación con GHCR

Si las imágenes son privadas, necesitas autenticarte:

```bash
# Login con GitHub Personal Access Token
echo $GITHUB_TOKEN | docker login ghcr.io -u evaristocuesta --password-stdin

# O en Kubernetes, crear un secret
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=evaristocuesta \
  --docker-password=$GITHUB_TOKEN

# Usar el secret en el deployment
spec:
  imagePullSecrets:
  - name: ghcr-secret
```

## 🧪 Ejecutar Solo el API (para pruebas)

```bash
docker run -d \
  --name clovance-api \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Database=clovance;Username=postgres;Password=postgres" \
  ghcr.io/evaristocuesta/clovance/clovance-api:latest
```

## 🖥️ Ejecutar Solo el Frontend (para pruebas)

```bash
docker run -d \
  --name clovance-frontend \
  -p 8000:8000 \
  -e API_URL=http://localhost:8080 \
  ghcr.io/evaristocuesta/clovance/clovance-frontend:latest
```

## 🔍 Inspeccionar Imágenes

```bash
# Ver información de la imagen
docker inspect ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0

# Ver layers y tamaño
docker history ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0

# Listar todas las versiones disponibles
# (Requiere GitHub CLI instalado)
gh api \
  -H "Accept: application/vnd.github+json" \
  /users/evaristocuesta/packages/container/clovance%2Fclovance-api/versions
```

## 📊 Multi-arquitectura

Las imágenes se publican para múltiples arquitecturas:
- `linux/amd64` (x86_64)
- `linux/arm64` (Apple Silicon, ARM servers)

Docker automáticamente selecciona la imagen correcta para tu arquitectura.

## 🚀 CI/CD Integration

### GitHub Actions (ejemplo para deployment)

```yaml
- name: Deploy to production
  run: |
	kubectl set image deployment/clovance-api \
	  api=ghcr.io/evaristocuesta/clovance/clovance-api:${{ github.ref_name }}
	kubectl set image deployment/clovance-frontend \
	  frontend=ghcr.io/evaristocuesta/clovance/clovance-frontend:${{ github.ref_name }}
```

### GitLab CI (ejemplo)

```yaml
deploy:
  image: bitnami/kubectl:latest
  script:
	- kubectl set image deployment/clovance-api api=$CI_REGISTRY_IMAGE/clovance-api:$CI_COMMIT_TAG
```
