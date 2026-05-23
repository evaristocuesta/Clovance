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
- Place all EF Core-related code under `Infrastructure/Database`
- Configurations in separate files
- Do not use automatic migrations in Production
- Always use AsNoTracking() in queries
- Identity service registration (AddIdentityCore and AddEntityFrameworkStores) should live in the AddDatabase extension, not duplicated in Program.cs.

## Transaction Description Search
- For transaction description search, use case-insensitive free-text contains semantics (not prefix-only).

## Aspire
- Configure health checks
- Use service defaults
- Telemetry enabled by default