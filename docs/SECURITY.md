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
