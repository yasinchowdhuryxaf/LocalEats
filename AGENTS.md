# LocalEats Project Instructions

Before changing code, read:

- docs/PRD.md
- docs/ARCHITECTURE.md
- docs/DESIGN.md
- docs/RULES.md
- docs/TASKS.md
- docs/MEMORY.md

## Approved stack

- C#
- ASP.NET Core
- Blazor Web App
- PostgreSQL
- Entity Framework Core with Npgsql
- ASP.NET Core Identity
- xUnit and Playwright

## Working rules

- Implement one task from `docs/TASKS.md` at a time.
- Plan before editing.
- Do not modify unrelated files.
- Keep business logic in Application and Domain layers.
- Keep EF Core and provider code in Infrastructure.
- Do not access the database directly from Blazor components.
- Validate and authorize on the server.
- Add tests for behavior changes.
- Use migrations for every schema change.
- Never commit secrets.
- Do not add dependencies without explaining why.
- Run formatting, build, and tests before declaring a task complete.
- Update `docs/TASKS.md` and `docs/MEMORY.md` after each completed task.
- After successful verification of each task, refresh `C:\Users\acer\Desktop\MY_PROJECT` with the portable source, documentation, migrations, local tool manifest, and CI configuration. Exclude generated outputs, `.git`, secrets, and database volumes; verify copied file hashes. Record sync status in `docs/MEMORY.md`.

## Required verification

```text
dotnet format --verify-no-changes
dotnet build
dotnet test
```

When reporting completion, list changed files, tests run, assumptions, skipped work, and remaining issues.

## Project isolation

- Access and modify only this LocalEats workspace and the authorized Desktop MY_PROJECT portable copy.
- Docker operations must target only the localeats Compose project or localeats-postgres container.
- Never access or modify ARAM PROJECT files, containers, services or database.
- Do not stop or reconfigure other PostgreSQL services. LocalEats uses 127.0.0.1:15432.

## Visible progress

After each finished UI slice, run the LocalEats localhost preview, show the changed page to the user, and explain what changed. Leave the final preview tab available. For backend-only work, explain the behavior and verification rather than inventing visible features.
