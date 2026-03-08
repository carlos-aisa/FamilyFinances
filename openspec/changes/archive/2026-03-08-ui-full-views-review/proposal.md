## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not implement global date-filter semantic changes (`end date inclusive`, global auto-apply, global reset/apply removal) in this change.
- Do not start hardcode-to-token normalization refactors across the codebase in this change.
- Do not introduce breaking API contract changes for existing reporting endpoints.
- Do not bypass current layered architecture boundaries.

### Required
- Apply a full UX and presentation review to all application views, including dashboard, reports, accounts, transactions, quick entry, payees, history, and login.
- Keep behavior changes in scope only when they are view interaction continuity improvements (for example, return-to-origin flows and report table drilldowns).
- Keep documentation and automated tests aligned with all changed UI/UX behaviors.
- Keep content and terminology consistent and user-facing copy clear in every updated view.

## Why

The current UI has visible inconsistency between views, repeated friction in navigation flows, and uneven report/chart presentation patterns that reduce day-to-day usability. A full cross-view review is needed now to establish a coherent, user-validated baseline before semantic filter changes and later hardcode normalization.

## What Changes

- Standardize visual presentation patterns across all views (button style consistency, section messaging consistency, list/card consistency, and layout coherence).
- Update dashboard and report copy/labels where wording is misleading or ambiguous (for example, month-context wording and report naming clarity).
- Improve accounts and quick-entry interaction patterns with accordion behavior, account discoverability, and clearer contextual guidance.
- Improve transaction-edit continuity so users return to the originating view context (accounts/history/report paths) instead of being redirected to unrelated default pages.
- Update payee and transaction presentation patterns (payee list layout, payee visibility as table column, and cleaner description content).
- Refine report interactions:
  - table row drilldowns to ledger movements where requested,
  - sortable report tables where needed,
  - revised report layout composition for readability,
  - aligned monthly evolution chart style across reporting surfaces,
  - improved annual chart readability with current-period cutoff treatment.
- Update Economic State evolution tabs with requested list/column adjustments and additional composition chart placement.
- Add login UX improvement to remember the last successful username in the login form.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `system`: Cross-view presentation, navigation continuity, and login UX requirements are updated for all major screens.
- `dashboard-household-financial-overview`: Dashboard copy, month-context messaging, and surface presentation alignment are updated.
- `quick-entry-workspace`: Quick-entry account discovery, contextual guidance placement, and shared date persistence behavior are updated.
- `accounts-balance-presentation`: Accounts view interaction model is updated to single-open accordion behavior plus updated as-of messaging placement.
- `historical-ledger-views`: Historical and account-origin transaction edit navigation must preserve origin context on return.
- `economic-state-reporting`: Evolution-tab table structure, month visibility rules, composition panel requirements, and layout rules are updated.
- `monthly-reporting-charts`: Monthly evolution charts must follow one consistent visual and interaction pattern across reporting pages.
- `annual-reporting-charts`: Annual evolution charts must follow a consistent current-period cutoff/readability pattern aligned with monthly chart behavior.
- `dashboard-reporting-entry`: Report naming and entry semantics are updated where report identity text changes.

## Impact

- Affected frontend components include:
  - `src/FamilyFinances.Web/Components/Pages/Dashboard/*`
  - `src/FamilyFinances.Web/Components/Pages/Reports/*`
  - `src/FamilyFinances.Web/Components/Pages/Accounts/*`
  - `src/FamilyFinances.Web/Components/Pages/Transactions/*`
  - `src/FamilyFinances.Web/Components/Pages/QuickEntry/*`
  - `src/FamilyFinances.Web/Components/Pages/Payees/*`
  - `src/FamilyFinances.Web/Components/Pages/History/*`
  - `src/FamilyFinances.Web/Components/Pages/Login/*`
- Affected shared styles and chart rendering:
  - `src/FamilyFinances.Web/wwwroot/css/app.css`
  - `src/FamilyFinances.Web/wwwroot/css/premium-theme.css`
  - `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`
- Affected web tests under `tests/FamilyFinances.Web.Tests/Features/*` with updated assertions for navigation continuity, layout, report interactions, and chart consistency.
- No planned backend schema migration in this change.

## Non-Goals

- No global date-range semantic migration in this change (handled by `global-filter-behavior-semantics`).
- No app-wide token/hardcode normalization refactor in this change (handled by `ui-hardcode-normalization`).
- No report-calculation formula changes to financial totals or metric semantics.
- No authentication architecture change beyond remembering the last successful username for login UX.

## Rollback Plan

- Revert changed view-level components and CSS blocks in small batches by area (dashboard, reports, accounts, quick entry, payees, login).
- Disable newly introduced drilldown/return-context routes behind conservative fallbacks if regressions are found.
- Restore prior chart layout placement and legacy report table rendering if readability regressions appear.
- Re-run web and reporting-focused test suites after rollback to confirm baseline behavior restoration.
