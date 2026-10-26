# LocalEats

LocalEats is a C# food-ordering platform built with Blazor, ASP.NET Core, and PostgreSQL.

## Current status

TASK-001 through TASK-003 are complete. TASK-004 adds shared analyzer settings, centralized package versions and a GitHub Actions build/test workflow. Identity is next in TASK-005. Basic persistence entities exist; restaurant and ordering features are still pending.

## Technology

- .NET 10 SDK, pinned in `global.json`
- Blazor Web App
- ASP.NET Core API
- PostgreSQL
- Entity Framework Core with Npgsql (database wiring begins in TASK-003)
- ASP.NET Core Identity (authentication begins after the database foundation)

## Repository layout

```text
src/       Application projects
tests/     Unit, integration, API, and E2E test projects
docs/      Product, architecture, design, rules, tasks, and memory
```

Read these files before changing code:

- `AGENTS.md`
- `docs/PRD.md`
- `docs/ARCHITECTURE.md`
- `docs/DESIGN.md`
- `docs/RULES.md`
- `docs/TASKS.md`
- `docs/MEMORY.md`

## Prerequisites

Install:

- .NET SDK version specified in `global.json`.
- Docker Desktop with Docker Compose support.
- Git.
- VS Code with C# Dev Kit.

Verify the SDK:

```powershell
dotnet --version
```

## Start local PostgreSQL

From the repository root:

```powershell
docker compose up -d postgres
```

Check the container:

```powershell
docker compose ps
```

The local database is available at:

```text
Host=localhost
Port=5432
Database=localeats
Username=localeats
Password=localeats_dev_password
```

Stop the database when finished:

```powershell
docker compose down
```

To remove the local database volume as well:

```powershell
docker compose down -v
```

The volume removal command deletes local development data.

## Run the application

Restore and build:

```powershell
dotnet restore LocalEats.slnx
dotnet build LocalEats.slnx
```

Run the Blazor web application:

```powershell
dotnet run --project src/LocalEats.Web
```

Open:

```text
http://localhost:5015
```

The web health endpoint is:

```text
http://localhost:5015/health
```

Run the API separately when working on API endpoints:

```powershell
dotnet run --project src/LocalEats.Api
```

The API health endpoint is:

```text
http://localhost:5253/health
```

## Verify the project

```powershell
dotnet format LocalEats.slnx --verify-no-changes --no-restore
dotnet build LocalEats.slnx --no-restore
dotnet test LocalEats.slnx --no-restore
```

At the scaffold stage, the Domain test project contains the initial smoke test. Application, API, and E2E tests are added with their feature tasks.

## Configuration

Local development configuration is stored in:

- `src/LocalEats.Web/appsettings.Development.json`
- `src/LocalEats.Api/appsettings.Development.json`
- `.env.example` for documented environment variables

Never commit real credentials. Use environment variables or local user secrets for sensitive values.

## Next task

See `docs/TASKS.md` for the ordered implementation plan. The next task is:

```text
TASK-005 - Add application user and Identity
```


## Database foundation (TASK-003)

Restore the repository-local EF tool before using migrations:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/LocalEats.Infrastructure --startup-project src/LocalEats.Api
```

In Development, starting either host applies pending migrations and seeds two sample restaurants with categories/items when the restaurant table is empty. Start one host first; this simple seed routine is intended for a single local developer. Production does not run this startup seeding/migration path. Set `ConnectionStrings__DefaultConnection` to override the database connection. No PostgreSQL tables need manual creation.

The first migration includes basic restaurant/menu/order/audit persistence. Later feature tasks still add their rules, required fields, ownership, and application flows. Identity has not started.

## CI status

[![Build and test](https://github.com/yasinchowdhuryxaf/LocalEats/actions/workflows/ci.yml/badge.svg)](https://github.com/yasinchowdhuryxaf/LocalEats/actions/workflows/ci.yml)

GitHub remote: `yasinchowdhuryxaf/LocalEats` (private). The workflow runs on pushes and pull requests; the badge links to the actual hosted status.

Shared build/analyzer settings are in `Directory.Build.props`; dependency versions are in `Directory.Packages.props`. See `docs/TEST_PLAN.md` for the Release verification commands and test naming convention.

## Portable copy after each completed task

The working source is this workspace. After each successful task, refresh `C:\Users\acer\Desktop\MY_PROJECT` with source, documentation, configuration, migrations, local tool manifest and CI files. Exclude `.git`, `bin`, `obj`, `work`, `outputs`, `TestResults`, `.env`, user secrets and machine-specific data. Compare copied file hashes before reporting success. Copying source does not copy Docker database volumes; a new computer recreates the sample database through migrations and development seeding.

