# Development sample data

TASK-016. LocalEats only. No production seeding is enabled.

| Restaurant | Area | Cuisine | Fresh sample status | Category | Item | Price minor units |
|---|---|---|---|---|---|---:|
| Dhaka Bites | Dhanmondi | Burgers | Approved, active, open | Burgers | Classic Beef Burger | 39900 |
| Bowl House | Uttara | Bangladeshi | Approved, active, closed | Rice Bowls | Chicken Rice Bowl | 32900 |

Addresses are clearly marked demonstration addresses, not verified real locations. Prices are illustrative integer minor units.

Development host startup applies pending migrations, ensures roles, and inserts missing sample slugs with their category/item graphs in a transaction. A PostgreSQL advisory transaction lock serializes the sample inserts. Existing sample slugs, menus, prices, names, addresses and open/closed settings are preserved, including older samples created before TASK-016. No records are deleted or reset. Existing unrelated restaurants do not suppress missing samples.

Outside Development, the startup extension returns without resolving the database or seeding roles/samples. No users or default passwords are seeded. Production migrations and any deliberate production catalog import need their own deployment workflow.

Local database: 127.0.0.1:15432, database localeats. Tests use the dedicated _tests database and own fixture rows only. Never use another project's database or container.

EF migration tooling additionally requires an explicit ConnectionStrings__DefaultConnection targeting exactly 127.0.0.1:15432/localeats. Missing or other endpoints are rejected before any connection is made; no ambient settings or fallback target is used. Follow the explicit command in README.md. This safeguard does not disable automatic approval review.

The seed integration test uses its own temporary localeats_seed_<guid>_tests database and removes only that database afterward. Pre-existing sample rows in other test/development databases are preserved. Local/CI test roles require CREATEDB for this isolated fixture.
