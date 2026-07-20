# Version Publishing Examples

This document provides practical examples of how to publish different types of releases.

## 🚀 Common Scenarios

### Scenario 1: First Release

You're ready to release version 1.0.0 of your application:

```powershell
# Verify everything is committed
git status

# Create and publish the release
.\scripts\publish-version.ps1 -Version "1.0.0"
```

**Result:**

- Tags created: `v1.0.0`, `v1.0`, `v1`, `latest`
- Images published to GHCR: `clovance-api:1.0.0`, `clovance-frontend:1.0.0`

---

### Scenario 2: Bug Fix

You found a bug in production and need to release a patch:

```powershell
# Create the patch release
.\scripts\publish-version.ps1 -Version "1.0.1"
```

**Result:**

- Tags created: `v1.0.1`, `v1.0` (updated), `v1` (updated), `latest` (updated)
- Users using `v1.0` automatically receive the fix

---

### Scenario 3: New Backward-Compatible Feature

You added new functionality that is backward compatible:

```powershell
.\scripts\publish-version.ps1 -Version "1.1.0"
```

**Result:**

- Tags created: `v1.1.0`, `v1.1`, `v1` (updated), `latest` (updated)

---

### Scenario 4: Beta Testing

You want some users to test a new feature before the official release:

```powershell
# First beta
.\scripts\publish-version.ps1 -Version "2.0.0-beta.1"
```

**Result:**

- Tags created: `v2.0.0-beta.1` (**only** this tag)
- `latest`, `v2`, and `v2.0` are **not** updated (production remains on v1.x.x)

**Beta users can use:**

```yaml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:2.0.0-beta.1
```

---

### Scenario 5: Release Candidate

The beta was successful, so you create a release candidate:

```powershell
.\scripts\publish-version.ps1 -Version "2.0.0-rc.1"
```

After validating the RC, publish the final release:

```powershell
.\scripts\publish-version.ps1 -Version "2.0.0"
```

**Result:**

- `latest`, `v2`, and `v2.0` now point to the stable 2.0.0 release.
- This is a breaking release, so users must explicitly migrate.

---

### Scenario 6: Urgent Hotfix

A critical production bug needs to be fixed immediately:

```powershell
# Create a hotfix branch from the production tag
git checkout v1.1.0
git checkout -b hotfix/critical-bug

# Apply the fix
git add .
git commit -m "fix: critical security issue"

# Merge into main and create the release
git checkout main
git merge hotfix/critical-bug
git push

# Publish the hotfix
.\scripts\publish-version.ps1 -Version "1.1.1"
```

---

### Scenario 7: Alpha Release for Internal Development

A very early version intended only for the development team:

```powershell
.\scripts\publish-version.ps1 -Version "3.0.0-alpha"
```

**Internal usage:**

```bash
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:3.0.0-alpha
```

---

## 🔄 Complete Development Workflow

Example of a complete release cycle from alpha to production:

```powershell
# 1. Initial development
.\scripts\publish-version.ps1 -Version "2.0.0-alpha"

# 2. First public testing release
.\scripts\publish-version.ps1 -Version "2.0.0-beta.1"

# 3. Second beta with fixes
.\scripts\publish-version.ps1 -Version "2.0.0-beta.2"

# 4. Release candidate
.\scripts\publish-version.ps1 -Version "2.0.0-rc.1"

# 5. Final release
.\scripts\publish-version.ps1 -Version "2.0.0"

# 6. Hotfix if necessary
.\scripts\publish-version.ps1 -Version "2.0.1"

# 7. New backward-compatible feature
.\scripts\publish-version.ps1 -Version "2.1.0"
```

---

## 📊 Generated Tags Comparison

| Version | Generated Tags | Updates `latest`? | Recommended Use |
|---------|----------------|------------------|-----------------|
| `1.0.0` | `1.0.0`, `1.0`, `1`, `latest` | ✅ Yes | Production |
| `1.0.1` | `1.0.1`, `1.0`, `1`, `latest` | ✅ Yes | Hotfix |
| `1.1.0` | `1.1.0`, `1.1`, `1`, `latest` | ✅ Yes | New feature |
| `2.0.0-alpha` | `2.0.0-alpha` | ❌ No | Internal development |
| `2.0.0-beta.1` | `2.0.0-beta.1` | ❌ No | Public testing |
| `2.0.0-rc.1` | `2.0.0-rc.1` | ❌ No | Release candidate |
| `2.0.0` | `2.0.0`, `2.0`, `2`, `latest` | ✅ Yes | Production (breaking release) |

---

## ✅ Best Practices

### ✅ DO

- Use pre-releases for testing before major releases.
- Follow Semantic Versioning strictly.
- Document breaking changes in `v2.0.0`, `v3.0.0`, etc.
- Use a dry run first to verify the release:
  `.\scripts\publish-version.ps1 -Version "1.0.0" -DryRun`
- Commit all changes before creating a release.

### ❌ DON'T

- ❌ Don't use pre-releases in production.
- ❌ Don't skip version numbers (for example, from `1.0.0` to `1.3.0`).
- ❌ Don't introduce breaking changes in minor or patch releases.
- ❌ Don't reuse version numbers by deleting and recreating tags.
- ❌ Don't publish releases without proper testing.

---

## 🎯 Docker Image Examples

### Production (Always Stable)

```yaml
# docker-compose.yml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:latest
    # Or a specific version:
	# image: ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0
```

### Testing (Specific Pre-release)

```yaml
services:
  api:
	image: ghcr.io/evaristocuesta/clovance/clovance-api:2.0.0-beta.1
```

---

## 🔍 Troubleshooting

### Error: "The tag 'vX.Y.Z' already exists"

```powershell
# List existing tags
git tag -l

# If you really want to replace it (NOT RECOMMENDED):
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0

# Then publish it again
.\scripts\publish-version.ps1 -Version "1.0.0"
```

### View the GitHub Actions Workflow

```text
https://github.com/evaristocuesta/Clovance/actions
```

### Verify Published Images

```bash
# Pull a specific version
docker pull ghcr.io/evaristocuesta/clovance/clovance-api:1.0.0
```