# Permissions

## Server policies

Identity creates signed role claims at login. Both hosts register the same policies through AddInfrastructure. Names and roles live in Application/Security so use cases and transports share one definition.

| Route/capability | Anonymous | Customer | Manager | Admin |
|---|---|---|---|---|
| Register/login | Public | Public | Public | Public |
| /session and logout confirmation | Login redirect | Allow | Allow | Allow |
| /account profile read/edit | Login redirect | Own only | Own only | Own only |
| /dashboard and /session/customer — CustomerAccess | Login redirect | Allow | Deny | Deny |
| /manager and /manager/restaurant, /manager/menu, /manager/orders — ManagerAccess | Login redirect | Deny | Allow | Deny |
| /admin, /admin/restaurants and /admin/users — AdminAccess | Login redirect | Deny | Deny | Allow |
| Existing customer-owned order through central service | Deny | Own only | Assigned restaurant only | Allow |
| Missing order | Deny | Deny | Deny | Deny |

Roles have no implicit hierarchy. A multi-role account receives only the union of its explicitly assigned roles. Admin operations use AdminAccess; an Admin-only account does not impersonate a Customer/Manager dashboard. Public registration always assigns Customer and accepts no role input.

Customer, manager and admin dashboards are implemented in TASK-011/012/013. Manager and admin tool routes currently show protected coming-soon pages without resource data or mutations. Cookie middleware challenges anonymous requests to /login with a validated local return URL. Signed-in users with the wrong role go to /access-denied. Route metadata enforces authorization on the server even for direct URL requests; hiding navigation links is only a presentation choice.

## Resource ownership

IResourceAuthorizationService.CanReadOrderAsync accepts the server's authenticated ClaimsPrincipal and an order ID. It obtains the caller ID from Identity claims, never from form data. The Infrastructure reader loads only the matching order's ownership fields from PostgreSQL using a scoped, no-tracking query. Customers can read only their own order; explicit Admin accounts can read an existing order. Anonymous users, malformed/empty caller IDs, unrecognized roles and missing records are denied. Denied and missing resources should map to the same not-found response in customer-facing order use cases.

TASK-023 adds authenticated /orders/{id}. OrderDetailsService calls the central resource service before its reader, and the reader repeats customer/admin scoping in SQL before projecting details or items. Customers receive the same 404 for missing and foreign orders; Admin can read existing orders. Manager-only accounts require the matching persisted restaurant assignment (TASK-025). The page is read-only, no-store and never accepts a caller-supplied owner ID. HTTP tests cover actual Identity cookies and PostgreSQL ownership.

TASK-025 persists ManagerRestaurants with one restaurant per manager. Both the signed Manager role and a matching assignment are required; rows alone cannot grant role access. Soft-deleted restaurants deny manager access. Closed/inactive/unapproved status does not hide historical orders from an assigned manager. No assignment is inferred from URL parameters or unverified claims. Revoked assignments are denied on the next read, and order-detail SQL repeats the checks. CanManageRestaurantAsync is the reusable Application guard for future manager use cases. See ADR 0005.

## Role changes and future features

This task provides no public role-grant endpoint, default admin accounts, manager assignment UI or user administration. Trusted provisioning is required. Identity cookies capture roles at sign-in; immediate role-removal/suspension revocation must be implemented with TASK-034 before administrative account changes are exposed. TASK-009 implements own-profile editing and TASK-025 persists restaurant assignments; profile/menu/order-management screens remain their scheduled tasks.

## Verification

HTTP tests exercise the complete 3×3 role/page matrix and anonymous route access using real Identity cookies. Application tests cover malformed/unauthenticated role claims, ownership mismatch and missing records. A PostgreSQL integration test proves that changing an order ID denies a Customer while explicit Admin access is allowed. Test data is isolated in the dedicated `_tests` database and cleaned by its owning fixture.

The /restaurants directory is public for all actors. Its Infrastructure reader always scopes to approved/active/non-deleted records, regardless of caller role or filter values. No private owner/user fields are projected.

TASK-018: /restaurants/{slug} is public only for approved/active/non-deleted restaurants. Unknown/private/invalid slugs return the same 404 screen, without leaking restaurant or item data. Only active categories are projected. Unavailable items remain visible with disabled controls; no basket-add endpoint exists yet.

TASK-019: guest/session baskets are scoped only to the current protected browser session; caller-supplied owner/restaurant/price values do not choose state or pricing. Add/increase validate public item/category/restaurant availability on the server. Logout clears only this browser's basket. Basket mutation forms require antiforgery. Checkout separately requires Customer access; explicit cash order creation and details are implemented in TASK-022/023.

TASK-021: /checkout requires CustomerAccess (authenticated Customer); Manager/Admin-only accounts are denied. Application also validates authenticated GUID identity and Customer claims. Contact prefill comes only from the signed-in profile. Review does not accept an owner/total as authority and creates no order or payment.

TASK-024: /orders is CustomerAccess-only. Admin/Manager-only accounts cannot use the customer history page; a multi-role Customer still sees only its own history. The Application service requires authenticated Customer/GUID claims and Infrastructure scopes both count and rows to that signed identity. customerId/userId query parameters do not select an owner. List entries expose only order number, date, restaurant, total/currency/status and its details link; details retain the existing resource checks.


TASK-025 adds no assignment or role-grant UI/API and changes no existing user's access. Trusted provisioning remains separate. Manager-only customer history access remains denied; assigned order details at /orders/{id} are the existing read surface. Tests cover matching/cross-restaurant reads, assignment removal, soft deletion, missing assignment, row-without-role denial and database constraints.