# Development Rules
# Project: LocalEats

This file is the working rulebook for developers and OpenAI Codex. Read it before changing code. If a new decision conflicts with this file, document the decision in `docs/decisions/` and update this file when the decision is approved.

## 1. General Principles

### 1.1 Build the smallest correct feature

- Build one small vertical slice at a time.
- Prefer a simple solution that is easy to explain and maintain.
- Do not add a feature because it may be useful later.
- Do not introduce microservices, message brokers, or complex infrastructure for the MVP without an approved decision record.

### 1.2 Plan before coding

Before implementing a non-trivial task:

1. Read `AGENTS.md`, `docs/PRD.md`, `docs/ARCHITECTURE.md`, and the relevant feature documentation.
2. Inspect the existing implementation.
3. State the files that will change.
4. Describe the implementation approach.
5. Identify assumptions and risks.
6. Wait for review when the task changes architecture, database schema, permissions, or public API behavior.

### 1.3 Keep changes reviewable

- One feature or fix per branch.
- Keep commits focused and descriptive.
- Do not mix formatting changes with behavior changes.
- Do not modify unrelated files.
- Do not rewrite working code only for personal preference.
- Review the diff before committing.

### 1.4 The server is the source of truth

- UI validation improves user experience but does not replace server validation.
- UI visibility does not replace authorization.
- Prices, order totals, menu availability, and status transitions are recalculated or checked on the server.
- Never trust IDs, roles, prices, quantities, or status values received from the browser.

### 1.5 Write decisions down

- Permanent technical choices belong in `docs/decisions/`.
- Current implementation status belongs in `docs/MEMORY.md`.
- Update `docs/TASKS.md` when work starts or finishes.
- Update the relevant documentation when behavior changes.

### 1.6 Definition of done

A task is not complete until:

- The implementation matches the PRD and architecture.
- Validation and authorization are present.
- Relevant tests pass.
- `dotnet build` passes.
- `dotnet test` passes.
- Formatting and analyzers pass.
- The changed files and assumptions are reported.
- No secrets, debug code, skipped tests, or unexplained hard-coded values were added.

## 2. Technology Standards

### 2.1 Approved stack

- Language: C#.
- Runtime: the .NET version recorded in `global.json` or the solution documentation.
- UI: Blazor Web App.
- Backend: ASP.NET Core.
- Database: PostgreSQL.
- ORM: Entity Framework Core with Npgsql.
- Authentication: ASP.NET Core Identity or the approved managed provider.
- Validation: FluentValidation.
- Tests: xUnit, FluentAssertions, Testcontainers, and Playwright where appropriate.
- Logging: Serilog or the approved structured logging provider.
- Background work: Hangfire or the approved job provider.

Do not replace an approved technology without an architecture decision record.

### 2.2 C# coding standard

- Enable nullable reference types.
- Treat compiler warnings as errors where practical.
- Use `async`/`await` for I/O-bound work.
- Accept and pass `CancellationToken` through application and database operations.
- Use meaningful names; avoid abbreviations.
- Use one public type per file unless a small private helper clearly belongs with it.
- Keep methods small and focused on one responsibility.
- Prefer immutable request and response models where practical.
- Use `record` types for DTOs when they represent data only.
- Use enums for finite state values such as `OrderStatus`.
- Do not use `dynamic` unless an approved integration requires it.
- Do not suppress warnings without a comment explaining why.
- Avoid `null!`; fix the object initialization or model the nullable state correctly.
- Use explicit access modifiers.
- Use `var` when the type is obvious; use an explicit type when it improves clarity.
- Do not use `DateTime.Now` for persisted or business timestamps; use `DateTimeOffset` and UTC.
- Do not use floating-point types for money. Store integer minor units such as `long TotalMinor`.

### 2.3 Formatting and analyzers

- Run `dotnet format` before committing.
- Follow the repository `.editorconfig`.
- Keep files UTF-8.
- Do not add unused using directives.
- Do not leave compiler warnings, analyzer warnings, or TODO comments without a linked task.

### 2.4 Entity Framework Core and PostgreSQL

- All schema changes require an EF Core migration.
- Never edit production tables manually.
- Never commit generated migration changes without reviewing them.
- Configure entities in separate configuration classes where the mapping is non-trivial.
- Use explicit foreign keys and indexes.
- Add `CreatedAt`, `UpdatedAt`, and `DeletedAt` where the entity requires recovery or history.
- Use database constraints for required fields, uniqueness, and valid ranges.
- Use `AsNoTracking()` for read-only queries.
- Project list queries to DTOs instead of loading full entities unnecessarily.
- Paginate every potentially large list.
- Never load an unbounded table into memory.
- Use a transaction for order creation and other multi-record operations that must be atomic.
- Do not expose EF Core entities directly as public API responses.

### 2.5 Blazor standards

- Keep components focused on rendering and user interaction.
- Put business operations in application services.
- Put reusable controls in `Components/Shared`.
- Use `EditForm` and the approved validation integration.
- Every data-driven page must handle loading, empty, error, and success states.
- Disable submit controls while a request is in progress.
- Show field-level validation messages.
- Use semantic HTML and accessible labels.
- Do not place database queries in `.razor` files.
- Do not put secrets or private configuration in browser-delivered code.

### 2.6 API and contract standards

- Use clear resource names and consistent routes.
- Validate request bodies, route parameters, query parameters, and headers at the boundary.
- Use DTOs from `LocalEats.Contracts`.
- Use a consistent error response shape.
- Use appropriate status codes:
  - `200` for successful reads or updates.
  - `201` for successful creation.
  - `204` for successful operations with no response body.
  - `400` for invalid input.
  - `401` for unauthenticated requests.
  - `403` for authenticated users without permission.
  - `404` for missing or inaccessible resources.
  - `409` for business conflicts such as an invalid state transition.
  - `429` for rate limiting.
- Do not expose database exception details.
- Use pagination for list responses.

## 3. Error Handling Standards

### 3.1 Error categories

Use these categories consistently:

```text
ValidationError
Unauthorized
Forbidden
NotFound
Conflict
UnexpectedError
```

### 3.2 Expected errors

Expected business errors should be represented as application results or known exceptions and mapped to a friendly response.

Examples:

- Invalid menu item quantity → `ValidationError`.
- Order belongs to another customer → `NotFound`.
- Manager does not manage the restaurant → `Forbidden` or `NotFound` according to the security policy.
- Invalid order status transition → `Conflict`.
- Restaurant is not approved → `Conflict`.

### 3.3 Unexpected errors

- Handle unexpected exceptions in one global middleware or pipeline location.
- Log the full exception server-side with a request ID.
- Return a generic message to the user.
- Send unexpected production exceptions to the approved error tracker.
- Never show stack traces, SQL, connection strings, tokens, or internal file paths to users.

### 3.4 Error response format

Use this shape for API errors:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "The request is invalid.",
    "requestId": "req_123",
    "fields": {
      "phoneNumber": "A valid phone number is required."
    }
  }
}
```

Rules:

- `code` is stable for clients.
- `message` is safe and understandable.
- `requestId` lets support find the server log.
- `fields` is optional and only used for field validation errors.

### 3.5 Logging standards

- Use structured logs, not scattered `Console.WriteLine` calls.
- Include request ID, operation name, result, and duration where useful.
- Include user ID and restaurant/organization ID only when safe and available.
- Redact passwords, tokens, cookies, authorization headers, payment details, and sensitive personal data.
- Do not log complete request bodies by default.
- Use `Information` for normal business events, `Warning` for recoverable problems, and `Error` for failures.
- Do not swallow exceptions with an empty `catch` block.
- If an exception is handled, explain why and record enough context to diagnose it.

### 3.6 User interface error behavior

- Show a clear, short message.
- Preserve user input when safe.
- Provide a retry action for recoverable read failures.
- Never blame the user for an internal failure.
- Do not display technical exception messages.

## 4. Project Structure Rules and Constraints

### 4.1 Project responsibilities

#### `LocalEats.Domain`

Allowed:

- Entities.
- Enums.
- Value objects.
- Core business rules.
- Domain events.

Forbidden:

- ASP.NET Core dependencies.
- EF Core dependencies.
- UI code.
- HTTP code.
- Provider-specific code.

#### `LocalEats.Application`

Allowed:

- Use cases.
- Application services.
- Permission checks.
- Validators.
- DTO mapping.
- Interfaces for external dependencies.

Forbidden:

- Direct PostgreSQL or provider-specific code.
- Blazor components.
- HTTP-specific assumptions inside domain services.

#### `LocalEats.Infrastructure`

Allowed:

- EF Core and PostgreSQL code.
- Identity persistence.
- Email, storage, cache, logging, and background-job integrations.
- Implementations of Application interfaces.

Forbidden:

- Business decisions that belong in Domain or Application.
- UI rendering.

#### `LocalEats.Api`

Allowed:

- Endpoint definitions.
- HTTP mapping.
- Middleware.
- OpenAPI configuration.

Forbidden:

- Direct `DbContext` usage in endpoints.
- Duplicated business logic.
- Trusting client-provided authorization or prices.

#### `LocalEats.Web`

Allowed:

- Pages, components, layouts, UI state, and API clients.
- User interaction and presentation rules.

Forbidden:

- Direct database access.
- Connection strings or secrets.
- Authorization implemented only by hiding controls.
- Duplicated order, price, or permission rules.

#### `LocalEats.Contracts`

Allowed:

- Request models.
- Response models.
- Error contracts.
- Shared transport enums when needed.

Forbidden:

- EF Core entities.
- Database navigation properties.
- Provider-specific types.

### 4.2 Dependency constraints

```text
Domain
  ↓
Application
  ↓
Infrastructure

Web ───────────────→ Application and Contracts
Api ───────────────→ Application and Contracts
Infrastructure ───→ Application and Domain
```

- Domain must remain independent.
- Application may depend on Domain, never Infrastructure.
- Infrastructure implements Application interfaces.
- Web and Api must use Application services.
- Do not add circular project references.
- Do not create a second database access layer.

### 4.3 Feature organization

Keep related code together by feature inside Application, Contracts, and Web:

```text
Application/Orders/
├── Commands/
├── Queries/
├── Validators/
├── Dtos/
└── OrderService.cs
```

Do not place every class in one global `Services` or `Helpers` folder.

### 4.4 Database and authorization constraints

- Every customer order query must scope by the current customer ID.
- Every manager query must scope by the manager’s assigned restaurant.
- Every admin operation must use an explicit admin policy.
- Never trust a restaurant ID supplied by the browser without checking ownership.
- Do not rely on global query filters as the only authorization mechanism.
- Historical order items must preserve their purchased name and price.
- Disabled menu items cannot be added to new orders.

### 4.5 Configuration and secrets

- Store configuration in environment variables or approved secret storage.
- Commit `.env.example`, never `.env` or real credentials.
- Validate required configuration during startup.
- Keep separate local, preview, and production values.
- Never place private keys in `wwwroot` or browser JavaScript.
- Do not hard-code database URLs, passwords, provider tokens, or production hostnames.

### 4.6 Testing constraints

- Add unit tests for domain rules.
- Add application tests for use cases and permission failures.
- Add integration tests using PostgreSQL for database behavior.
- Add API tests for authentication, authorization, validation, and error responses.
- Add Playwright coverage for the main customer order flow.
- Test happy paths, invalid input, empty data, conflicts, and unauthorized access.
- Do not change a test merely to make it pass.
- Do not skip a test without documenting the reason and linking a task.

### 4.7 Git constraints

- Use a feature or fix branch for each task.
- Keep `main` deployable.
- Use descriptive commit messages such as:
  - `feat: add restaurant menu browsing`
  - `fix: prevent cross-customer order access`
  - `test: cover invalid order transitions`
- Never commit secrets or generated local databases.
- Review `git diff` and `git status` before committing.

## 5. Codex Working Procedure

For every task, Codex should:

1. Read the relevant documentation.
2. Inspect the existing code.
3. Restate the task and acceptance criteria.
4. Produce a short implementation plan.
5. Identify files to change.
6. Implement only the requested scope.
7. Add or update meaningful tests.
8. Run formatting, build, and relevant tests.
9. Review the diff for unrelated changes and security issues.
10. Report changed files, tests run, assumptions, skipped work, and remaining issues.
11. Update `docs/TASKS.md`, `docs/MEMORY.md`, or an ADR when appropriate.

Do not say a task is complete when tests are failing, behavior is stubbed, or important work was skipped without clearly reporting it.
