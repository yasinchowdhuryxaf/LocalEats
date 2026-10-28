# Restaurant foundation status model

Date: 2026-10-29. TASK-015.

Keep the existing IsApproved and IsActive flags as the single source of truth for approval and platform availability. Do not introduce duplicate enum columns. Detailed rejection/review workflow belongs to TASK-033.

Add IsOpen for the manager-controlled open/closed state, defaulting to false. IsVisible means approved, active and not soft-deleted. CanAcceptOrders additionally requires IsOpen. A closed but otherwise visible restaurant may be browsed; checkout must later enforce availability on the server. These derived Domain properties are not database columns; EF list queries must express their underlying predicates for SQL translation.

Address is optional up to 500 characters; Area and Cuisine remain optional up to 120 characters to preserve existing drafts and sample records. Actual publication/checkout validation is enforced by later use cases. No guessed address or automatic opening is backfilled. Existing approval/active values are preserved.

Retain the unique slug and approval/active index; add area/cuisine indexes. Database constraints reject space-only names/slugs. No new runtime dependency is required.

Local development isolation: publish only the LocalEats container at 127.0.0.1:15432 while retaining its internal 5432 port and existing volume. The host already has a separate PostgreSQL listener; changing LocalEats avoids touching that service or any unrelated project. CI continues to use its explicitly configured ephemeral PostgreSQL service.
