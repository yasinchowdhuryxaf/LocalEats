# Product Requirements Document
# Project: LocalEats

## Product Overview

LocalEats is a small food-ordering web application inspired by the basic customer journey of foodpanda: a customer selects a location, browses available restaurants, opens a menu, adds items to a basket, checks out, and views the order status.

The project is intentionally limited in scope. Its purpose is to build a strong full-stack foundation that is easy to understand and extend.

### Technology direction

- Frontend: Blazor Web App
- Backend: ASP.NET Core
- Language: C#
- Database: PostgreSQL
- ORM: Entity Framework Core with Npgsql
- Authentication: ASP.NET Core Identity
- Testing: xUnit, integration tests, and Playwright for one critical browser flow

### Reference

The reference website presents a simple customer journey: enter a location, browse restaurants, open a restaurant menu, add items to a basket, proceed to checkout, and wait for delivery. This PRD uses that journey as inspiration while defining a much smaller product.

Reference: https://www.foodpanda.com.bd/

## Problem Statement

People often need a simple way to discover nearby restaurants and place a food order without calling each restaurant separately. Small restaurants also need a basic digital menu and order-management tool without adopting a large marketplace system.

LocalEats will solve the first version of this problem by connecting customers with a limited set of restaurants in one service area. It will make the ordering process clear, predictable, and easy to test.

## Goals

### Product goals

1. Let a customer browse approved restaurants.
2. Let a customer view a restaurant menu.
3. Let a customer add menu items to a basket.
4. Let a customer submit one order.
5. Let a customer see the current order status.
6. Let a restaurant manager maintain its profile and menu.
7. Let a restaurant manager accept or reject incoming orders.
8. Give an administrator basic control over restaurants and users.

### Engineering goals

1. Keep the application as a modular monolith.
2. Separate Blazor UI, application services, domain rules, and persistence.
3. Enforce validation and authorization on the server.
4. Use PostgreSQL migrations for every schema change.
5. Test the most important business rules and one complete customer flow.
6. Make the code understandable to a beginner who is learning C# full-stack development.

### Success criteria for the MVP

A test customer can:

1. Create an account and sign in.
2. Browse an approved restaurant.
3. Add menu items to a basket.
4. Submit an order with a delivery address.
5. Refresh the page and still see the order.
6. See the order move from Pending to Accepted or Rejected.

A test restaurant manager can:

1. Sign in.
2. Update the restaurant profile.
3. Add, edit, disable, and remove menu items.
4. Accept or reject a customer order.

## Target Users

### 1. Customer

A person who wants to find food, view a menu, and place a simple delivery or pickup order.

Customer needs:

- A clear restaurant list.
- Useful restaurant and menu information.
- A simple basket.
- A short checkout form.
- A visible order status.
- Useful validation and error messages.

### 2. Restaurant Manager

A person responsible for one restaurant’s profile, menu, and incoming orders.

Manager needs:

- A protected dashboard.
- Menu management.
- Restaurant availability control.
- A list of incoming orders.
- Simple order status actions.

### 3. Administrator

A trusted platform operator who manages the application rather than daily restaurant work.

Administrator needs:

- A protected admin area.
- Restaurant approval and suspension.
- User account visibility.
- Basic audit information.

## Core Feature Scope

### Feature 1: Authentication

- Customer registration.
- Customer login and logout.
- Manager login.
- Admin login.
- Protected pages based on role.
- Passwords and sessions handled by ASP.NET Core Identity.

### Feature 2: Restaurant directory

- List approved restaurants.
- Filter by area and cuisine.
- Search by restaurant name.
- Show restaurant name, image, cuisine, delivery/pickup availability, and open/closed state.
- Show an empty state when no restaurants match.

### Feature 3: Restaurant and menu page

- Show restaurant details.
- Show menu categories.
- Show menu item name, description, price, and availability.
- Prevent unavailable items from being added to the basket.

### Feature 4: Basket

- Add an available menu item.
- Change quantity.
- Remove an item.
- Show subtotal.
- Keep items from only one restaurant in the basket.
- Show an empty basket state.

### Feature 5: Checkout and order creation

- Require a signed-in customer.
- Collect name, phone number, address, and order notes.
- Recalculate prices on the server.
- Create an order and its order items in one database transaction.
- Display an order confirmation page.

Payment is out of scope for the first version. The MVP should record the payment method as `CashOnDelivery` or `PayAtPickup`.

### Feature 6: Customer order history

- List the customer’s orders.
- Show order number, restaurant, total, date, and status.
- Show order details.
- Prevent a customer from viewing another customer’s order.

### Feature 7: Manager dashboard

- View the manager’s restaurant.
- Edit restaurant name, description, address, phone, and operating status.
- Create, edit, disable, and delete menu categories and items.
- View incoming orders.
- Accept or reject an order.
- Mark an accepted order as Preparing, Ready, Completed, or Cancelled.

### Feature 8: Admin controls

- Approve a new restaurant.
- Suspend or reactivate a restaurant.
- View users and their roles.
- View basic order and restaurant records.
- Record important admin actions in an audit log.

## Order Statuses

```text
Pending
Accepted
Rejected
Preparing
Ready
Completed
Cancelled
```

Allowed transitions:

```text
Pending -> Accepted
Pending -> Rejected
Accepted -> Preparing
Preparing -> Ready
Ready -> Completed
Accepted -> Cancelled
Preparing -> Cancelled
```

The server must reject invalid transitions.

## Basic Business Rules

1. Only approved and active restaurants appear in the public directory.
2. A basket can contain items from only one restaurant.
3. Only available menu items can be ordered.
4. Prices are read from the database and recalculated on the server during checkout.
5. A customer can view only their own orders.
6. A manager can manage only their assigned restaurant.
7. An administrator can manage platform records according to admin permissions.
8. Disabled menu items remain in historical orders but cannot be ordered again.
9. An order must contain at least one item.
10. Every order stores the item name and price at the time of purchase so historical orders remain accurate.

## Out of Scope for the MVP

- Online card, mobile-wallet, or gateway payments.
- Delivery rider assignment and live GPS tracking.
- Coupons, loyalty points, and promotions.
- Customer ratings and reviews.
- Chat between customer and restaurant.
- Native Android or iOS applications.
- Multi-country currency support.
- Real-time order tracking.
- Advanced restaurant analytics.
- Microservices and Kubernetes.
- AI recommendations.

## High-Level Pages

### Customer pages

```text
/
/restaurants
/restaurants/{slug}
/basket
/checkout
/orders
/orders/{id}
/login
/register
```

### Manager pages

```text
/manager
/manager/restaurant
/manager/menu
/manager/orders
```

### Admin pages

```text
/admin
/admin/restaurants
/admin/users
/admin/audit-log
```

## Non-Functional Requirements

### Usability

- Responsive at 375px, 768px, and desktop widths.
- Every form has labels, validation, loading, success, and error states.
- Keyboard navigation must work for all important actions.

### Security

- Authentication and authorization are enforced server-side.
- Every order and restaurant query is scoped to the current user or manager.
- All request data is validated before business logic runs.
- Secrets exist only in environment variables.
- Admin actions are audited.

### Reliability

- Order creation is transactional.
- Database changes use EF Core migrations.
- Errors return a friendly message and receive a request ID in logs.

### Performance

- Paginate restaurant, order, and admin lists.
- Add database indexes for foreign keys and common filters.
- Optimize restaurant images.
- Avoid loading the entire menu or order history at once.

## Initial Database Entities

```text
ApplicationUser
Restaurant
RestaurantManager
MenuCategory
MenuItem
Order
OrderItem
DeliveryAddress
AuditLog
```

Recommended relationships:

```text
ApplicationUser 1 ─── * Order
Restaurant 1 ─── * MenuCategory
MenuCategory 1 ─── * MenuItem
Restaurant 1 ─── * Order
Order 1 ─── * OrderItem
MenuItem 1 ─── * OrderItem
ApplicationUser 1 ─── * AuditLog
```

## MVP Delivery Phases

### Phase 1: Foundation

- Create the .NET solution and projects.
- Configure Blazor, ASP.NET Core, EF Core, PostgreSQL, and Identity.
- Add documentation and environment configuration.
- Add health check and database migration.

### Phase 2: Restaurant browsing

- Add restaurant and menu entities.
- Seed sample restaurants and menu items.
- Build restaurant list, detail, and menu pages.

### Phase 3: Basket and checkout

- Implement basket state.
- Implement checkout validation.
- Create orders transactionally.
- Add customer order history.

### Phase 4: Manager operations

- Add manager authorization.
- Build menu CRUD.
- Build order status management.

### Phase 5: Admin operations

- Add restaurant approval.
- Add user and restaurant administration.
- Add audit logging.

### Phase 6: Verification and deployment

- Add unit, integration, and end-to-end tests.
- Add CI pipeline.
- Deploy to preview.
- Perform production QA.
- Deploy production and monitor errors.

## Open Decisions Before Implementation

- Will the MVP support delivery, pickup, or both?
- What single city or service area will be supported first?
- Will customers need an account before adding to the basket, or only before checkout?
- Who changes an order to Ready and Completed?
- What phone-number format is required?
- What is the cancellation policy?
- What image storage provider will be used?
- Which hosting provider and monthly budget are acceptable?

## Definition of Done

A feature is complete only when:

- Its business rule is documented.
- Its server-side validation and authorization exist.
- Its database changes are represented by a migration.
- Its loading, empty, error, and success states work.
- Relevant tests pass.
- `dotnet build` passes.
- `dotnet test` passes.
- The change is reviewed in a small Git commit.
- `TASKS.md` and `MEMORY.md` are updated.
