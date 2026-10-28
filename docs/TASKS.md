# Project Tasks
# Project: LocalEats

This file defines the implementation order for LocalEats. Build the project in small, reviewable steps. Complete and verify one task before starting the next task.

## How to Use This File

For every task:

1. Read `AGENTS.md`, `docs/PRD.md`, `docs/ARCHITECTURE.md`, `docs/DESIGN.md`, and `RULES.md`.
2. Create a branch for the task.
3. Ask Codex for a plan before code changes.
4. Implement only the task’s stated scope.
5. Add meaningful tests.
6. Run formatting, build, and relevant tests.
7. Review the diff.
8. Update this file and `docs/MEMORY.md`.
9. Commit the completed task.

Recommended task states:

```text
[ ] Not started
[-] In progress
[x] Complete
[!] Blocked
```

Recommended branch names:

```text
feature/TASK-001-project-setup
feature/TASK-010-login
feature/TASK-020-customer-dashboard
fix/TASK-XXX-description
```

## Required Verification After Every Task

```text
dotnet format --verify-no-changes
dotnet build
dotnet test
```

Also run the focused test for the feature. Do not mark a task complete when tests are failing or skipped without an explanation.

# Phase 1: Project Setup

Goal: create a clean, buildable C# solution with the agreed architecture.

## TASK-001: Create the solution

Status: [x] Complete

- [x] Create `LocalEats.slnx`.
- [x] Create Web, Api, Application, Domain, Infrastructure, and Contracts projects.
- [x] Create Domain, Application, API, and end-to-end test projects.
- [x] Add project references according to `ARCHITECTURE.md`.
- [x] Enable nullable reference types through the generated project defaults.
- [x] Add `.editorconfig`.
- [x] Add shared `Directory.Build.props` and `Directory.Packages.props` (completed in TASK-004).

Acceptance criteria:

- The solution restores and builds.
- No circular project references exist.
- Domain does not reference infrastructure or UI.

## TASK-002: Configure local development

Status: [x] Complete

- [x] Add `README.md` setup instructions.
- [x] Add `.env.example` without real secrets.
- [x] Add Docker Compose for local PostgreSQL.
- [x] Add local development configuration.
- [x] Add a basic ASP.NET Core health endpoint.
- [x] Document the required .NET SDK and commands.

Acceptance criteria:

- A new developer can start PostgreSQL and run the application by following `README.md`.
- `/health` returns a successful response.

## TASK-003: Configure database foundation

Status: [x] Complete — verified 2026-10-27.

- [x] Add EF Core and Npgsql.
- [x] Create `LocalEatsDbContext`.
- [x] Configure database dependency injection.
- [x] Add database health checking.
- [x] Create and apply the first migration.
- [x] Add development-only seed infrastructure.

Acceptance criteria:

- The application can connect to PostgreSQL.
- Migrations run successfully.
- No database tables are created manually.

## TASK-004: Configure quality checks

Status: [x] Complete — local checks and hosted GitHub CI verified; see MEMORY.md.

- [x] Add formatting and analyzer settings.
- [x] Add a GitHub Actions workflow for restore, build, format, and tests.
- [x] Add a test project smoke test.
- [x] Add a consistent test naming convention.

Acceptance criteria:

- CI fails when build, formatting, or tests fail.
- The README shows the CI status.

# Phase 2: Authentication

Goal: allow customers, managers, and administrators to sign in safely.

## TASK-005: Add application user and Identity

Status: [x] Complete — verified 2026-10-27.

- [x] Add `ApplicationUser`.
- [x] Configure ASP.NET Core Identity.
- [x] Add Identity migrations.
- [x] Seed the development roles: Customer, Manager, Admin.
- [x] Do not implement custom password hashing.

Acceptance criteria:

- Users and roles are stored in PostgreSQL.
- Passwords are handled by ASP.NET Core Identity.

## TASK-006: Build registration

Status: [x] Complete — local verification and hosted CI recorded in MEMORY.md.

- [x] Create the registration page.
- [x] Add email, password, and password confirmation fields.
- [x] Add server-side validation.
- [x] Add duplicate-email handling.
- [x] Assign the Customer role by default.
- [x] Add loading, success, and error states.

Tests:

- Valid registration succeeds.
- Invalid email fails.
- Weak password fails.
- Password confirmation mismatch fails.
- Duplicate email returns a safe message.

## TASK-007: Build login and logout

Status: [x] Complete — verification recorded in MEMORY.md.

- [x] Create the login page.
- [x] Implement login using Identity.
- [x] Implement logout.
- [x] Handle invalid credentials safely.
- [x] Add a return URL only after validating it as a local URL.
- [x] Add session/cookie configuration.

Tests:

- Valid login succeeds.
- Invalid login fails without revealing whether an email exists.
- Logout invalidates the session.
- Unauthenticated users cannot open protected pages.

## TASK-008: Add role and policy authorization

Status: [x] Complete — role/ownership verification recorded in MEMORY.md.

- [x] Create Customer, Manager, and Admin policies.
- [x] Add protected route behavior.
- [x] Add server-side authorization checks.
- [x] Add a central resource-ownership authorization service.
- [x] Add authorization documentation to `docs/PERMISSIONS.md`.

Acceptance criteria:

- A customer cannot access manager or admin pages.
- A manager cannot access admin actions.
- Hiding a button is not the only security control.

## TASK-009: Add account page

Status: [x] Complete — verification recorded in MEMORY.md.

- [x] Create `/account`.
- [x] Display the authenticated user’s basic profile.
- [x] Add profile name and phone editing.
- [x] Validate profile changes on the server.
- [x] Add a safe account error state.

# Phase 3: Customer Dashboard

Goal: give a signed-in customer a useful starting point before implementing ordering.

## TASK-010: Create the application shell

- [x] Build the header, navigation, basket link placeholder, and account menu.
- [x] Add responsive mobile navigation.
- [x] Add consistent page container and footer.
- [x] Apply `DESIGN.md` color, typography, and spacing tokens.

Acceptance criteria:

- The shell works at 375px, 768px, and desktop widths.
- Keyboard focus is visible.

## TASK-011: Build the customer dashboard

- [x] Create `/dashboard`.
- [x] Show a welcome message.
- [x] Show recent orders placeholder or empty state.
- [x] Show a link to browse restaurants.
- [x] Show account and logout actions.
- [x] Add loading and empty states.

Acceptance criteria:

- A signed-in customer is redirected to the dashboard.
- A logged-out visitor is redirected to login.

## TASK-012: Build the manager dashboard

- [x] Create `/manager`.
- [x] Show restaurant setup status.
- [x] Show pending order count placeholder.
- [x] Add links to restaurant, menu, and orders.
- [x] Restrict the page to Manager users.

## TASK-013: Build the admin dashboard

- [x] Create `/admin`.
- [x] Show pending restaurant count placeholder.
- [x] Show user and restaurant management links.
- [x] Restrict the page to Admin users.

## TASK-014: Add dashboard integration tests

- [x] Test redirects for logged-out users.
- [x] Test role-specific dashboard access.
- [x] Test empty states.
- [x] Test that one user cannot view another user’s private dashboard data.

# Phase 4: Restaurant Directory and Menu Browsing

Goal: let customers discover approved restaurants and view menus.

## TASK-015: Create restaurant domain entities

Foundation entities were introduced during TASK-003. This task remains open for required fields, rules, indexes, and tests.

- [x] Add Restaurant.
- [x] Add Restaurant status and approval status.
- [x] Add restaurant address, area, cuisine, and opening status fields.
- [x] Add EF Core configuration and migration.
- [x] Add indexes for status, area, cuisine, and slug.

## TASK-016: Seed restaurants and menu data

- [x] Add development sample restaurants.
- [x] Add menu categories.
- [x] Add menu items and prices.
- [x] Keep seed data out of production unless explicitly requested.

## TASK-017: Build restaurant directory

- [x] Create `/restaurants`.
- [x] Show only approved and active restaurants.
- [x] Add search by name.
- [x] Add area and cuisine filters.
- [x] Add pagination.
- [x] Add loading, empty, and error states.

## TASK-018: Build restaurant detail and menu page

- [x] Create `/restaurants/{slug}`.
- [x] Show restaurant information and opening status.
- [x] Show categories and available menu items.
- [x] Show unavailable items clearly.
- [x] Add `Add to basket` placeholder behavior.

Tests:

- Unapproved restaurants are not public.
- Disabled menu items cannot be added.
- Invalid restaurant slugs return a safe not-found result.

# Phase 5: Basket and Order Creation

Goal: allow a customer to create a valid order from one restaurant.

## TASK-019: Implement basket state

- [x] Add basket state service.
- [x] Add menu item to basket.
- [x] Increase and decrease quantity.
- [x] Remove item.
- [x] Calculate display subtotal.
- [x] Prevent items from different restaurants in one basket.
- [x] Add empty basket state.

## TASK-020: Build basket page

- [x] Create `/basket`.
- [x] Show restaurant name and basket items.
- [x] Show quantities, line totals, and subtotal.
- [x] Add checkout action.
- [x] Add mobile layout.

## TASK-021: Build checkout page

- [x] Create `/checkout`.
- [x] Add customer name, phone, address, and notes.
- [x] Add `CashOnDelivery` and `PayAtPickup` options.
- [x] Validate all fields.
- [x] Show final order summary.
- [x] Disable submission while processing.

## TASK-022: Create order transaction

Basic persistence entities exist from TASK-003; transactional ordering behavior remains unimplemented.

- [ ] Add Order and OrderItem entities.
- [ ] Re-read menu prices from PostgreSQL.
- [ ] Recalculate totals on the server.
- [ ] Reject unavailable or invalid items.
- [ ] Create the order and items in one transaction.
- [ ] Save purchased item name and price snapshots.
- [ ] Add order number generation.

Tests:

- Valid order succeeds.
- Empty basket fails.
- Invalid quantity fails.
- Changed client price is ignored.
- Disabled item fails.
- Order creation rolls back when a required operation fails.

## TASK-023: Build order confirmation

- [ ] Create `/orders/{id}`.
- [ ] Show order number, restaurant, items, total, address, and status.
- [ ] Restrict access to the order owner, manager, or admin according to policy.
- [ ] Show a not-found result for another customer’s order.

## TASK-024: Build customer order history

- [ ] Create `/orders`.
- [ ] Add pagination.
- [ ] Show order date, restaurant, total, and status.
- [ ] Add an empty state.

# Phase 6: Restaurant Manager and Menu Management

Goal: allow a manager to operate one restaurant.

## TASK-025: Add restaurant manager relationship

- [ ] Add ManagerRestaurant relationship.
- [ ] Add ownership/resource authorization checks.
- [ ] Add migration.
- [ ] Add tests for manager access and cross-restaurant denial.

## TASK-026: Build restaurant profile management

- [ ] Create `/manager/restaurant`.
- [ ] Edit name, description, address, phone, cuisine, and active status.
- [ ] Validate fields.
- [ ] Prevent managers from editing another restaurant.

## TASK-027: Build menu category management

- [ ] Create, edit, disable, and delete categories.
- [ ] Prevent deletion when business rules require preservation.
- [ ] Add manager authorization tests.

## TASK-028: Build menu item management

- [ ] Create, edit, disable, and delete menu items.
- [ ] Validate price and quantity-related fields.
- [ ] Preserve disabled items referenced by historical orders.
- [ ] Add image field placeholder without requiring file storage yet.

## TASK-029: Build manager order list

- [ ] Create `/manager/orders`.
- [ ] Show only orders for the manager’s restaurant.
- [ ] Add pagination and filters.
- [ ] Add order detail view.
- [ ] Add loading, empty, and error states.

# Phase 7: Order Status Management

Goal: let restaurant staff process incoming orders safely.

## TASK-030: Implement order status rules

- [ ] Add the allowed order status transitions from `PRD.md`.
- [ ] Reject invalid transitions.
- [ ] Add a domain test for every valid and invalid transition.

## TASK-031: Add manager order actions

- [ ] Add Accept.
- [ ] Add Reject with reason.
- [ ] Add Preparing.
- [ ] Add Ready.
- [ ] Add Completed.
- [ ] Add Cancelled where allowed.
- [ ] Add confirmation for destructive actions.

## TASK-032: Add audit events

- [ ] Add AuditLog entity.
- [ ] Record manager order status changes.
- [ ] Record actor and timestamp.
- [ ] Add integration tests.

# Phase 8: Admin Panel

Goal: allow platform administration without mixing admin rules into customer or manager code.

## TASK-033: Build restaurant approval

- [ ] Create `/admin/restaurants`.
- [ ] Show pending restaurants.
- [ ] Approve a restaurant.
- [ ] Reject a restaurant with reason.
- [ ] Suspend and reactivate a restaurant.
- [ ] Record audit events.

## TASK-034: Build user administration

- [ ] Create `/admin/users`.
- [ ] Search users.
- [ ] View role and status.
- [ ] Suspend/reactivate users if approved by the product policy.
- [ ] Ensure suspended users lose access immediately.

## TASK-035: Build audit log viewer

- [ ] Create `/admin/audit-log`.
- [ ] Add filters for actor, action, and date.
- [ ] Add pagination.
- [ ] Do not expose secrets or sensitive payloads.

# Phase 9: Email and Notifications

Goal: add useful transactional notifications without delaying core requests.

## TASK-036: Add background job infrastructure

- [ ] Configure Hangfire or the approved job provider.
- [ ] Add development dashboard protection.
- [ ] Add retry policy.
- [ ] Add failed-job visibility.
- [ ] Make jobs idempotent.

## TASK-037: Add order notifications

- [ ] Send order confirmation.
- [ ] Send rejection notification.
- [ ] Send cancellation notification.
- [ ] Add notification status tracking.
- [ ] Keep provider credentials in environment variables.

# Phase 10: Payment Gateway

Goal: add online payment only after the cash/pickup order flow is correct.

Payment is intentionally after authentication, dashboards, menu management, checkout, order creation, and status management.

## TASK-038: Decide payment provider and policy

- [ ] Choose Stripe, SSLCommerz, bKash, or another approved provider.
- [ ] Decide whether the platform or restaurant receives the funds.
- [ ] Decide refund and cancellation rules.
- [ ] Decide whether payment is required before order creation.
- [ ] Document the decision in `docs/decisions/`.

Do not implement code before these decisions are approved.

## TASK-039: Add payment intent/transaction model

- [ ] Add Payment and PaymentEvent entities.
- [ ] Store provider transaction ID.
- [ ] Store amount in integer minor units.
- [ ] Add unique constraint for provider event IDs.
- [ ] Add migration.

## TASK-040: Create payment checkout

- [ ] Create payment session or intent through the provider.
- [ ] Never trust client-submitted payment amount.
- [ ] Show pending payment state.
- [ ] Handle success and failure safely.

## TASK-041: Implement webhook handling

- [ ] Verify webhook signatures.
- [ ] Make webhook processing idempotent.
- [ ] Store processed event IDs.
- [ ] Handle success, failure, refund, and cancellation events.
- [ ] Test replaying the same webhook twice.

## TASK-042: Connect payment to order lifecycle

- [ ] Decide whether an order starts as PendingPayment or Pending.
- [ ] Prevent paid orders from being duplicated.
- [ ] Keep payment state and order state separate.
- [ ] Add customer payment status display.
- [ ] Add manager payment visibility without exposing sensitive details.

# Phase 11: Testing and Security

Goal: verify the full application from the user’s perspective and test high-risk boundaries.

## TASK-043: Unit test business rules

- [ ] Order totals.
- [ ] Quantity validation.
- [ ] Status transitions.
- [ ] Restaurant approval rules.
- [ ] Permission decisions.

## TASK-044: Integration test database behavior

- [ ] Use PostgreSQL Testcontainers.
- [ ] Test migrations.
- [ ] Test order transaction behavior.
- [ ] Test unique constraints.
- [ ] Test cross-user and cross-restaurant isolation.

## TASK-045: Add Playwright customer flow

Test:

```text
Register
→ Login
→ Browse restaurants
→ Open menu
→ Add item
→ Checkout
→ Place order
→ Refresh
→ View order
```

## TASK-046: Add Playwright manager flow

Test:

```text
Login as manager
→ Open manager dashboard
→ Add menu item
→ View customer order
→ Accept order
→ Mark order ready
```

## TASK-047: Perform security review

- [ ] Verify all protected pages.
- [ ] Verify all resource ownership checks.
- [ ] Test invalid IDs and role manipulation.
- [ ] Test unsafe input and HTML injection.
- [ ] Scan for secrets.
- [ ] Check cookies and security headers.
- [ ] Check logs for passwords, tokens, and sensitive data.

# Phase 12: CI/CD and Deployment

Goal: make the project deployable and maintainable.

## TASK-048: Configure CI

- [ ] Restore dependencies.
- [ ] Verify formatting.
- [ ] Build in Release mode.
- [ ] Run unit tests.
- [ ] Run integration tests with PostgreSQL.
- [ ] Run security/dependency checks.

## TASK-049: Configure preview environment

- [ ] Create a separate preview database.
- [ ] Configure environment variables.
- [ ] Run migrations during deployment.
- [ ] Verify health checks.
- [ ] Run customer and manager smoke tests.

## TASK-050: Configure production deployment

- [ ] Create a separate production database.
- [ ] Enable automated backups.
- [ ] Configure HTTPS and domain.
- [ ] Configure error tracking.
- [ ] Configure production logging.
- [ ] Document rollback steps.
- [ ] Require approval before production deployment.

## TASK-051: Production QA

- [ ] Test registration and login.
- [ ] Test restaurant browsing.
- [ ] Test basket and checkout.
- [ ] Test order status.
- [ ] Test manager actions.
- [ ] Test admin approval.
- [ ] Test mobile layout.
- [ ] Test invalid input and unauthorized access.
- [ ] Test health endpoint and error tracking.

# Phase 13: Maintenance and Future Growth

Do these only after the MVP is stable and monitored.

## TASK-052: Monitoring and alerts

- [ ] Monitor uptime.
- [ ] Monitor error rate.
- [ ] Monitor response latency.
- [ ] Monitor failed background jobs.
- [ ] Monitor database health.
- [ ] Write a runbook for each actionable alert.

## TASK-053: Performance review

- [ ] Review slow queries.
- [ ] Check pagination.
- [ ] Check N+1 queries.
- [ ] Review indexes.
- [ ] Review image sizes.
- [ ] Load-test the most important endpoints.

## TASK-054: Add caching only where measured

- [ ] Identify a measured slow read.
- [ ] Document cache key and expiration.
- [ ] Include restaurant/tenant scope in keys.
- [ ] Invalidate cache after writes.
- [ ] Add cache-isolation tests.

## TASK-055: Reassess product scope

Review whether the product actually needs:

- Delivery rider management.
- Reviews and ratings.
- Coupons.
- Loyalty.
- Restaurant analytics.
- Multiple branches.
- Mobile applications.
- Marketplace payment splitting.

Do not begin these features until the existing MVP is reliable and the business need is clear.

# Milestone Checklist

## Milestone 1: Foundation complete

- [x] Solution builds.
- [x] Database connects.
- [x] Health check works.
- [x] CI is green.

## Milestone 2: Authentication complete

- [x] Customer registration works.
- [x] Login/logout works.
- [x] Protected routes work.
- [x] Role policies work.

## Milestone 3: First useful customer experience

- [x] Customer can browse restaurants.
- [x] Customer can view a menu.
- [x] Customer can use the basket.
- [ ] Customer can place a cash/pickup order.

## Milestone 4: Restaurant operations complete

- [ ] Manager can manage the menu.
- [ ] Manager can view orders.
- [ ] Manager can update order status.
- [ ] Audit events are recorded.

## Milestone 5: Admin and production quality complete

- [ ] Admin can approve restaurants.
- [ ] Cross-user access is blocked.
- [ ] Automated tests cover critical flows.
- [ ] Preview deployment works.
- [ ] Production deployment and rollback are documented.

## Current Work

- Current phase: Phase 5 - Basket and Order Creation
- Current task: TASK-022
- Branch: `codex/task-021-checkout-page`
- Status: [ ] Not started
- Last completed task: TASK-021 - Build checkout page
- Next task: TASK-022 - Create order transaction
- Known issues: E2E automation remains TASK-045; manager restaurant assignments remain TASK-025.

Update this section after every completed task.
