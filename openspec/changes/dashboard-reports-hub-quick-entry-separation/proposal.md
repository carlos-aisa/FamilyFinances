## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not change accounting formulas, sign conventions, or reporting calculations.
- Do not remove any existing detailed report routes under `/reports/*`.
- Do not introduce language selector controls in navigation (language remains in Settings).
- Do not redesign unrelated modules (Accounts, History, Transactions CRUD) in this change.

### Required
- Keep a dedicated quick transaction capture experience, but move it out of `/`.
- Make Dashboard (`/`) the primary reporting entry surface.
- Preserve deep-link compatibility for existing report pages.
- Update Web UI tests and navigation expectations to match the new information architecture.

## Why

The current home page mixes heavy data-entry workflows with analytical report discovery, which increases cognitive load and makes reporting less visible as a daily decision surface. Splitting responsibilities into a reporting-first Dashboard and a dedicated Quick Entry page improves navigation clarity without changing business behavior.

## What Changes

- Reframe Dashboard (`/`) as a report hub that surfaces the main report entry cards and shortcuts.
- Introduce a dedicated `Quick Entry` page (`/quick-entry`) that hosts transaction quick-capture workflows currently shown on Dashboard.
- Update primary navigation to expose `Quick Entry` directly while keeping access to key report destinations.
- Keep existing report detail pages unchanged (`/reports/economic-state`, `/reports/monthly-summary`, `/reports/account-totals`, `/reports/account-group-totals`, etc.).
- Keep `/reports` available as a compatibility entry point (same report hub intent), while Dashboard becomes the primary landing experience.
- Update localization labels and automated UI tests for the new IA (navigation + page intent).

## Capabilities

### New Capabilities
- `quick-entry-workspace`: Defines a dedicated route and UX contract for rapid transaction capture outside of Dashboard.

### Modified Capabilities
- `dashboard-reporting-entry`: Dashboard requirements change from quick-entry-first to reporting-hub-first, with direct navigation to major reports and Quick Entry.
- `economic-state-reporting`: Entry-path requirement is expanded so Economic State must be reachable from the Dashboard report hub (not only from `/reports`).
- `system`: Global navigation IA is updated to include a dedicated Quick Entry destination and Dashboard-as-report-hub behavior.

## Impact

- Frontend pages/components:
  - `src/FamilyFinances.Web/Components/Pages/Dashboard/*`
  - `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor`
  - `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`
- Localization resources:
  - `src/FamilyFinances.Web/Resources/SharedResource*.resx`
- Web tests:
  - `tests/FamilyFinances.Web.Tests/Features/Layout/*`
  - `tests/FamilyFinances.Web.Tests/Features/Dashboard/*`
  - `tests/FamilyFinances.Web.Tests/Features/Reports/*` (navigation-entry expectations only)
- No backend API or database schema changes are expected.

## Non-Goals

- No changes to report data contracts or API endpoints.
- No new financial metrics or chart calculation logic.
- No migration of Settings, Backup/Restore, or localization architecture.
- No broad visual redesign beyond the IA shift required for Dashboard and Quick Entry separation.

## Rollback Plan

- Restore previous Dashboard composition (quick-entry-first layout) and remove `/quick-entry` route usage.
- Restore prior navigation menu structure (including previous report entry placement).
- Keep `/reports/*` detail routes untouched so rollback is UI-structure-only.
- Re-run Web UI regression tests for navigation/report reachability after rollback.
