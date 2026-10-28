# LocalEats

LocalEats is a C# food-ordering platform built with Blazor, ASP.NET Core, and PostgreSQL.

## Current status

TASK-001 through TASK-003 are complete. TASK-004 adds shared analyzer settings, centralized package versions and a GitHub Actions build/test workflow. Identity persistence and development roles are implemented in TASK-005. Registration is implemented at `/register`. Login/logout is implemented at /login and /logout; role policies and central order ownership checks are implemented in TASK-008. The account page is implemented at /account. TASK-010 adds the responsive shared application shell. Basic persistence entities exist; restaurant and ordering features are still pending.

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
Host=127.0.0.1
Port=15432
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
TASK-018 - Build restaurant detail and menu page
```


## Database foundation (TASK-003)

Restore the repository-local EF tool before using migrations:

```powershell
dotnet tool restore
$env:ConnectionStrings__DefaultConnection = 'Host=127.0.0.1;Port=15432;Database=localeats;Username=localeats;Password=localeats_dev_password'
dotnet tool run dotnet-ef database update --project src/LocalEats.Infrastructure --startup-project src/LocalEats.Infrastructure
```

In Development, starting either host applies pending migrations and inserts missing sample restaurants with categories/items by sample slug. Existing records and edits are preserved; sample insertion is serialized with a PostgreSQL transaction lock. Production does not run this startup seeding/migration path. Set `ConnectionStrings__DefaultConnection` to override the database connection. No PostgreSQL tables need manual creation.

The first migration includes basic restaurant/menu/order/audit persistence. Later feature tasks still add their rules, required fields, ownership, and application flows. Identity has not started.

## CI status

[![Build and test](https://github.com/yasinchowdhuryxaf/LocalEats/actions/workflows/ci.yml/badge.svg)](https://github.com/yasinchowdhuryxaf/LocalEats/actions/workflows/ci.yml)

GitHub remote: `yasinchowdhuryxaf/LocalEats` (private). The workflow runs on pushes and pull requests; the badge links to the actual hosted status.

Shared build/analyzer settings are in `Directory.Build.props`; dependency versions are in `Directory.Packages.props`. See `docs/TEST_PLAN.md` for the Release verification commands and test naming convention.

## Portable copy after each completed task

The working source is this workspace. After each successful task, refresh `C:\Users\acer\Desktop\MY_PROJECT` with source, documentation, configuration, migrations, local tool manifest and CI files. Exclude `.git`, `bin`, `obj`, `work`, `outputs`, `TestResults`, `.env`, user secrets and machine-specific data. Compare copied file hashes before reporting success. Copying source does not copy Docker database volumes; a new computer recreates the sample database through migrations and development seeding.

## Registration (TASK-006)

Open `http://localhost:5015/register`. A valid email, a 12–128 character password meeting Identity complexity rules, and matching confirmation create a Customer account. Registration does not sign the user in. Existing/duplicate accounts receive a safe error. Passwords are never logged or stored as plain text. Customer role provisioning must be completed in production before registration is enabled (development startup already seeds it).

The full test suite now needs a separate local PostgreSQL database. Create it once after starting Docker:

```powershell
docker compose exec postgres createdb -U localeats localeats_registration_tests
```

If it already exists, retain it. See `docs/TEST_PLAN.md` for `LOCALEATS_TEST_CONNECTION` and the full verification procedure. CI creates its own temporary PostgreSQL service.


## Sign-in and sign-out (TASK-007)

Open `/login` after registering. Successful sign-in opens `/session` (a protected status page). `/logout` shows confirmation and submits a protected POST to sign out. Invalid credentials and lockout use the same generic message. Sessions use an eight-hour sliding cookie; the remember option persists it across browser restarts. Production cookies require HTTPS. Role policies, account pages and dashboards follow in later tasks. See `docs/SECURITY.md` for the current scope and deployment requirements.


## Authorization (TASK-008)

Customer, Manager and Admin entry pages require their explicit signed-in roles on the server. `/session` shows the relevant entry links. Wrong-role direct requests go to `/access-denied`; anonymous requests go to login. No privileged account is seeded and registration cannot choose roles. See `docs/PERMISSIONS.md` for the exact access matrix and the central order ownership service. Manager restaurant assignments and actual dashboards/order pages remain their scheduled feature tasks.


## Account/profile (TASK-009)

After signing in, open `/account` or select Your account from `/session`. Email is readonly. Name and phone are optional (100 and 32 character limits); phone syntax is validated without imposing a country or verifying the number. Changes affect only the signed-in user. A stale edit requires reload rather than overwriting newer changes. The display-name field is created by the included migration; development startup applies it automatically. No email/password-change or account-deletion flow is included in this task.

## Customer dashboard (TASK-011)

Customers land at /dashboard after default login. It shows a welcome, account actions and an order-history placeholder. Browse restaurants opens the public directory; menus, ordering and order history remain later tasks. Explicit safe local login return URLs remain honored.

## Manager dashboard (TASK-012)

Manager-role users can open /manager. Restaurant setup, menu and orders links lead to protected coming-soon pages. Assignment status and pending counts are not connected yet; real restaurant ownership remains TASK-025. No default Manager account or public role-grant endpoint exists.

## Admin dashboard (TASK-013)

Admin-role users can open /admin with links to protected restaurant/user management placeholders. Pending restaurant counts are not connected. Actual approval and user administration remain TASK-033/034; no default Admin account, role-grant endpoint or management mutation is introduced.

Dashboard integration coverage (TASK-014) verifies role access, logout challenges, placeholder states, safe missing-profile feedback and two-customer greeting isolation. Real order/restaurant data coverage follows the corresponding feature tasks.

LocalEats PostgreSQL is published only at 127.0.0.1:15432 (container port 5432). This avoids interfering with separately installed PostgreSQL or other projects. Only manage the localeats Compose project; never stop unrelated containers/services.

EF tooling requires an explicitly configured 127.0.0.1:15432/localeats endpoint and refuses missing or unrelated targets before connecting. This local tooling intentionally does not perform production migrations; a production deployment workflow must be approved separately.

## Restaurant directory (TASK-017)

Open http://127.0.0.1:5015/restaurants. Search by name, filter by exact area/cuisine, and page through six results at a time. Only approved, active, non-deleted restaurants are public; closed ones remain browseable. Menu pages are TASK-018. Sample cards use decorative placeholders until image storage is decided.
