# AGENTS.md

Personal finance app: .NET 10 + Aspire backend (`api/`), Angular 22 frontend (`frontend/`). En, es locales. Docker is required for local dev and integration tests.

## Running the app

- Start everything: `cd api/aspire/Clovance.AppHost && dotnet run` (API + frontend + PostgreSQL/pgWeb + Aspire dashboard; frontend on port 7000, API health at `/health`). Requires Docker running — the AppHost uses `AddDockerComposeEnvironment`.
- **Do not run `pnpm start` / `ng serve` standalone**: `frontend/proxy.conf.js` throws unless Aspire env vars `services__clovance-apiservice__http__0`/`https__0` exist, and `pnpm start` uses Windows `%PORT%` syntax that only works when launched by the AppHost.

## Commands

Backend (`api/Clovance.slnx` exists; CI restores/builds per project dir):
- Build all: `dotnet build api/Clovance.slnx`
- Unit tests: `dotnet test` in `api/tests/Clovance.UnitTests` (in-memory DB + NSubstitute; no Docker needed)
- Integration tests: `dotnet test` in `api/tests/Clovance.IntegrationTests` — **requires Docker** (Testcontainers spins up `postgres:18.3-alpine` per run). Uses env `Testing`: NoOp email sender, refresh-token cleanup disabled.
- Single test: `dotnet test --filter "FullyQualifiedName~SomeName"`
- `api/tests/Clovante.IntegrationTests2` is an orphan (only `obj/`, not in the solution) — ignore it.

Frontend (`cd frontend`; pnpm 11.8.0, Node 22):
- `pnpm install --frozen-lockfile`
- `pnpm build`
- `pnpm test --watch=false --providers-file=src/testing/ci-test.providers.ts` (this is the CI command; `pnpm test` alone runs watch mode, and the providers file stubs Transloco + CDK dialogs)
- Prettier is configured (`frontend/.prettierrc`: printWidth 100, singleQuote); format before committing.

## Backend conventions

Read `api/.github/copilot-instructions.md` — it is accurate and authoritative (vertical slice, minimal APIs, Result pattern). Key mechanics an agent gets wrong without reading it:

- **Central Package Management**: package versions ONLY in `api/Directory.Packages.props`; csproj `PackageReference` has no `Version`. `TreatWarningsAsErrors` is on — any warning fails the build.
- **Endpoint auto-registration**: endpoints implement `IApiEndPoint.MapApiEndpoints(...)` and are scanned from the assembly; route group comes from the namespace (`Features.Transactions.*` → `/transactions`). Never call `MapGroup` manually. Each operation = `Endpoint.cs` + `Command/Query.cs` + `Handler.cs` + `Validator.cs` in its own folder under `Features/<Area>/<Operation>/`.
- Handlers are plain classes implementing `IHandler<TReq, TRes>` with `HandleAsync`; registered by assembly scanning, injected directly into endpoints. No MediatR.
- Validation and logging are cross-cutting **decorators** (`api/src/Clovance.ApiService/Pipelines/`), applied via `.WithValidation<T>()` — not endpoint filters.
- EF Core code lives under `Infrastructure/Database`; queries must use `AsNoTracking()`. Migrations are auto-applied at startup (`MigrateAsync` in `Program.cs`); add new ones with `dotnet ef migrations add <Name>` from `api/src/Clovance.ApiService`.
- API responses return **stable error codes only** (see `Features/Shared/AppErrors.cs`); all user-facing text is translated in Angular. Never return hardcoded English messages from the API.
- `Program` is `public` partial-ish (referenced as `WebApplicationFactory<Program>`); JWT key comes from `Jwt:KeyFilePath`.

## Frontend conventions

Read `frontend/.github/copilot-instructions.md` — it is accurate. Non-obvious rules it enforces:
- Do NOT set `standalone: true` (default since v20); no `ngClass`/`ngStyle`; no `@HostBinding`/`@HostListener` decorators; no `*ngIf`/`*ngFor` (use `@if`/`@for`).
- `input()`/`output()` functions, `inject()`, signals (`set`/`update`, never `mutate`), `ChangeDetectionStrategy.OnPush`, `providedIn: 'root'` services.
- Path aliases: `@core/*`, `@layouts/*`, `@features/*`, `@shared/*`.
- Translations via Transloco; files in `frontend/public/i18n/` (en, es). UI = Tailwind 4 + Flowbite.
- Prod build enforces budgets: any component style > 8kB **errors** the build — keep styles in Tailwind classes, not big CSS blocks.

## CI & releases

- Two independent workflows with path filters: `.github/workflows/dotnet.yml` (`api/**`) and `angular.yml` (`frontend/**`), each with a `-skip` no-op counterpart. A PR needs both required checks green, so don't break the other stack.
- Releases: tag via `scripts/publish-version.ps1 -Version X.Y.Z` (PowerShell; validates semver, `-DryRun` available) → triggers `publish-images.yml` → pushes to GHCR. See `docs/DOCKER_PUBLISH.md` and `docs/DOCKER_USAGE.md`.
