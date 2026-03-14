## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not introduce new visual behavior requirements that were not validated in `ui-full-views-review`.
- Do not ship page-specific one-off style patches as the long-term solution.
- Do not duplicate style source-of-truth files without documented ownership.
- Do not hardcode new chart color/style constants in page components.

### Required
- Remove as many hardcoded UI values as practical and replace them with shared tokens/primitives.
- Keep visual output functionally equivalent to the approved UX baseline from prior changes, except for explicitly documented month-focused chart baseline normalization.
- Establish explicit style ownership and consumption rules across shared CSS, component CSS, and chart config paths.
- Add regression tests/checks that protect against reintroducing hardcoded style drift.

## Why

The current frontend still contains many hardcoded visual values (sizes, spacing, chart dimensions, color hex values, inline styles), which causes inconsistency and slows future UI changes. A dedicated normalization change is needed to make the entire application uniformly maintainable and predictable.

## What Changes

- Inventory and replace hardcoded style values in Razor markup, shared CSS, component CSS, and chart setup code.
- Expand tokenized style contracts so all primary surfaces consume common values for:
  - typography scales,
  - spacing and paddings,
  - border radius,
  - control heights,
  - table density,
  - chart panel and legend dimensions,
  - chart semantic color mappings.
- Consolidate style source-of-truth strategy and remove conflicting duplicated rules.
- Replace page-level chart color literals with shared semantic palette resolution.
- Introduce guardrails (tests or static checks) to detect regressions such as new inline hardcoded styles or duplicate style blocks.
- Keep runtime behavior unchanged for token/style refactors, with one explicit chart semantic adjustment:
  - month-focused daily evolution charts are normalized to month opening balance (day-by-day evolution starts at 0),
  - preserving day-1 transaction impact.
- Apply one additive reporting contract extension required by that normalization:
  - include `OpeningBalanceCents` in monthly chart DTOs (`MonthlyBalanceChartDto`, `MonthlyChartSeriesDto`) without breaking existing clients.

## Capabilities

### New Capabilities
- `global-ui-token-governance`: Defines application-wide token governance, style ownership boundaries, and anti-hardcode enforcement rules.

### Modified Capabilities
- `premium-frontend-design-system`: Token contract scope is expanded from dashboard/reports focus to full-application coverage.
- `system`: Global presentation consistency requirements are updated to require tokenized style consumption over hardcoded values.
- `monthly-reporting-charts`: Monthly chart presentation must consume shared tokens and semantic palette mappings.
- `annual-reporting-charts`: Annual chart presentation must consume shared tokens and semantic palette mappings.

## Impact

- Affected frontend assets and style layers:
  - `src/FamilyFinances.Web/wwwroot/css/app.css`
  - `src/FamilyFinances.Web/wwwroot/css/premium-theme.css`
  - `src/FamilyFinances.Web/wwwroot/app.css`
  - component-level `.razor.css` files
  - `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`
- Affected Razor components that currently rely on inline or literal style values across dashboard, reports, accounts, payees, quick-entry, history, settings, and login pages.
- Affected chart model/palette composition paths in:
  - `src/FamilyFinances.Web/Features/Reports/Charts/*`
  - report/dashboard page components that currently set `ColorHex` literals.
- Affected test surface:
  - frontend layout/presentation tests,
  - chart rendering payload assertions,
  - new guard tests for hardcoded style regressions.

## Non-Goals

- No new UX features or flow redesign.
- No date-filter semantic changes.
- No backend domain model changes.
- No breaking API behavior changes (only additive `OpeningBalanceCents` metadata in monthly chart DTOs).
- No change in approved user-facing wording unless required for token/system consistency.

## Rollback Plan

- Preserve previous style snapshots and revert token-consumption refactors by module if regressions are detected.
- Re-enable prior chart literal style paths temporarily if tokenized chart rendering causes visual breakage.
- Roll back style ownership consolidation in controlled steps (shared styles first, then component-scoped overrides).
- Re-run visual regression-oriented web tests and reporting smoke tests after rollback to verify baseline restoration.
