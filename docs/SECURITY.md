# Authentication security

TASK-007 implements cookie sign-in on the Web host using ASP.NET Core Identity. Login/logout pages use static server rendering because cookies must be written during a normal HTTP request, before response headers are sent. Interactive registration continues to use the Application service.

- Passwords are verified by Identity; there is no custom hashing or credential store.
- Invalid, unknown and locked-out accounts receive the same generic sign-in error. Password fields clear on failed submissions. Passwords and request bodies are not logged.
- Five failed sign-in attempts trigger Identity's 15-minute account lockout.
- Cookies are HttpOnly, SameSite=Lax, expire after eight hours with sliding renewal, and require HTTPS outside Development. The remember option makes the cookie persist across browser restarts within its lifetime.
- Logout requires an authenticated, antiforgery-protected POST. GET only shows confirmation. Standard Identity sign-out clears the current browser cookie; it does not sign out other devices or revoke every previously issued ticket.
- Return URLs must start with a single slash and contain no backslashes/control characters. Invalid destinations fall back to /session.
- /session is a minimal authenticated status page; it is not the account page or a dashboard. TASK-008 adds explicit role entry policies and central order ownership checks; see PERMISSIONS.md.
- Registration provisions only Customer. Manager/Admin roles cannot be selected by the browser.

Production rollout must provision roles, use HTTPS, persist/protect Data Protection keys, and select a key-sharing/session strategy if multiple hosts or replicas are deployed. Web/API cookies are not yet configured for shared authentication across separate hosts. Broader rate limiting, revocation requirements and security review remain the scheduled security/deployment tasks.

Tests cover cookie sign-in/out, anonymous access, local-only redirects, antiforgery rejection on login/logout, persistent cookies, lockout, and real PostgreSQL account persistence in a dedicated `_tests` database. Test credentials are synthetic and disposable.

TASK-009 adds self-service `/account` for any authenticated role. Caller ID comes from signed Identity claims, not the request. Email, roles and phone-confirmation state cannot be edited by overposting form fields. Name is encoded as text by Razor. A concurrency stamp prevents stale-form overwrites. Phone edits use Identity's stamp/verification handling and refresh the current sign-in cookie. Name/phone are contact details only; phone verification and security-sensitive account-change workflows remain separate future work.

TASK-019: browser baskets use an opaque protected session cookie (HttpOnly/Lax, Secure outside Development), server memory scoped by session ID, server canonical item lookup, one-restaurant/quantity limits and antiforgery-protected POST forms. Logout clears this browser's basket. Basket UI responses use no-store. Subtotals are display snapshots only; checkout must re-read prices and availability. Session state is temporary/single-host, not an account-owned durable order or production shared cache.

TASK-022: checkout writes require CustomerAccess, antiforgery, a session-bound request ID, server-side contact/basket validation and a server-held reviewed-quote fingerprint. Customer identity comes only from signed claims. Persistence re-reads catalog eligibility/prices in a Serializable transaction; unique customer/request IDs protect against repeated submission. Receipt lookup is scoped to that customer. Contact/address/notes are stored on committed orders, not in session or logs; errors expose only safe messages. New contact snapshots make production retention/privacy policy necessary before deployment. Rate limiting and payment-provider integration remain later tasks; no online charge occurs here.

TASK-023 order details require authentication and central resource authorization, followed by customer/admin-scoped SQL for metadata and items. Wrong-customer, Manager-only, malformed and missing requests expose no order details; authenticated denied/missing responses use 404. Pages are no-store and Razor encodes contact/notes/item values. Admin access is explicit, not role hierarchy. Manager assignments remain TASK-025.
