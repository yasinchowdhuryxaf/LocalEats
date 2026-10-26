# Permissions

## Server policies

Identity creates signed role claims at login. Both hosts register the same policies through AddInfrastructure. Names and roles live in Application/Security so use cases and transports share one definition.

| Route/capability | Anonymous | Customer | Manager | Admin |
|---|---|---|---|---|
| Register/login | Public | Public | Public | Public |
| /session and logout confirmation | Login redirect | Allow | Allow | Allow |
| /account profile read/edit | Login redirect | Own only | Own only | Own only |
| /dashboard and /session/customer — CustomerAccess | Login redirect | Allow | Deny | Deny |
| /manager — ManagerAccess | Login redirect | Deny | Allow | Deny |
| /admin — AdminAccess | Login redirect | Deny | Deny | Allow |
| Existing customer-owned order through central service | Deny | Own only | Deny until assigned-restaurant support | Allow |
| Missing order | Deny | Deny | Deny | Deny |

Roles have no implicit hierarchy. A multi-role account receives only the union of its explicitly assigned roles. Admin operations use AdminAccess; an Admin-only account does not impersonate a Customer/Manager dashboard. Public registration always assigns Customer and accepts no role input.

The three role entry pages are minimal authorization shells. The customer dashboard is implemented in TASK-011; manager/admin dashboard features remain TASK-012/013. Cookie middleware challenges anonymous requests to /login with a validated local return URL. Signed-in users with the wrong role go to /access-denied. Route metadata enforces authorization on the server even for direct URL requests; hiding navigation links is only a presentation choice.

## Resource ownership

IResourceAuthorizationService.CanReadOrderAsync accepts the server's authenticated ClaimsPrincipal and an order ID. It obtains the caller ID from Identity claims, never from form data. The Infrastructure reader loads only the matching order's ownership fields from PostgreSQL using a scoped, no-tracking query. Customers can read only their own order; explicit Admin accounts can read an existing order. Anonymous users, malformed/empty caller IDs, unrecognized roles and missing records are denied. Denied and missing resources should map to the same not-found response in customer-facing order use cases.

No order-view endpoint is implemented yet. TASK-023 must call this central service (or equivalently scope its data query) before returning order details, then add HTTP isolation tests. The current service is tested against actual database ownership; it does not accept client-supplied owner IDs.

ManagerRestaurant persistence is scheduled for TASK-025. Until that assignment is stored and checked on the server, Manager-only accounts are denied individual order access by this service. TASK-025 will add verified assigned-restaurant checks and cross-restaurant denial tests; no assignment is inferred from URL parameters or unverified claims.

## Role changes and future features

This task provides no public role-grant endpoint, default admin accounts, manager assignment UI or user administration. Trusted provisioning is required. Identity cookies capture roles at sign-in; immediate role-removal/suspension revocation must be implemented with TASK-034 before administrative account changes are exposed. TASK-009 implements own-profile editing, and TASK-025 adds restaurant ownership. Do not mark those tasks complete based on these policy definitions.

## Verification

HTTP tests exercise the complete 3×3 role/page matrix and anonymous route access using real Identity cookies. Application tests cover malformed/unauthenticated role claims, ownership mismatch and missing records. A PostgreSQL integration test proves that changing an order ID denies a Customer while explicit Admin access is allowed. Test data is isolated in the dedicated `_tests` database and cleaned by its owning fixture.
