<picture>
  <source media="(prefers-color-scheme: dark)" srcset="./docs/images/logo-full-light.svg">
  <img alt="Clovance" src="./docs/images/logo-full-dark.svg">
</picture>

# Clovance

Personal finance management application built with .NET Aspire, Angular, and PostgreSQL. Clovance helps you manage your daily accounting by tracking multiple accounts, recording expenses and income, and keeping a clear history of your financial movements. You can also register transfers between accounts, organize your personal finances in one place, and review statistics to better understand spending patterns and cash flow over time.

![Clovance](/docs/images/clovance-screenshot.jpg)

## 🚀 Quick Start

### Docker Compose

You can use Docker Compose instead of local tooling to run Clovance app. For complete setup options and commands, see [Docker Usage Guide](docs/DOCKER_USAGE.md).

### Local Development

```bash
# From the repository root
cd api/aspire/Clovance.AppHost
dotnet run
```

This will start:
- API Service
- Frontend
- PostgreSQL with pgWeb
- Aspire Dashboard

### Publish a New Version

```powershell
# Production release
.\scripts\publish-version.ps1 -Version "1.0.0"

# Pre-release (beta, rc, alpha)
.\scripts\publish-version.ps1 -Version "1.0.0-beta1"
```

This will create a Git tag and automatically trigger the GitHub Actions workflow, which will publish the Docker images to GitHub Container Registry.

## 📚 Documentation

- [Docker Image Publishing](docs/DOCKER_PUBLISH.md) - Complete guide for publishing and deployment

## 🏗️ Project Structure

```
Clovance/
├── api/                          # .NET backend
│   ├── aspire/                   # Aspire configuration
│   │   ├── Clovance.AppHost/     # Application orchestration
│   │   └── Clovance.ServiceDefaults/
│   ├── src/
│   │   └── Clovance.ApiService/  # REST API
│   └── tests/
│       ├── Clovance.UnitTests/
│       └── Clovance.IntegrationTests/
├── frontend/                     # Angular frontend
├── scripts/                      # Utility scripts
└── docs/                         # Documentation
```

## 🛠️ Technologies

**Backend:**
- .NET 10
- .NET Aspire
- PostgreSQL
- Entity Framework Core
- JWT Authentication

**Frontend:**
- Angular 22
- TypeScript
- Tailwind CSS
- Flowbite UI

**DevOps:**
- Docker
- GitHub Actions
- GitHub Container Registry (GHCR)

## 📦 Docker Images

Docker images are automatically published to:
- [https://ghcr.io/evaristocuesta/clovance/clovance-api](https://ghcr.io/evaristocuesta/clovance/clovance-api)
- [https://ghcr.io/evaristocuesta/clovance/clovance-frontend](https://ghcr.io/evaristocuesta/clovance/clovance-frontend)

## 🤝 Contributing

For contributions, go to the [Contributing Guide](CONTRIBUTING.md)

## 📄 License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE.md) file for details.