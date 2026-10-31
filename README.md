<picture>
  <source media="(prefers-color-scheme: dark)" srcset="./docs/images/logo-full-light.svg">
  <img alt="Clovance" src="./docs/images/logo-full-dark.svg">
</picture>

# Clovance

Clovance is a personal finance management application built with .NET Aspire, Angular, and PostgreSQL. It helps you keep track of your daily finances and maintain a clear history of your financial transactions. Track multiple accounts, record expenses and income, register transfers between accounts, manage loans, and review statistics to better understand your spending patterns and cash flow over time. The interface is available in English and Spanish.

![Clovance](docs/images/clovance-screenshot-01.jpg)

![Clovance](docs/images/clovance-screenshot-02.jpg)

![Clovance](docs/images/clovance-screenshot-03.jpg)

## 🚀 Quick Start

### Easy install (recommended)

Install Clovance on Windows, Linux, or macOS with a single command. The installer checks Docker, asks a few questions, generates the configuration, pulls the images, and starts the app.

**Linux / macOS:**

```bash
curl -fsSL https://evaristocuesta.github.io/clovance/install/install.sh | sh
```

**Windows (PowerShell):**

```powershell
irm https://evaristocuesta.github.io/clovance/install/install.ps1 | iex
```

The scripts also support `uninstall` and `update` subcommands. See the [Installation Guide](docs/INSTALL.md) for all parameters and options. For manual setup, see the [Docker Usage Guide](docs/DOCKER_USAGE.md).

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

- [Installation Guide](docs/INSTALL.md) - One-command installers for Windows, Linux, and macOS
- [Docker Usage Guide](docs/DOCKER_USAGE.md) - Setup, ports, reverse proxy, and operations
- [Docker Image Publishing](docs/DOCKER_PUBLISH.md) - Publishing and deployment guide
- [Versioning Examples](docs/VERSIONING_EXAMPLES.md) - Semver tagging reference

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