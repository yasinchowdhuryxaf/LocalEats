# Browser-session basket state

Date: 2026-10-29. TASK-019.

Customers may build an anonymous basket before signing in; checkout will require the Customer policy in its own task. The basket contains only public catalog snapshots, not addresses/payment data. It belongs to the browser session, not an account. Logout clears it.

Application owns BasketState and BasketService. Infrastructure reads current public product/restaurant/category availability and canonical prices. Web binds state to an opaque protected ASP.NET Core session cookie and stores the BasketState in its own process memory cache, never in a shared singleton basket. Mutations/snapshots are locked; creation of cached session states is serialized. No client price, restaurant identity or basket owner is trusted.

Proposed MVP limits: 20 units per item and 50 distinct items. Price bounds prevent integer overflow. The last decrement removes the line; removing the final line resets the restaurant restriction. Adding/increasing re-reads server availability and updates that line's display price. Reductions/removal remain possible after availability changes. Final checkout must re-read all prices/availability transactionally.

This is temporary single-host state with a 30-minute sliding inactivity expiry. Refresh and navigation preserve it, but server restart/eviction clears it. It is not durable or shared across app instances. Before multi-instance hosting, implement a shared atomic state store behind IBasketStateStore. Existing distributed-memory session/cache APIs are provided by ASP.NET Core; no extra package was added.

Cookie LocalEats.BasketSession is HttpOnly, SameSite=Lax and Secure outside Development. Session-specific UI responses use no-store caching. All basket writes use server-rendered EditForms with antiforgery and POST/redirect/GET. No new public JSON endpoint or database schema is introduced. The compact menu preview makes state testable; the dedicated /basket page and checkout remain TASK-020 onward.

The user explicitly chose to keep existing sample restaurants' closed statuses unchanged for this demonstration. Open/available basket flows are exercised in isolated test fixtures; no development catalog status is changed.
