# Ejemplos de Publicación de Versiones

Este documento muestra ejemplos prácticos de cómo publicar diferentes tipos de versiones.

## 🚀 Escenarios Comunes

### Escenario 1: Primera Release

Estás listo para lanzar la versión 1.0.0 de tu aplicación:

```powershell
# Verifica que todo esté commiteado
git status

# Crea y publica la versión
.\scripts\publish-version.ps1 -Version "1.0.0"
```

**Resultado:**
- Tags creados: `v1.0.0`, `v1.0`, `v1`, `latest`
- Imágenes en GHCR: `clovance-api:1.0.0`, `clovance-frontend:1.0.0`

---

### Escenario 2: Bug Fix

Encontraste un bug en producción y necesitas lanzar un patch:

```powershell
# Crea la versión de patch
.\scripts\publish-version.ps1 -Version "1.0.1"
```

**Resultado:**
- Tags creados: `v1.0.1`, `v1.0` (actualizado), `v1` (actualizado), `latest` (actualizado)
- Usuarios usando `v1.0` automáticamente obtendrán el fix

---

### Escenario 3: Nueva Feature (Compatible)

Agregaste nueva funcionalidad compatible con la versión anterior:

```powershell
.\scripts\publish-version.ps1 -Version "1.1.0"
```

**Resultado:**
- Tags creados: `v1.1.0`, `v1.1`, `v1` (actualizado), `latest` (actualizado)

---

### Escenario 4: Beta Testing

Quieres que algunos usuarios prueben una nueva feature antes del lanzamiento oficial:

```powershell
# Primera beta
.\scripts\publish-version.ps1 -Version "2.0.0-beta.1"
```

**Resultado:**
- Tags creados: `v2.0.0-beta.1` (SOLO este tag)
- `latest`, `v2`, `v2.0` NO se actualizan (producción sigue en v1.x.x)

**Los usuarios beta pueden usar:**
```yaml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:2.0.0-beta.1
```

---

### Escenario 5: Release Candidate

La beta fue exitosa, ahora creas un release candidate:

```powershell
.\scripts\publish-version.ps1 -Version "2.0.0-rc.1"
```

Después de validar el RC, lanzas la versión final:

```powershell
.\scripts\publish-version.ps1 -Version "2.0.0"
```

**Resultado:**
- Ahora `latest`, `v2`, `v2.0` apuntan a la versión estable 2.0.0
- Breaking change: los usuarios deben migrar explícitamente

---

### Escenario 6: Hotfix Urgente

Bug crítico en producción:

```powershell
# Crear branch de hotfix desde el tag de producción
git checkout v1.1.0
git checkout -b hotfix/critical-bug

# Hacer el fix
git add .
git commit -m "fix: critical security issue"

# Merge a main y crear tag
git checkout main
git merge hotfix/critical-bug
git push

# Publicar hotfix
.\scripts\publish-version.ps1 -Version "1.1.1"
```

---

### Escenario 7: Alpha para Desarrollo Interno

Versión muy temprana solo para el equipo de desarrollo:

```powershell
.\scripts\publish-version.ps1 -Version "3.0.0-alpha"
```

**Uso interno:**
```bash
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:3.0.0-alpha
```

---

## 🔄 Flujo Completo de Desarrollo

Ejemplo de ciclo completo desde alpha hasta producción:

```powershell
# 1. Desarrollo inicial
.\scripts\publish-version.ps1 -Version "2.0.0-alpha"

# 2. Primera versión de testing público
.\scripts\publish-version.ps1 -Version "2.0.0-beta.1"

# 3. Segunda beta con fixes
.\scripts\publish-version.ps1 -Version "2.0.0-beta.2"

# 4. Release candidate
.\scripts\publish-version.ps1 -Version "2.0.0-rc.1"

# 5. Release final
.\scripts\publish-version.ps1 -Version "2.0.0"

# 6. Hotfix si es necesario
.\scripts\publish-version.ps1 -Version "2.0.1"

# 7. Nueva feature compatible
.\scripts\publish-version.ps1 -Version "2.1.0"
```

---

## 📊 Comparación de Tags Generados

| Versión | Tags Creados | ¿Actualiza `latest`? | Uso Recomendado |
|---------|--------------|---------------------|-----------------|
| `1.0.0` | `1.0.0`, `1.0`, `1`, `latest` | ✅ Sí | Producción |
| `1.0.1` | `1.0.1`, `1.0`, `1`, `latest` | ✅ Sí | Hotfix |
| `1.1.0` | `1.1.0`, `1.1`, `1`, `latest` | ✅ Sí | Nueva feature |
| `2.0.0-alpha` | `2.0.0-alpha` | ❌ No | Desarrollo interno |
| `2.0.0-beta.1` | `2.0.0-beta.1` | ❌ No | Testing público |
| `2.0.0-rc.1` | `2.0.0-rc.1` | ❌ No | Release candidate |
| `2.0.0` | `2.0.0`, `2.0`, `2`, `latest` | ✅ Sí | Producción (breaking) |

---

## ✅ Mejores Prácticas

### ✅ DO

- **Usa pre-releases** para testing antes de lanzamientos importantes
- **Sigue semantic versioning** estrictamente
- **Documenta breaking changes** en v2.0.0, v3.0.0, etc.
- **Usa dry-run** primero para verificar: `.\scripts\publish-version.ps1 -Version "1.0.0" -DryRun`
- **Commitea todo** antes de crear una versión

### ❌ DON'T

- ❌ No uses pre-releases para producción
- ❌ No saltes versiones (de 1.0.0 a 1.3.0)
- ❌ No hagas breaking changes en minor/patch versions
- ❌ No reutilices números de versión (borrar y recrear tags)
- ❌ No publiques versiones sin testing previo

---

## 🎯 Ejemplos de Uso de Imágenes

### Producción (siempre estable)

```yaml
# docker-compose.yml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:latest
	# O específico:
	# image: ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0
```

### Testing (pre-release específica)

```yaml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:2.0.0-beta.1
```

### Kubernetes (versión pinned)

```yaml
spec:
  containers:
  - name: api
	image: ghcr.io/evaristocuesta/clovance/clovance-api:1.2.3
	# Nunca uses 'latest' en k8s producción
```

---

## 🔍 Troubleshooting

### Error: "El tag 'vX.Y.Z' ya existe"

```powershell
# Ver tags existentes
git tag -l

# Si realmente quieres reemplazar (NO RECOMENDADO):
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0
# Luego crear de nuevo
.\scripts\publish-version.ps1 -Version "1.0.0"
```

### Ver el workflow de GitHub Actions

```bash
# URL directa
https://github.com/evaristocuesta/Clovance/actions

# Desde CLI (requiere GitHub CLI)
gh run list --workflow=publish-images.yml
gh run watch
```

### Verificar imágenes publicadas

```bash
# Ver tus paquetes
https://github.com/evaristocuesta?tab=packages

# Pull de una versión específica
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0

# Ver tags disponibles (requiere token si es privado)
docker search ghcr.io/evaristocuesta/clovance
```
