# Development roadmap

The project is developed in small steps, with validation and tests completed alongside each feature.

## Completed

- Project structure, PostgreSQL configuration and database migrations.
- Identity-based registration, login, account pages and role protection.
- Restaurant directory, menu pages and customer basket.
- Checkout, transactional order creation and saved order history.
- Persisted Manager-to-restaurant assignments.
- Restaurant profile, category and menu-item management.
- Restaurant order lists with filtering and pagination.
- Validated Manager order actions and rejection reasons.
- Transactional audit records for Manager status changes.
- Automated domain, application and PostgreSQL-backed HTTP tests.

## Next: administration

The next phase covers restaurant approval and suspension, user administration and a paginated audit-log viewer. These features will use explicit Admin permissions and server-side resource checks.

## Before a production release

- Automate the main Customer and Manager browser journeys.
- Complete accessibility and mobile-layout reviews.
- Review security, session behavior and account suspension.
- Select hosting and configure preview deployment.
- Establish backups, monitoring, error tracking and rollback procedures.

## Deliberately outside the first release

Online payment gateways, rider assignment, live location tracking, promotions, reviews and native mobile applications are deferred. The current priority is a reliable ordering workflow with a clear and maintainable C# foundation.
