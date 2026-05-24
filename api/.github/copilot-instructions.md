# Instructions for .NET 10 + Vertical Slice

## Architecture
- Vertical Slice Architecture
- Each feature is self-contained
- Minimal APIs (no MediatR)
- Direct handler injection in endpoints

## Slice Structure
Each operation (Create, Update, Delete, GetById) must have:
- Endpoint.cs (implements IApiEndPoint interface)
- Command/Query.cs (request/response records)
- Handler.cs (business logic - plain classes with Handle method)
- Validator.cs (Fluent Validation)

## Endpoint Registration
- All endpoints implement `IApiEndPoint` interface with `MapApiEndpoints(IEndpointRouteBuilder app)` method
- Endpoints are automatically registered via `RegisterApiEndpointsFromAssembly(typeof(Program).Assembly)`
- Route groups are created automatically based on namespace convention:
  - `Features.Auth.*` → `/auth` group
  - `Features.Transactions.*` → `/transactions` group
- Do NOT manually create MapGroup in endpoints - it's automatic based on namespace
- Endpoints only map their specific route (e.g., `app.MapPost("/login", ...)`)

## Validation
- Use FluentValidation for command/query validation
- Apply validation using `.WithValidation<TCommand>()` extension on endpoint builders
- Validators are executed automatically via endpoint filter before handler execution

## Naming Conventions
- Commands: `CreateUserCommand`, `UpdateUserCommand`
- Queries: `GetUserByIdQuery`, `GetUsersQuery`
- Handlers: `CreateUserCommandHandler`
- Endpoints: `CreateUserEndpoint`

## Handler Pattern
- Handlers must implement `IHandler<TRequest, TResponse>` from `Features/Shared`
- Must expose `HandleAsync(TRequest command, CancellationToken cancellationToken)`
- Commands with no payload response should return `Unit`
- Register handlers via `AddHandlersFromAssembly(typeof(Program).Assembly)` (automatic scanning for `IHandler<,>` implementations)
- Inject handlers directly into endpoints

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
