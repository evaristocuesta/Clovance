# Instructions for .NET 10 + Vertical Slice

## Architecture
- Vertical Slice Architecture
- Each feature is self-contained
- Minimal APIs with Carter or FastEndpoints
- MediatR for CQRS

## Slice Structure
Each operation (Create, Update, Delete, GetById) must have:
- Endpoint.cs (route and validation)
- Command/Query.cs (request)
- Handler.cs (business logic)
- Validator.cs (Fluent Validation)

## Naming Conventions
- Commands: `CreateUserCommand`, `UpdateUserCommand`
- Queries: `GetUserByIdQuery`, `GetUsersQuery`
- Handlers: `CreateUserCommandHandler`
- Endpoints: `CreateUserEndpoint`

## Entity Framework
- Configurations in separate files
- Do not use automatic migrations in Production
- Always use AsNoTracking() in queries

## Aspire
- Configure health checks
- Use service defaults
- Telemetry enabled by default