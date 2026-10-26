# Test plan

## Local verification and CI

Run from the repository root with the SDK selected by global.json:

```powershell
dotnet restore LocalEats.slnx
dotnet tool restore
dotnet format LocalEats.slnx --verify-no-changes --no-restore
dotnet build LocalEats.slnx --configuration Release --no-restore
dotnet test LocalEats.slnx --configuration Release --no-build --no-restore
```

Compiler/analyzer warnings and dependency audit warnings fail the build/restore. Do not suppress audit warnings to obtain a passing result.

## Naming and organization

- Test file/class: `<Subject>Tests`.
- Test method: `<Method>_<Condition>_<ExpectedResult>`.
- Arrange inputs, perform one operation, then assert observable behavior.
- Domain tests cover business rules; Application tests cover use cases; API tests cover boundaries and infrastructure integration; E2E tests cover browser journeys.
- Current API foundation tests need no live database. Application and E2E projects are placeholders until their feature tasks; an empty project does not mean its features are tested.
- Real PostgreSQL migration/constraint tests belong to TASK-044 and browser automation to TASK-045.

## Hosted checks

`.github/workflows/ci.yml` runs restore, tool restore, formatting, Release build and tests on pushes and pull requests. Each failing command fails the job. Test results are retained as artifacts. No deployment or database mutation occurs in this initial workflow. TASK-048 extends it with PostgreSQL integration coverage.

GitHub execution and its badge require a GitHub repository remote and a push. Local verification is recorded separately from hosted status.
