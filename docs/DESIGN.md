# Design System
# Project: LocalEats

## 1. Design Direction

LocalEats should feel welcoming, clear, trustworthy, and practical. The interface should help a customer make a food decision quickly while giving restaurant managers simple operational screens.

The design is intentionally light and focused. It should look like a dependable local food service rather than a crowded marketplace.

### Primary design goals

- Make restaurant discovery fast.
- Make menu and price information easy to scan.
- Make checkout short and predictable.
- Make order status obvious.
- Keep manager and admin screens functional and uncluttered.
- Use the same components and spacing throughout the application.

## 2. Design Principles

### 2.1 Clarity before decoration

Every screen should have one clear primary action. Avoid decorative elements that compete with restaurant names, menu items, prices, or order status.

### 2.2 Content first

Use real content hierarchy:

```text
Restaurant name
Cuisine and location
Opening/status information
Menu categories
Menu items and prices
Primary action
```

Do not hide important information behind unnecessary clicks.

### 2.3 Consistency

- Use the same button styles everywhere.
- Use the same input, card, table, alert, and status components throughout the application.
- Use design tokens instead of page-specific colors or spacing.
- Reuse existing components before creating a new variation.

### 2.4 Strong feedback

Every user action should provide feedback:

- Loading state while work is in progress.
- Inline validation for invalid fields.
- Success message after a completed action.
- Friendly error message when work fails.
- Empty state when no data exists.

### 2.5 Mobile-first layout

The customer experience must work at 375px wide before adding desktop enhancements. Manager and admin tables should remain usable on small screens through responsive tables, horizontal scrolling, or stacked cards.

### 2.6 Accessible by default

- Use semantic HTML elements.
- Every form input has a visible label.
- Every interactive element is keyboard accessible.
- Focus states are visible.
- Color is never the only way to communicate meaning.
- Text and controls meet WCAG AA contrast targets.
- Use ARIA only when native HTML does not provide the required meaning.

### 2.7 Trust and transparency

Show prices, fees, restaurant status, order status, and error messages clearly. Never surprise the customer with a changed total at the final step.

## 3. Color Palette

Use CSS variables or Tailwind theme tokens. Do not hard-code colors inside individual components.

### 3.1 Brand colors

| Token | Hex | Use |
|---|---|---|
| `brand-50` | `#FFF7ED` | Soft brand background |
| `brand-100` | `#FFEDD5` | Highlight background |
| `brand-500` | `#F97316` | Primary brand accent |
| `brand-600` | `#EA580C` | Primary button hover/active |
| `brand-700` | `#C2410C` | Dark brand text or pressed state |

The orange brand family suggests food and energy while remaining distinct from status colors.

### 3.2 Neutral colors

| Token | Hex | Use |
|---|---|---|
| `neutral-0` | `#FFFFFF` | Cards and page surfaces |
| `neutral-50` | `#FAFAF9` | Application background |
| `neutral-100` | `#F5F5F4` | Subtle panels and input backgrounds |
| `neutral-200` | `#E7E5E4` | Borders and dividers |
| `neutral-500` | `#78716C` | Secondary text |
| `neutral-700` | `#44403C` | Body text |
| `neutral-900` | `#1C1917` | Headings and primary text |

### 3.3 Semantic colors

| Token | Hex | Use |
|---|---|---|
| `success-50` | `#F0FDF4` | Success background |
| `success-600` | `#16A34A` | Success text and indicators |
| `warning-50` | `#FFFBEB` | Warning background |
| `warning-600` | `#D97706` | Warning text and indicators |
| `danger-50` | `#FEF2F2` | Error background |
| `danger-600` | `#DC2626` | Error text and destructive actions |
| `info-50` | `#EFF6FF` | Informational background |
| `info-600` | `#2563EB` | Informational text and links |

### 3.4 Color usage rules

- Use `brand-500` for the main call to action, such as `Add to basket` or `Place order`.
- Use `brand-600` for hover and active states.
- Use `danger-600` only for destructive actions or serious errors.
- Use status text together with a badge, icon, or label; never rely on color alone.
- Keep body text on white or `neutral-50` backgrounds.
- Do not introduce a new color for a single page.

## 4. Typography

### 4.1 Font family

Use a clean system sans-serif stack for the first version:

```css
font-family: Inter, ui-sans-serif, system-ui, -apple-system,
  BlinkMacSystemFont, "Segoe UI", sans-serif;
```

If Inter is not available, the system fallback is acceptable. Do not download a font solely for a minor visual difference.

### 4.2 Type scale

| Token | Size | Line height | Weight | Use |
|---|---:|---:|---:|---|
| `display` | 36px | 44px | 700 | Homepage hero on desktop |
| `h1` | 30px | 38px | 700 | Page title |
| `h2` | 24px | 32px | 700 | Section title |
| `h3` | 20px | 28px | 600 | Card or panel title |
| `body-lg` | 18px | 28px | 400 | Introductory text |
| `body` | 16px | 24px | 400 | Default content |
| `body-sm` | 14px | 20px | 400 | Secondary information |
| `label` | 14px | 20px | 600 | Form labels and table headers |
| `caption` | 12px | 16px | 400 | Supporting metadata |
| `button` | 14px | 20px | 600 | Button text |

### 4.3 Typography rules

- Use one primary font family.
- Use weight and size to create hierarchy; do not use excessive uppercase text.
- Keep paragraphs between 45 and 80 characters per line on large screens.
- Use sentence case for labels, headings, and buttons.
- Restaurant names and menu item names should be visually stronger than descriptions.
- Prices must be easy to find and align consistently.

## 5. Spacing and Layout

### 5.1 Spacing scale

Use a 4px base unit:

| Token | Value | Typical use |
|---|---:|---|
| `space-1` | 4px | Icon/text gap |
| `space-2` | 8px | Tight internal gap |
| `space-3` | 12px | Compact field spacing |
| `space-4` | 16px | Default component gap |
| `space-5` | 20px | Card internal spacing |
| `space-6` | 24px | Related content groups |
| `space-8` | 32px | Section spacing |
| `space-10` | 40px | Large section spacing |
| `space-12` | 48px | Page section separation |
| `space-16` | 64px | Hero or major page spacing |

### 5.2 Layout widths

- Mobile page padding: 16px.
- Tablet page padding: 24px.
- Desktop page padding: 32px.
- Maximum content width: 1200px.
- Reading-width content: 720px.
- Customer restaurant grid: 1 column mobile, 2 columns tablet, 3 or 4 columns desktop.
- Checkout content: two columns on desktop, one column on mobile.

### 5.3 Grid and gaps

- Use a 12-column grid for desktop layouts where useful.
- Use 16px gaps for compact cards.
- Use 24px gaps between major sections.
- Do not create dense layouts that make food images, prices, or status labels difficult to scan.

### 5.4 Borders and elevation

- Default border: `1px solid neutral-200`.
- Card radius: 12px.
- Input radius: 8px.
- Button radius: 8px.
- Modal radius: 16px.
- Use a soft shadow only for cards that need separation from the page background.
- Avoid heavy gradients, strong outlines, and excessive shadows.

## 6. Website UI Components

### 6.1 Application shell

#### Header

The header should include:

- LocalEats logo or wordmark.
- Restaurant search or location entry where appropriate.
- Basket link with item count.
- Login/account link.
- Responsive mobile menu.

#### Footer

The footer should include:

- Short product description.
- Customer support/contact link.
- Restaurant manager link.
- Privacy and terms links when available.

#### Page container

All standard pages use a centered container with responsive horizontal padding and a maximum width of 1200px.

### 6.2 Buttons

Variants:

- `Primary`: main action, brand background.
- `Secondary`: supporting action, neutral border.
- `Tertiary`: low-emphasis action, text only.
- `Destructive`: delete, suspend, or cancel actions.
- `Icon`: compact action with an accessible label.

States:

- Default.
- Hover.
- Focus-visible.
- Pressed.
- Disabled.
- Loading.

Rules:

- Use action verbs: `Add to basket`, `Place order`, `Accept order`.
- Do not use `Submit` when a more specific label is possible.
- Show a spinner or progress indicator while a button action is pending.

### 6.3 Restaurant card

Must display:

- Restaurant image or accessible placeholder.
- Restaurant name.
- Cuisine or category.
- Area or delivery location.
- Open/closed status.
- Optional delivery or pickup label.

Behavior:

- The whole card may link to the restaurant detail page.
- The card must have a visible focus state.
- Avoid placing multiple competing primary buttons on one card.

### 6.4 Restaurant detail header

Must display:

- Restaurant name.
- Description.
- Location.
- Opening status and hours.
- Cuisine.
- Restaurant image.

Use a sticky or clearly visible basket summary on desktop where it improves checkout discovery.

### 6.5 Menu category and menu item

Menu category:

- Clear category heading.
- Optional item count.
- Consistent vertical spacing.

Menu item:

- Name.
- Short description.
- Price.
- Availability state.
- Optional image.
- `Add to basket` action.

Unavailable items should remain understandable but have a disabled action and clear `Unavailable` label.

### 6.6 Basket

The basket should show:

- Restaurant name.
- Item name and quantity.
- Quantity controls.
- Item price and line total.
- Subtotal.
- Delivery or pickup information if applicable.
- Primary checkout action.

On mobile, the basket may be a full page. On desktop, it may be a side panel or dedicated summary section.

### 6.7 Checkout form

Fields:

- Customer name.
- Phone number.
- Delivery address or pickup option.
- Order notes.
- Payment method label for the MVP.

Rules:

- Group related fields.
- Show required fields clearly.
- Keep the final total visible.
- Do not change prices on the client without server confirmation.

### 6.8 Order status

Use a status badge with text and an optional icon:

| Status | Meaning | Suggested visual |
|---|---|---|
| Pending | Restaurant has not responded | Warning |
| Accepted | Restaurant accepted the order | Info |
| Rejected | Restaurant rejected the order | Danger |
| Preparing | Restaurant is preparing it | Brand |
| Ready | Ready for pickup or delivery | Success |
| Completed | Order is finished | Success |
| Cancelled | Order will not continue | Neutral or danger |

Do not rely on color alone. Always show the status name.

### 6.9 Forms and inputs

Inputs must include:

- Visible label.
- Helpful placeholder only when necessary.
- Required indicator where applicable.
- Inline validation message.
- Focus-visible style.
- Disabled and loading states.

Use the correct input type for email, phone, number, password, and date/time.

### 6.10 Alerts and notifications

Variants:

- Success.
- Information.
- Warning.
- Error.

An alert should contain:

- Clear title or message.
- Optional supporting detail.
- Optional action such as `Retry`.
- Dismiss control only when dismissal is safe.

Use toast notifications for small completed actions. Use inline alerts for errors that require the user to understand or correct something.

### 6.11 Tables and manager screens

Manager and admin tables should support:

- Clear column headings.
- Status badges.
- Row actions.
- Empty state.
- Loading skeleton.
- Error state with retry.
- Mobile overflow or stacked-card layout.
- Pagination for large lists.

Do not hide critical actions only inside an inaccessible hover interaction.

### 6.12 Empty states

An empty state should explain:

1. What is empty.
2. Why it may be empty.
3. What the user can do next.

Examples:

- `Your basket is empty. Browse restaurants to add your first item.`
- `No orders yet. Your completed orders will appear here.`
- `No menu items yet. Add a category to start building your menu.`

### 6.13 Loading states

Use:

- Skeleton cards for restaurant lists.
- Skeleton rows for manager tables.
- Button progress indicators for form actions.
- A clear page-level loading state for initial route data.

Do not display a blank page while waiting for a request.

### 6.14 Error states

Error messages should:

- Explain what failed in plain language.
- Give the user a next action when possible.
- Include `Try again` for recoverable read failures.
- Never expose exception details or database errors.

### 6.15 Modal and confirmation dialog

Use a modal only for a focused decision or short form.

- Trap focus while open.
- Close with Escape when safe.
- Restore focus to the triggering element.
- Use a clear title and action labels.
- Require confirmation for destructive actions such as deleting a menu item or suspending a restaurant.

## 7. Page-Specific Design Guidance

### 7.1 Homepage

Primary goal: guide the customer into restaurant discovery.

Suggested structure:

```text
Header
Hero/search area
Popular or featured restaurants
Cuisine/category shortcuts
Short explanation of how ordering works
Footer
```

Keep the hero short. The primary action should be searching or browsing restaurants.

### 7.2 Restaurant listing

Primary goal: compare restaurants quickly.

- Keep filters visible and understandable.
- Show result count when useful.
- Preserve filter state during navigation.
- Provide an empty state for no matches.

### 7.3 Restaurant detail

Primary goal: choose menu items.

- Keep restaurant identity visible.
- Group menu items by category.
- Make prices and add actions easy to scan.
- Keep basket access visible.

### 7.4 Checkout

Primary goal: submit a correct order.

- Minimize distractions.
- Show basket summary and total.
- Show validation next to the relevant field.
- Make the final action explicit: `Place order`.

### 7.5 Manager dashboard

Primary goal: complete daily work quickly.

- Use a clear sidebar or tab navigation.
- Show pending orders prominently.
- Separate configuration screens from active order work.
- Use tables on desktop and cards on mobile.

## 8. Responsive Breakpoints

Use the project’s CSS framework defaults unless a component needs a documented exception.

| Breakpoint | Width | Behavior |
|---|---:|---|
| Small | < 640px | Single column, compact navigation, full-page basket |
| Medium | 640px–1023px | Two-column cards where useful, expanded spacing |
| Large | ≥ 1024px | Desktop navigation, multi-column restaurant grid, two-column checkout |

Test at:

- 375px mobile.
- 768px tablet.
- 1024px small desktop.
- 1440px large desktop.

## 9. Accessibility Requirements

- All images have useful alternative text or are marked decorative.
- All form controls have labels.
- Heading order is logical.
- Keyboard focus is visible.
- Buttons and links are distinguishable.
- Dialogs manage focus correctly.
- Status text is readable without color perception.
- Touch targets are large enough for comfortable mobile use.
- Automated accessibility checks run on core pages.
- Manual keyboard testing is performed before release.

## 10. Design Implementation Rules

1. Build shared tokens before page-specific styling.
2. Build shared components before duplicating markup.
3. Do not introduce one-off colors or spacing values without updating this file.
4. Do not use a component variant for only one accidental visual difference; decide whether the variant is reusable.
5. Keep content and business rules out of presentational components where possible.
6. Use real loading, empty, error, and success states during development.
7. Test every new component on mobile and desktop.
8. Review contrast, keyboard navigation, and focus states.
9. Do not expose secrets or internal identifiers in visible UI.
10. Update this document when the visual system changes.

## 11. Design Definition of Done

A screen is complete when:

- It follows the color, typography, spacing, and component tokens.
- It works at mobile, tablet, and desktop widths.
- It includes loading, empty, error, and success states where applicable.
- Forms include validation, disabled states, and success feedback.
- Keyboard navigation and visible focus work.
- Text and controls meet contrast requirements.
- It uses shared components rather than duplicated UI patterns.
- It has been reviewed against the relevant PRD user flow.
