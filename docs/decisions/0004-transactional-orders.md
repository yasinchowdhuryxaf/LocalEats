# ADR 0004: Transactional cash orders

Status: accepted for TASK-022, implementing the planned cash-order MVP.

## Flow and boundaries

Customer-only static SSR checkout first reviews the canonical basket. Review stores only a SHA-256 fingerprint of item IDs, names, quantities, prices and restaurant name in the browser session. An explicit Place order POST submits the session-bound request ID. Contact fields are validated again; the signed-in claim supplies customer identity. Browser totals and restaurant IDs are ignored.

Application owns validation and canonical summary calculations. Infrastructure uses a fresh scoped EF context and a PostgreSQL Serializable transaction to re-read current public/open restaurant, active category and available item data. A changed quote requires review again. It saves the order header and purchased item snapshots, then commits both together. A failed item save rolls back even an already-saved header.

The total is the item subtotal in BDT minor units. This development cash-order slice has a zero delivery fee and collects no online payment. Delivery-area eligibility, taxes and payment-provider integration require later explicit product decisions. New orders are Pending; CashOnDelivery requires an address and PayAtPickup permits none.

## Replay and concurrency

Each new checkout GET receives a new request GUID. A unique database index on CustomerId/RequestId prevents duplicate orders. An existing request returns only its own customer's receipt, even after the basket is cleared. Serialization, deadlock and uniqueness races retry up to three times with new scopes/transactions. Unexpected or exhausted failures reach the safe checkout error handler.

A successful request redirects to a minimal receipt on /checkout?placed=true. Replaying a completed request does not clear a subsequently added basket. The session retains the request ID so a retry after a lost response can recover the committed order. A new checkout invalidates older form tokens. This is intentionally a single-active-checkout-per-session design; it does not provide multi-tab independent drafts or distributed session storage.

## Persistence and legacy data

Order contact name, phone, delivery address, notes, payment method and request ID are nullable in the migration to preserve legacy rows without inventing data. New application orders require validated contact/method values. Purchased names and integer prices live on OrderItem, preserving history after catalog edits. Order numbers use a random GUID with an LE- prefix and the existing unique constraint.

CustomerId now references Identity users with SET NULL on deletion, preserving order records. No account-deletion or retention workflow is introduced here; production retention, privacy and deletion policy must be defined before deployment. Migration tooling remains restricted to the explicitly supplied LocalEats endpoint.

The receipt is not the full order-details page. TASK-023 adds /orders/{id} and its resource authorization; TASK-024 adds customer history. Browser baskets remain temporary as documented in ADR 0003.
