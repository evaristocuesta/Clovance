# Contributing to Clovance

Thank you for your interest in contributing to Clovance!

Whether you are reporting a bug, suggesting an improvement, or submitting code, your contributions are welcome.

## Reporting Issues

If you find a bug or have a feature request, please open a GitHub Issue and include as much detail as possible:

- Steps to reproduce the issue
- Expected behavior
- Actual behavior
- Environment details (OS, browser, .NET version, etc.)

## Development Setup

Clone the repository:

```bash
git clone https://github.com/evaristocuesta/clovance.git
cd clovance
```

Start the application:

```bash
cd api/aspire/Clovance.AppHost
dotnet run
```

## Coding Guidelines

Please follow the existing coding style and project structure.

### Backend

- Use C# and .NET 10 features where appropriate.
- Follow the existing Clean Architecture and Vertical Slice Architecture.
- Keep handlers focused on a single responsibility.
- Write unit tests for new functionality whenever possible.

### Frontend

- Follow Angular style guidelines.
- Use standalone components.
- Use signals where appropriate.
- Keep components small and reusable.

## Pull Requests

Before submitting a Pull Request, please ensure that:

- Your branch is up to date.
- The project builds successfully.
- Existing tests pass.
- New functionality includes tests when appropriate.
- Documentation has been updated if necessary.

Please keep Pull Requests focused on a single change.

## Code of Conduct

Be respectful and constructive when interacting with other contributors.

## License

By contributing to this project, you agree that your contributions will be licensed under the Apache License 2.0. See the [LICENSE](LICENSE.md) file for details.