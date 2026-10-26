# Identity persistence

Date: 2026-10-27
Status: Implemented for TASK-005

ASP.NET Core Identity owns account credentials, roles and password hashing. ApplicationUser inherits IdentityUser<Guid> and lives in Infrastructure/Identity because it depends on ASP.NET Core Identity. This keeps Domain independent of web/provider packages. The architecture's earlier example placing ApplicationUser in Domain is superseded by this decision.

LocalEatsDbContext inherits IdentityDbContext with GUID user/role keys. The Identity migration adds standard AspNet tables alongside existing restaurant tables without replacing them. CustomerId/ActorId foreign-key enforcement remains part of the later ordering/audit work; TASK-005 does not add order behavior.

Both hosts register the same Identity services and authentication/authorization middleware. Login and registration pages/endpoints are separate tasks. Cookies are host-specific at this stage; cross-host sign-in is not implemented. Production must use HTTPS and managed Data Protection keys when authentication is deployed.

Development startup seeds Customer, Manager and Admin roles idempotently after migrations. No default users or passwords are seeded. Production role provisioning is an explicit deployment operation; it does not run through the development seeder.

Defaults: unique email, minimum password length 12 with Identity's standard complexity requirements, five failed sign-in attempts before a 15-minute lockout. Registration/login tasks will use these defaults and Identity managers rather than custom credential logic.
