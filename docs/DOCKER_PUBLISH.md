# Publicación de Imágenes Docker con GitHub Actions

Este documento explica cómo funciona la publicación automática de imágenes Docker a GitHub Container Registry (GHCR).

## 🚀 Proceso de Publicación

### Automático con Script (Recomendado)

Usa el script de PowerShell incluido para crear y publicar versiones de forma segura:

```powershell
# Desde la raíz del repositorio

# Versión de producción
.\scripts\publish-version.ps1 -Version "1.0.0"

# Pre-release (beta, rc, alpha)
.\scripts\publish-version.ps1 -Version "1.0.0-beta1"
.\scripts\publish-version.ps1 -Version "2.0.0-rc.2"
.\scripts\publish-version.ps1 -Version "1.0.0-alpha"

# Para ver qué haría sin ejecutarlo (dry run)
.\scripts\publish-version.ps1 -Version "1.0.0" -DryRun
```

El script verificará automáticamente:
- ✅ Formato de versión correcto (X.Y.Z o X.Y.Z-prerelease)
- ✅ No hay cambios sin commitear
- ✅ Estás sincronizado con el remoto
- ✅ El tag no existe ya

### Manual

También puedes crear tags manualmente:

```bash
# Crear un tag de versión de producción
git tag v1.0.0

# Crear un tag de pre-release
git tag v1.0.0-beta1

# Subir el tag a GitHub
git push origin v1.0.0
# o
git push origin v1.0.0-beta1
```

### Qué ocurre automáticamente

Esto disparará el workflow de GitHub Actions que:
1. Construirá las imágenes Docker para API y Frontend
2. Las etiquetará con múltiples tags:
   - `v1.0.0` (versión completa)
   - `v1.0` (major.minor)
   - `v1` (major)
   - `latest` (si es la rama por defecto)
   - `main-<sha>` (SHA del commit)
3. Las publicará en `ghcr.io/evaristocuesta/clovance/`
4. Generará attestation de provenance

### Imágenes Publicadas

- **API**: `ghcr.io/evaristocuesta/clovance/clovance-api:v1.0.0`
- **Frontend**: `ghcr.io/evaristocuesta/clovance/clovance-frontend:v1.0.0`

## 📦 Estructura de Tags

El workflow genera automáticamente múltiples tags por cada versión:

### Versiones de Producción

```
v1.2.3 → Genera los siguientes tags:
  - v1.2.3    (versión completa)
  - v1.2      (major.minor)
  - v1        (major)
  - latest    (solo si es default branch)
  - main-abc123def (SHA corto del commit)
```

### Pre-releases (beta, rc, alpha)

```
v1.2.3-beta1 → Genera solo:
  - v1.2.3-beta1  (versión completa)
  - main-abc123def (SHA corto del commit)

⚠️ Las pre-releases NO actualizan:
  - major.minor (v1.2)
  - major (v1)
  - latest
```

Esto asegura que las versiones de prueba no sobrescriban las versiones estables en producción.

## 🔧 Configuración Local con Aspire

### Desarrollo Local

Para ejecutar localmente con Aspire:

```bash
cd api/aspire/Clovance.AppHost
dotnet run
```

Esto utilizará `appsettings.Development.json` y NO intentará publicar contenedores.

### Generar Manifests para Deployment

Aspire puede generar manifests de deployment (Kubernetes, Docker Compose, etc.) que referenciarán las imágenes publicadas en GHCR:

```bash
# Generar manifests
dotnet run --project api/aspire/Clovance.AppHost \
  -- \
  --publisher manifest \
  --output-path ./manifests
```

Esto generará archivos de deployment listos para usar con las imágenes de GHCR.

## 🐳 Probar Construcción Local

### API Service

```bash
# Desde la raíz del repositorio
docker build -t clovance-api:local -f api/src/Clovance.ApiService/Dockerfile .
docker run -p 8080:8080 clovance-api:local
```

### Frontend

```bash
# Desde el directorio frontend
cd frontend
docker build -t clovance-frontend:local .
docker run -p 8000:8000 clovance-frontend:local
```

## 🔐 Permisos de GHCR

Las imágenes se publican como públicas por defecto. Para cambiar la visibilidad:

1. Ve a https://github.com/evaristocuesta?tab=packages
2. Encuentra el paquete (clovance-api o clovance-frontend)
3. Ve a "Package settings"
4. Cambia la visibilidad a "Public" o "Private"

## 📋 Requisitos del Workflow

El workflow requiere los siguientes permisos (ya configurados):
- `contents: read` - Para clonar el repositorio
- `packages: write` - Para publicar en GHCR

Usa automáticamente `${{ secrets.GITHUB_TOKEN }}` que se genera automáticamente en cada workflow.

## 🔄 Versionado Semántico

Se recomienda seguir [Semantic Versioning](https://semver.org/):

- **MAJOR** (v1.0.0 → v2.0.0): Cambios incompatibles en la API
- **MINOR** (v1.0.0 → v1.1.0): Nueva funcionalidad compatible
- **PATCH** (v1.0.0 → v1.0.1): Correcciones de bugs
- **PRE-RELEASE** (v1.0.0-beta1): Versiones de prueba antes del lanzamiento oficial

### Ejemplos de Versionado

```bash
# Desarrollo y testing
git tag v1.0.0-alpha && git push origin v1.0.0-alpha    # Versión muy temprana
git tag v1.0.0-beta.1 && git push origin v1.0.0-beta.1  # Primera beta
git tag v1.0.0-beta.2 && git push origin v1.0.0-beta.2  # Segunda beta
git tag v1.0.0-rc.1 && git push origin v1.0.0-rc.1      # Release candidate

# Producción
git tag v1.0.0 && git push origin v1.0.0  # Release inicial
git tag v1.0.1 && git push origin v1.0.1  # Bug fix
git tag v1.1.0 && git push origin v1.1.0  # Nueva feature
git tag v2.0.0 && git push origin v2.0.0  # Breaking change
```

## 🚨 Troubleshooting

### ¿Cuándo usar pre-releases?

Usa pre-releases para:
- ✅ **Testing** de nuevas features antes del lanzamiento
- ✅ **Betas públicas** para usuarios que quieran probar versiones anticipadas
- ✅ **Release candidates** antes de una versión final
- ✅ **Versiones alpha** para desarrollo muy temprano

**NO uses pre-releases para:**
- ❌ Versiones de producción estables
- ❌ Hotfixes urgentes (usa versiones de producción)

### Usar una pre-release en producción

Si necesitas usar una pre-release específica:

```yaml
# docker-compose.yml
services:
  api:
    image: ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0-beta1
```

```bash
# Docker pull
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0-beta1
```

### El workflow falla con "permission denied"

Verifica que el repositorio tenga activado "Read and write permissions" para workflows:
1. Settings → Actions → General
2. Workflow permissions → "Read and write permissions"

### Las imágenes no se ven en GHCR

Pueden tardar unos minutos en aparecer. Verifica:
1. El workflow completó exitosamente: https://github.com/evaristocuesta/Clovance/actions
2. Los paquetes en: https://github.com/evaristocuesta?tab=packages

### Quiero cambiar el registry

Edita `.github/workflows/publish-images.yml` y cambia:
```yaml
env:
  REGISTRY: ghcr.io  # Cambiar a docker.io, quay.io, etc.
```

También actualiza `appsettings.Production.json`:
```json
{
  "Parameters": {
	"container-registry": "docker.io/tu-usuario/clovance"
  }
}
```

## 📚 Referencias

- [GitHub Container Registry Docs](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [.NET Aspire Deployment](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/overview)
- [Docker Build Push Action](https://github.com/marketplace/actions/build-and-push-docker-images)
