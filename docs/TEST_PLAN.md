# Test plan

## Local verification and CI

Run from the repository root with the SDK selected by global.json:

Start PostgreSQL and create the separate registration test database once before running the full suite:

```powershell
docker compose up -d postgres
docker compose exec postgres createdb -U localeats localeats_registration_tests
```

If that database already exists, use it; do not recreate or delete it. Registration tests migrate it and remove only their own generated users. They reject database names that do not end in `_tests`. Set `LOCALEATS_TEST_CONNECTION` to use another dedicated PostgreSQL test database. The application database remains `localeats`.

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
- Foundation/Identity unit tests need no live database. Registration persistence tests use the separate PostgreSQL test database. Application tests cover registration validation. The E2E project remains a placeholder; browser verification for TASK-006 was performed manually using the browser tool.
- Real PostgreSQL migration/constraint tests belong to TASK-044 and browser automation to TASK-045.

## Hosted checks

`.github/workflows/ci.yml` runs restore, tool restore, formatting, Release build and tests on pushes and pull requests. Each failing command fails the job. Test results are retained as artifacts. An ephemeral PostgreSQL service supplies the registration test database; production and local application data are never used by CI. TASK-048 extends coverage to the later business workflows.

GitHub execution and its badge require a GitHub repository remote and a push. Local verification is recorded separately from hosted status.

## Login/logout HTTP checks (TASK-007)

The API test project references the Web host and Microsoft.AspNetCore.Mvc.Testing (test-only) to send actual HTTP form requests through an in-process host. Tests use the same dedicated `_tests` PostgreSQL database; PostgreSQL test classes run serially and remove only their generated users. Covered: authentication/anonymous access, generic failure messages, local-only return URLs, protected logout POST, cookies, remember option, and lockout. Browser verification also exercised registration → failed login → successful login → logout → protected-page redirect with a disposable test account.

## Role and ownership checks (TASK-008)

The HTTP suite tests the full 3×3 signed-role/entry-page matrix plus anonymous access to each role page. Application security tests reject untrusted/malformed principals, unknown roles, other-customer orders and missing records. The PostgreSQL isolation test creates two orders with different owners, checks access using Identity-generated claims, and verifies explicit Admin access. It removes only its own restaurant/orders and user during cleanup. These tests do not claim a complete manager assignment or dashboard/order-view feature.

## Account/profile checks (TASK-009)

Application tests check normalization, optional fields, limits, phone syntax and authenticated caller ID. HTTP/PostgreSQL tests verify saved profiles across requests, rejected invalid input, identity-bound access despite overposted IDs/email, stale-version conflicts, antiforgery, safe deleted-user errors, HTML encoding and phone verification reset. Test fixtures clean only their generated users. The additive display-name migration is exercised by test DB initialization.

TASK-010: HTTP tests verify public/Customer shell navigation and absence of privileged links. Manual browser checks at 375/768/1440 verify no horizontal overflow, native menu Enter operation and visible keyboard focus. Route permission tests continue to enforce authorization independently of navigation.

TASK-011: HTTP tests cover Customer-only /dashboard, anonymous login challenge, denied Manager/Admin, encoded profile greeting, placeholder links, default dashboard login destination and preserved local return URLs. Browser verified default login, responsive cards at 375/768/1440 and logout. Order history queries remain TASK-023; browser E2E automation remains TASK-045.

TASK-012: HTTP role matrix covers /manager and its restaurant/menu/orders destinations, including anonymous challenges and Customer/Admin-only denial. Allowed Manager responses verify setup/count placeholders, links and route-specific headings. Browser verified anonymous login redirect. Authenticated visual QA was not run because temporary UI-account privilege provisioning was rejected by automatic approval review; no grant was made. Existing isolated test fixtures verified authenticated responses.

TASK-013: HTTP role matrix covers /admin, /admin/restaurants and /admin/users. Tests verify Admin-only access, Customer/Manager denial, anonymous login challenges, count placeholder, management/account/logout links and route-specific placeholder headings. Browser confirms anonymous /admin redirects to login. Authenticated visual QA is not performed; authenticated rendering is checked with isolated HTTP test fixtures. No development-account privilege changes are made.

## Dashboard integration coverage (TASK-014)

| Requirement | Coverage |
|---|---|
| Logged-out redirects | Existing anonymous route matrix for customer, manager, admin and tool routes; added post-logout challenges for all three dashboards. |
| Role access | Existing real Identity cookie role matrix permits only each explicitly assigned dashboard role and denies other roles. |
| Empty/placeholder state | Blank Customer profile renders Welcome back and the future-order placeholder. Manager/Admin fixtures assert unavailable counts and correct navigation. |
| Customer isolation | Two separately signed-in customers request each other's IDs through userId/customerId query strings. Each response contains only its caller's display name. |
| Missing profile | Delete only the fixture's own test account after login; assert safe load failure and retry without account details or provider exceptions. |

These tests use real HTTP requests, Identity cookies and PostgreSQL in the dedicated `_tests` database. Each fixture removes only its generated users. No running development account is granted privileges. This task introduces no schema, runtime behavior or dependencies.

Order-history and assigned-restaurant isolation cannot be asserted as implemented features yet: the dashboards currently expose placeholders. Add those integration cases when TASK-023 and TASK-025 connect the real data. Browser E2E automation remains TASK-045; manager/admin visual QA remains outstanding.

TASK-015 local tests use only the LocalEats PostgreSQL endpoint 127.0.0.1:15432. CI supplies LOCALEATS_TEST_CONNECTION for its own ephemeral service. New tests cover status/soft-deletion visibility, closed defaults, address persistence, and PostgreSQL rejection of blank names/slugs and oversized addresses. No ARAM PROJECT resources are accessed or modified.

TASK-016 and migration safety: factory tests reject missing, port-5432 and unrelated database endpoints before connecting, and accept the explicit LocalEats target. A real PostgreSQL seed fixture verifies samples even with an existing restaurant, complete category/item graphs, positive integer prices, open/closed examples, repeat-run stable IDs/counts and preservation of developer edits. Existing outside-Development test verifies no database resolution.

Seed test isolation: the seed fixture creates a uniquely named localeats_seed_<guid>_tests database on the configured LocalEats test server, migrates it and drops only that exact fixture database afterward. The LocalEats test role needs CREATEDB permission (provided by the local/CI PostgreSQL container). Existing test sample records are neither cleared nor overwritten. Other test fixtures keep their existing dedicated _tests database.

TASK-017: public HTTP tests create fixture-owned restaurant rows and verify hidden flags/deletion, stable 6-item pagination, case-insensitive name/area/cuisine filters, closed visibility, empty/invalid filter/page responses and safe injected read failures. Browser demonstrated search/filter/empty/reset and responsive widths 375/768/1440. Final live preview remains open; screenshot proof is outputs/restaurant-directory.png. No real development records were changed for pagination demonstration.

TASK-018: real HTTP/PostgreSQL tests verify 21-item pagination, inactive-category exclusion, unavailable/disabled basket placeholders, BDT minor-unit formatting, private/inactive/deleted/missing/invalid restaurant 404s, closed/empty menu and safe 503 read failure. Browser demonstrated real sample menu and checked responsive widths 375/768/1440. Screenshot outputs/restaurant-menu.png and live menu tab provide visual proof. E2E automation remains TASK-045.

TASK-019: Application tests cover totals, decrease-to-empty, restaurant mixing, quantity/line limits, unsafe prices and concurrent increments. HTTP/PostgreSQL tests cover guest add/increase/decrease/remove, separate client isolation, ignored price/restaurant overposting, unavailable/closed/hidden rejection, second-restaurant denial, logout clearing, antiforgery and no-store headers. Existing closed development samples remain unchanged at the user's request.

TASK-020: HTTP tests verify /basket empty/no-store state, restaurant/item/unit and line totals, update/decrease/remove/clear, separate cookie-client privacy, disabled checkout and missing-antiforgery rejection. Browser demonstrated menu navigation to empty /basket and checked 375/1440 fit. Filled flows use isolated fixtures, not changes to the user's closed development samples.

TASK-021: tests cover Customer policy/guest challenges, both cash methods, conditional delivery address, invalid contact/notes/method, changed server price and availability, CSRF, ignored total/user overposting and unchanged order counts/basket. All 177 tests pass. Pending-state JS syntax checked with node --check. Browser verified /checkout login return gate; signed-in visual and pending-button QA remain outstanding until user signs in and has a valid basket.

TASK-022: 6 new Application and 8 PostgreSQL/HTTP cases cover Customer/claims guards, invalid contact/missing review, authoritative current price, purchase snapshot retention after catalog edits, invalid/empty/disabled/changed-price drafts, same-request concurrency/idempotency, rollback after an already-saved header, receipt redirect and preserving a later basket on replay. Final suite 191 passed (8 Domain, 59 Application, 124 HTTP/database). Serialization-wrapper regression was fixed and its test rerun before the full suite. Browser remains at checkout sign-in; signed-in visuals and JS pending behavior remain unverified manually. E2E tests remain TASK-045.

TASK-023: 11 new HTTP/PostgreSQL cases cover cash delivery/pickup saved contact/item snapshots after catalog changes, customer/Admin/Manager resource policy, safe foreign/missing/invalid 404, anonymous challenge, bounded item paging with invalid/large offsets, legacy empty/null data and injected safe 503 retry. Checkout receipt link is covered. All 202 tests passed (8 Domain, 59 Application, 135 HTTP/database). Display assertions decode HTML entities to test user-visible text while retaining Razor encoding. Filled authenticated browser/responsive QA remains outstanding; the localhost sign-in boundary was observed.

TASK-024: 5 Application plus 10 HTTP/PostgreSQL cases cover Customer/GUID claims, signed owner scope, role/anonymous gates, empty/no-store history, dashboard links, no alternate-owner query selection, stable newest-first 10-row pagination including tied dates, high/invalid pages, saved details and safe 503 retry. Final 217 tests passed (8 Domain, 64 Application, 145 HTTP/database). Live signed-in empty history checked at 375/768/1191 with no overflow; filled card/paging visuals and TASK-045 E2E remain pending.
