# LocalEats Project - Start Here

This folder is a portable copy of the LocalEats project. Open this folder directly in VS Code on another Windows computer.

## What is included

- `LocalEats.slnx` - .NET solution.
- `src/` - Blazor Web, ASP.NET Core API, Application, Domain, Infrastructure, and Contracts projects.
- `tests/` - test projects.
- `docs/` - PRD, architecture, design, rules, tasks, and project memory.
- `AGENTS.md` - instructions for Codex.
- `docker-compose.yml` - local PostgreSQL service.
- `.env.example` - documented environment variables.
- `README.md` - normal setup instructions.

## First setup on another computer

1. Install the .NET SDK version shown in `global.json`.
2. Install Docker Desktop and start Docker Engine.
3. Install VS Code and the C# Dev Kit extension.
4. Open this folder in VS Code.
5. Open the terminal at this folder.
6. Start PostgreSQL:

```powershell
docker compose up -d postgres
docker compose ps
```

7. Restore and verify the solution:

```powershell
dotnet tool restore
dotnet restore LocalEats.slnx
dotnet build LocalEats.slnx
docker compose exec postgres createdb -U localeats localeats_registration_tests
dotnet test LocalEats.slnx
```

8. Run the web application:

```powershell
dotnet run --project src/LocalEats.Web
```

9. Open `http://localhost:5015`.
10. Check `http://localhost:5015/health`.

## Continue the project with Codex

Before changing code, ask Codex to read:

- `AGENTS.md`
- `docs/PRD.md`
- `docs/ARCHITECTURE.md`
- `docs/DESIGN.md`
- `docs/RULES.md`
- `docs/TASKS.md`
- `docs/MEMORY.md`

The next task is:

```text
TASK-010 - Create the application shell
```

TASK-001 through TASK-004 are implemented. Database migrations, development sample data and build/CI configuration are included. TASK-005 added Identity accounts and roles. TASK-006 added registration at /register. TASK-007 added login/logout. TASK-008 added role/resource authorization. TASK-009 added the account page. TASK-010 adds the application shell.

Use this first Codex prompt:

```text
Read AGENTS.md and all files inside docs/.

We are starting TASK-010 from docs/TASKS.md.
First inspect the current solution and provide a plan.
Do not edit files until the plan is clear.
```

## Important sharing rules

- Do not share real passwords or production secrets.
- `.env.example` contains development placeholders only.
- Keep `.env` and user secrets on each computer separately.
- Generated `bin/`, `obj/`, and local database data are not part of the source project.

Database endpoint: 127.0.0.1:15432. Use the LocalEats Compose service; do not change or stop other projects' databases. Internal container PostgreSQL remains port 5432.

EF tools require the explicit LocalEats ConnectionStrings__DefaultConnection shown in README.md. Missing or unrelated endpoints are refused before connecting. See docs/SEED_DATA.md for repeatable development-only sample behavior.

Current milestone: TASK-001 through TASK-019 complete. Next is TASK-020 basket page. Existing sample data is preserved; a fresh development database receives the catalog described in docs/SEED_DATA.md.
