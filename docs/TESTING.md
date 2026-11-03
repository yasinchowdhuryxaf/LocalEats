# Testing and reliability

Testing focuses on behavior that could affect customer orders, restaurant access or stored data.

## Latest local verification

| Test area | Passed |
| --- | ---: |
| Domain rules | 62 |
| Application services | 145 |
| HTTP and PostgreSQL integration | 232 |
| **Total** | **439** |

The latest run had no failures or skipped tests. Formatting verification and the build also passed, with zero warnings and errors. These results describe the application verification performed before preparing this portfolio overview.

## Behaviors covered

- Registration, login and protected routes.
- Customer ownership and cross-restaurant access restrictions.
- Menu validation and preservation of items used in historical orders.
- Exact price conversion and server-side checkout calculations.
- Transactional order creation and protection against repeated submissions.
- All pairs of defined order statuses, including invalid transitions.
- Confirmation and reason requirements for destructive order actions.
- Stale forms, revoked restaurant assignments and competing updates.
- Correct audit actor, restaurant, order, timestamp and status metadata.
- No extra audit events for rejected, stale or repeated commands.

## Testing a real database failure

An integration test deliberately makes PostgreSQL reject an audit insert after the status update has executed inside its transaction. The test then verifies that the order's status, rejection reason and update timestamp remain unchanged and no audit row is committed.

A subsequent valid request succeeds and creates exactly one audit record. This checks the rollback behavior against PostgreSQL rather than relying only on a mock.

## Isolation

Integration tests use dedicated test databases and fixture-owned records. Manual walkthroughs use a separate disposable demonstration database. They do not require changing normal user accounts or restaurant availability.

## Remaining verification

Automated browser journeys, responsive and keyboard audits, production security review, load testing, deployment checks and rollback exercises remain planned work. Local test results are not a claim of production readiness.
