# Project Memory
# Project: LocalEats

This file records the current project state for the developer and OpenAI Codex. It is a working memory map, not a permanent decision log. Permanent technical decisions belong in `docs/decisions/`.

## 1. Current Status

### Overall status

**TASK-005 Identity foundation implemented. GitHub CI is connected and passing. TASK-006 registration is next.** The product definition, architecture, design system, project rules, and implementation task plan are prepared. The .NET solution and project structure have been scaffolded, restored, built, tested, and format-checked.

### Product

LocalEats is a light food-ordering web application inspired by the basic customer journey of foodpanda:

```text
Browse restaurants
→ View menu
→ Add items to basket
→ Checkout
→ Place order
→ View order status
```

The three primary actor areas are:

- Customer.
- Restaurant Manager.
- Platform Administrator.

### Approved direction

- Language: C#.
- Frontend: Blazor Web App.
- Backend: ASP.NET Core.
- Database: PostgreSQL.
- ORM: Entity Framework Core with Npgsql.
- Authentication: ASP.NET Core Identity or an approved managed provider.
- Architecture: Modular monolith.
- Testing: xUnit, integration tests with PostgreSQL, and Playwright for critical flows.

### MVP direction

The MVP includes:

- Registration, login, logout, and protected roles.
- Restaurant directory and menu browsing.
- Basket and checkout.
- Cash-on-delivery or pay-at-pickup order creation.
- Customer order history.
- Restaurant manager menu and order management.
- Admin restaurant approval and basic administration.
- Validation, authorization, migrations, tests, CI, preview deployment, and production QA.

Online payment, delivery rider tracking, reviews, loyalty, native mobile apps, and advanced analytics are deferred until the core flow is stable.

## 2. Documentation Completed by AI

The following planning documents have been created:

- `PRD.md` - product overview, problem, goals, users, MVP features, rules, scope, and success criteria.
- `ARCHITECTURE.md` - high-level workflow, technology stack, solution structure, dependencies, and integration flow.
- `RULES.md` - general principles, C# standards, error handling, project boundaries, security rules, testing rules, and Codex procedure.
- `DESIGN.md` - design principles, color palette, typography, spacing, components, responsive behavior, and accessibility requirements.
- `TASKS.md` - ordered implementation plan from project setup through authentication, dashboards, ordering, payments, testing, and deployment.
- `MEMORY.md` - this current project-state document.

These documents should be copied into the repository under `docs/` where appropriate, with `AGENTS.md` at the repository root.

## 3. Completed Tasks

### Planning tasks completed

- [x] Defined the product name: LocalEats.
- [x] Defined the Customer, Restaurant Manager, and Admin actors.
- [x] Defined the light MVP scope.
- [x] Defined the core customer ordering journey.
- [x] Defined manager and admin responsibilities.
- [x] Selected C# as the project language.
- [x] Selected Blazor Web App for the frontend.
- [x] Selected ASP.NET Core for the backend.
- [x] Selected PostgreSQL for the database.
- [x] Selected Entity Framework Core and Npgsql for data access.
- [x] Selected a modular monolith architecture.
- [x] Defined the solution folder structure.
- [x] Defined project dependency constraints.
- [x] Defined initial database entities.
- [x] Defined authentication roles and authorization direction.
- [x] Defined order statuses and valid status transitions.
- [x] Defined the visual design system.
- [x] Defined the implementation phases and task IDs.
- [x] Defined the payment gateway as a later phase after the core order flow.

### Code tasks completed

- [x] Created `LocalEats.slnx`.
- [x] Created Web, Api, Application, Domain, Infrastructure, and Contracts projects.
- [x] Created Domain, Application, API, and E2E test projects.
- [x] Added project references following the architecture.
- [x] Initialized Git and added a .NET-oriented `.gitignore`.
- [x] Added `global.json` for the installed .NET SDK.
- [x] InitialDatabaseFoundation migration created and applied on 2026-10-21; recheck pending after Docker starts.
- [x] Identity persistence, middleware, development roles and migration implemented; registration/login UI remain upcoming.
- [ ] No UI pages have been implemented yet.
- [ ] No CI pipeline has been implemented yet.

## 4. In Progress

### Current phase

**Phase 1 - Project Setup**

### Current task

**TASK-006 - Build registration (not started)**

### Current task objective

Build registration in TASK-006 using the Identity managers configured in TASK-005. No registration page has been implemented yet.

### Current status

TASK-001 through TASK-005 complete. TASK-006 is next. See the latest activity entries for verification and portable-copy results.

## 5. Next Tasks

1. TASK-006 - Build registration.
2. TASK-007 - Build login and logout.
3. TASK-008 - Add role and resource authorization.
4. TASK-009 - Add account page.
5. TASK-010 - Create the application shell.

## 6. Known Decisions

These decisions are currently approved for the MVP direction:

- Use a modular monolith rather than microservices.
- Use C# across frontend and backend.
- Use Blazor instead of a separate JavaScript frontend.
- Use PostgreSQL as the relational database.
- Use EF Core migrations for all schema changes.
- Use server-side validation and authorization.
- Use integer minor units for prices.
- Keep payment out of the first ordering slice.
- Keep delivery logistics out of the MVP.
- Use small tasks and vertical slices.

If a decision changes, create an ADR and update this file.

## 7. Open Decisions

The following decisions must be answered before the relevant implementation task:

- [ ] Exact .NET SDK version.
- [ ] Delivery, pickup, or both for the first release.
- [ ] Initial city or service area.
- [ ] Whether customers can browse before login.
- [ ] Whether login is required before adding items or only before checkout.
- [ ] Restaurant approval workflow details.
- [ ] Cancellation policy.
- [ ] Payment provider and settlement model.
- [ ] Image/file storage provider.
- [ ] Hosting provider and monthly budget.
- [ ] Email provider.
- [ ] Production privacy and retention requirements.

Do not silently guess a business policy when one of these decisions affects data, payment, permissions, or user experience.

## 8. Known Issues and Gaps

- The solution exists and builds; dependency security advisories remain visible.
- Documentation is present under docs/.
- Local PostgreSQL migration, seed data and health endpoints verified on 2026-10-27.
- No test database setup exists yet.
- No preset development user accounts are created; development roles exist.
- No design tokens have been implemented in CSS or Blazor components.
- No hosting provider has been selected.
- No payment provider has been selected.

## 9. AI Working Memory Rules

When Codex starts a new session:

1. Read this file first.
2. Read `AGENTS.md` and the relevant planning documents.
3. Check the current task and its dependencies in `TASKS.md`.
4. Inspect the repository before proposing changes.
5. Do not repeat completed planning work.
6. Do not claim a documentation decision is implemented in code.
7. Update this file only with verified project state.
8. Record permanent decisions in `docs/decisions/`.
9. Record current issues and next steps here.
10. Report assumptions, skipped work, and test results at the end of each task.

## 10. Task Completion Record

When a task is completed, add an entry using this format:

```text
### TASK-XXX - Task name

Completed on: YYYY-MM-DD
Branch: feature/TASK-XXX-short-name
Summary: What was implemented.
Files changed: List the important files.
Tests: Commands and results.
Decisions: Any new ADR or approved choice.
Known issues: Anything still incomplete.
Next task: TASK-YYY.
```

## 11. Recent Activity

### 2026-10-16 - Planning documents created

- Created the PRD for the LocalEats MVP.
- Created the architecture document for the C# and Blazor modular monolith.
- Created the project rules document.
- Created the design system document.
- Created the ordered task plan.
- Confirmed that payment belongs after the core ordering workflow.

### 2026-10-17 - Memory map created

- Created this project memory document.
- Confirmed that implementation has not started.
- Set the next task to TASK-001: Create the solution.

### 2026-10-17 - TASK-001 scaffold started

- Created `LocalEats.slnx` using the installed .NET SDK.
- Created the six source projects and four test projects.
- Added project references according to the architecture.
- Initialized Git and added `.gitignore` and `global.json`.
- Domain, Application, and Contracts projects build successfully without restore.
- Initial restore was retried with approved network access and completed successfully.
- The task was kept in progress until build, test, and format verification passed.

### 2026-10-17 - TASK-001 completed

- Restored all solution projects and NuGet packages.
- Built `LocalEats.slnx` successfully with zero warnings and zero errors.
- Added and passed the initial Domain project smoke test.
- Ran the complete test command successfully; feature test projects are empty until their feature tasks are implemented.
- Ran `dotnet format LocalEats.slnx --verify-no-changes` successfully.
- Removed the generated sample weather endpoint from the API scaffold.
- Advanced the next task to TASK-002: Configure local development.

### 2026-10-17 - TASK-002 completed

- Added `README.md` with setup, run, health-check, and verification commands.
- Added `.env.example` with documented local and future service configuration.
- Added `docker-compose.yml` with a PostgreSQL 17 development service and health check.
- Added development settings for the Web and API projects.
- Confirmed health endpoint mappings exist in both Web and API projects.
- Formatting, build, and tests passed.
- Docker CLI is not installed in this environment, so the PostgreSQL container could not be started here.
- Advanced the next task to TASK-003: Configure database foundation.

### 2026-10-17 - Portable project package created

- Added `START_HERE.md` with setup instructions for another Windows computer.
- Copied the source project, documentation, solution, Docker configuration, tests, and Codex instructions to `Desktop\\MY_PROJECT`.
- Excluded generated `bin`, `obj`, temporary work, output, and Git metadata from the portable copy.
- The next task remains TASK-003: Configure database foundation.

## 12. Current Task Handoff

The next Codex session should begin with this prompt:

```text
Read AGENTS.md, docs/PRD.md, docs/ARCHITECTURE.md, docs/RULES.md,
docs/DESIGN.md, docs/TASKS.md, and docs/MEMORY.md.

We are starting TASK-006: Build registration.
Do not implement restaurant features yet.
First inspect the repository and provide a plan listing the projects,
project references, files to create, and verification commands.
Verify Docker, migration history, development seeding, health endpoints, formatting, build and tests. Update the records only with verified results.
```

### 2026-10-27 - TASK-003 verification resumed

- TASK-001 and TASK-002 are completed. Current task is TASK-003; TASK-004 follows, then TASK-005 Identity.
- InitialDatabaseFoundation migration was applied successfully on 2026-10-21.
- Infrastructure contains EF Core/Npgsql, DbContext, mappings, database health check and development seed code.
- Restaurant/menu/order/audit entities were added early; their later feature tasks remain open.
- Docker engine was unavailable at resume; Docker Desktop startup and dependency restore are being checked.
- Removed blanket NU1902/NU1903 warning suppression; dependency advisories must stay visible.
- Added tests for missing connection configuration and prevention of seeding outside Development.
- Final build, formatting, tests, current database state and seed verification are pending. Do not mark TASK-003 complete yet.
- Earlier statements that no solution/migration exists are historical and superseded by this entry.


### 2026-10-27 - TASK-003 completed

- Docker LocalEats PostgreSQL is healthy. InitialDatabaseFoundation and all six application tables exist.
- Development API startup applied no extra migration and inserted 2 restaurants, 2 categories and 2 menu items. Starting Web afterward succeeded without duplicate-seed errors.
- API and Web /health each returned HTTP 200 Healthy.
- dotnet build passed (dependency advisories visible), dotnet format --verify-no-changes passed after normalizing UTF-8/CRLF, and dotnet test passed: 1 Domain smoke test plus 2 database foundation safety tests; 0 failed/skipped.
- Application and E2E test projects have no tests yet. Full PostgreSQL constraint/transaction automation remains TASK-044.
- Known issue: EF design-tool transitive System.Security.Cryptography.Xml/Pkcs 9.0.0 packages have NuGet security advisories. Warning suppression was removed; resolve advisories during the next dependency/quality work before deployment.
- The initial test attempt hit a running API executable file lock; both verification hosts were stopped and the retry passed.
- Next task is TASK-004 quality/CI checks. TASK-005 Identity has not started.
- Changed this session: API/Infrastructure project files, database foundation C# formatting, tests/LocalEats.Api.Tests/DatabaseFoundationTests.cs, README.md, docs/TASKS.md, docs/MEMORY.md.
- No production services or other Docker containers were modified. Desktop MY_PROJECT remains the earlier portable snapshot and was not refreshed in this session.


### 2026-10-27 - TASK-004 quality checks implemented

- Added Directory.Build.props for nullable references, analyzers, warnings-as-errors, dependency auditing and assembly-conflict errors.
- Added Directory.Packages.props and moved all project package versions into it. Patched Microsoft EF/extensions to 10.0.14, Npgsql provider to 10.0.3 and local dotnet-ef to 10.0.14. Explicit Relational pin prevents design/runtime assembly conflicts. No additional technology was introduced.
- Restore/audit passed without advisories; Release build passed with zero warnings/errors; all 3 existing tests passed, zero failed/skipped. Application and E2E feature test projects remain empty.
- Added .github/workflows/ci.yml, .gitattributes and docs/TEST_PLAN.md. Workflow checks restore, tool restore, format, Release build and tests; it uploads TRX results.
- Replaced the Domain smoke test with an architecture boundary test and documented Method_Condition_ExpectedResult test naming.
- No GitHub remote exists, so the workflow has not run on GitHub; README explicitly records hosted status as pending instead of claiming a green badge. TASK-048 will extend CI with PostgreSQL integration coverage.
- Branch: codex/task-004-quality-checks. No commit or push made.
- Updated AGENTS.md to refresh Desktop MY_PROJECT after every successfully verified task, excluding generated files and secrets and checking hashes.
- Portable copy sync: completed 2026-10-27; all 76 portable source/configuration/documentation files matched SHA-256 hashes. Docker volume data is not part of the source copy; new computers recreate sample data through migrations.
- Next task: TASK-005 Identity. Registration, login/logout and authorization remain later tasks.


### 2026-10-27 - GitHub CI connected and verified

- Verified private repository https://github.com/yasinchowdhuryxaf/LocalEats belongs to the authenticated user.
- Preserved remote initial history and pushed foundation commit b694bcc to main.
- GitHub Actions run 40399816936 passed restore/audit, tool restore, formatting, Release build and tests: https://github.com/yasinchowdhuryxaf/LocalEats/actions/runs/40399816936
- Added the live CI badge to README. GitHub CLI authorization includes workflow scope.
- GitHub reported action runtime deprecation notices; the job passed. Updating action major versions can be handled with CI maintenance.
- Desktop MY_PROJECT refresh follows this completed CI connection step; TASK-005 is next.

### 2026-10-27 - TASK-005 Identity foundation

- Added Infrastructure/Identity/ApplicationUser (GUID keys), shared role names and development role provisioning. Added ASP.NET Core Identity EF provider 10.0.14 and framework reference; removed redundant packages supplied by the shared framework.
- LocalEatsDbContext now extends IdentityDbContext. AddIdentity migration created standard AspNet tables and applied successfully to the local PostgreSQL database; existing 2 sample restaurants were preserved.
- Customer, Manager and Admin persisted exactly once. Starting Web after API reused existing roles without inserting duplicates. No preset users or passwords are created.
- Both hosts register Identity and authentication/authorization middleware. Default password hashing/token generation is provided by Identity.
- Tests cover Identity service registration/defaults, GUID account/role mappings, and standard password hashing. Existing foundation and Domain boundary tests remain.
- API and Web health endpoints returned HTTP 200 Healthy. Formatting passed. Final Release build passed with zero warnings/errors; all 6 tests passed, zero failures/skips.
- ADR docs/decisions/0001-identity-persistence.md explains why Identity-dependent ApplicationUser belongs in Infrastructure, replacing the earlier Domain example.
- Registration, login/logout, role policy enforcement and account UI remain TASK-006 onward. No sign-in UI or feature E2E coverage is claimed.
- Portable copy sync: completed 2026-10-27; all 83 portable files matched SHA-256 hashes. Next task is TASK-006 registration.

- TASK-005 hosted CI verified: run 40400739493 passed all steps on GitHub Linux: https://github.com/yasinchowdhuryxaf/LocalEats/actions/runs/40400739493 . Commit b5db5e7 is published on main. Portable copy contains the verified Identity implementation and updated memory.
