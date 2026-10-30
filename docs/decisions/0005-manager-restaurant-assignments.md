# ADR 0005: Verified manager assignments

Status: accepted for the planned TASK-025 relationship and authorization slice.

Each Manager can be assigned to at most one restaurant in the light MVP. ManagerRestaurants uses ManagerId as its primary key, a RestaurantId foreign key and a UTC CreatedAt timestamp. Several managers can share a restaurant. This is a restaurant-specific implementation choice aligned with the PRD's one-restaurant manager scope.

The assignment references an existing Identity user and restaurant. Removing either reference cascades only the assignment row. Existing order retention rules remain unchanged. A row does not grant the Manager role: resource checks require both signed Manager claims and a matching persisted assignment. No public assignment or role-grant endpoint, automatic backfill, default Manager account or real account assignment is added.

CanManageRestaurantAsync is a reusable Application permission check for future manager operations. Customer/Admin-only roles do not inherit manager-tool permissions. Customer own-order and explicit Admin order reads retain their existing semantics; Manager order reads require a matching assignment. Multi-role access is the union of explicit capabilities.

Soft-deleted restaurants deny manager access. Closed, inactive or unapproved restaurants can still have their historical records read by an assigned Manager; public visibility and new-order eligibility remain separate rules. This step exposes no profile/menu/status mutation. TASK-026 and later manager commands must enforce any additional mutation restrictions on the server.

Assignments are queried on every request without caching. Revoking an assignment takes effect on the next read without changing the login cookie. Order details repeat the role and assignment predicate in SQL for both metadata and item queries, protecting against an assignment changing after the initial authorization check. Requests cannot nominate their own restaurant assignment through route, query or claims fields.

Identity role-cookie invalidation remains the separately documented TASK-034 concern. This decision does not claim immediate role revocation across existing sessions. Trusted assignment provisioning and its administration/audit flow must be implemented separately before an operational manager onboarding workflow is exposed.
