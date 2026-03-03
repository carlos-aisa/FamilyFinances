## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not add report shortcut cards or report-navigation CTA blocks to Dashboard.
- Do not use tabbed containers in the Dashboard analytical layout.
- Do not change accounting formulas, sign conventions, or reporting semantics.
- Do not remove or rename existing `/reports/*` routes.
- Do not introduce language selector controls in navigation (language remains in Settings).

### Required
- Dashboard must present household financial status at a glance, centered on the selected/current month and previous month comparison.
- Dashboard must be chart-first, minimizing dense/growing tables in the primary cockpit area.
- Move quick transaction capture workflows out of Dashboard into a dedicated `Quick Entry` workspace route.
- Keep report discovery/navigation in the main menu (`Reports`) instead of Dashboard shortcut cards.
- Include Accounts and Reports presentation improvements requested in `reports-and-accounts-presentation-refresh`.
- Desktop layout target is 2560x1440 with no vertical scroll for the primary dashboard analytical surface in current scope.
- Keep multi-resolution adaptation (font/chart scaling across additional resolutions) out of this iteration; it will be handled by a separate pending OpenSpec change.
- Year-over-year comparison must be prepared for future data availability with explicit data-sufficiency states.

## Why

Current scope was split across two drafts: one for Dashboard/Quick Entry information architecture and one for Reports/Accounts presentation depth. A single change is needed to unify both intents: Dashboard as a household financial cockpit, Quick Entry as a dedicated capture workspace, and report/account views improved for decision-ready analysis.

## What Changes

- Redefine Dashboard (`/`) as an at-a-glance household financial overview surface (not a report-links hub), with no tabs in dashboard composition.
- Introduce a dedicated `Quick Entry` page (`/quick-entry`) that hosts current rapid transaction capture workflows moved out of Dashboard.
- Keep `Reports` in main navigation as the deep-dive entry path; Dashboard must not duplicate report shortcut cards.
- Add a compact dashboard KPI strip for current month and previous month deltas, including at least: Income, Expense, Net Result (`Income - Expense`), and Net Worth.
- Add key dashboard analytical blocks (Option 1 visual composition):
  - month-focused `Income vs Expense` line chart,
  - annual `Income vs Expense` month-result bar chart,
  - monthly net-balance line chart (`Income - Expense`) replacing growing month tables,
  - asset total evolution chart,
  - annual account-group evolution chart,
  - expense composition pie chart with `Top N` contributors (`N=8..10`) plus aggregated `Others`.
- Add explicit data-sufficiency states for limited history and future-ready hooks for same-month last-year comparison.
- Update the Accounts list view to show both accumulated balance and selected-period balance (current month for this iteration).
- Update Economic State report with an `Expenses` evolution tab symmetric to existing Asset/Income evolution tabs.
- Update Economic State summary to include:
  - monthly net list (`Income - Expense`),
  - month-focused `Income vs Expense` chart,
  - annual month-by-month `Income vs Expense` bar comparison.
- Update Account Totals (period totals) to:
  - default order by net change inside each nature group,
  - allow user-driven sorting by clicking table headers.
- Update Account Group State Evolution to:
  - show exact selected-month balance in list,
  - render annual evolution as month-result bars (non-cumulative),
  - rebalance list/chart width for chart readability,
  - remove or relocate low-value comparability info card that reduces analysis area.
- Add/update localization and automated tests for new dashboard contracts, quick-entry route separation, sorting behavior, and revised chart semantics.

## Capabilities

### New Capabilities
- `dashboard-household-financial-overview`: Defines the dashboard analytical contract (KPI strip, chart-first cockpit blocks, no-tabs layout, and data-sufficiency states).
- `quick-entry-workspace`: Defines a dedicated route and UX contract for rapid transaction capture outside Dashboard.
- `accounts-balance-presentation`: Defines dual-balance presentation (accumulated + selected period) on the Accounts view.

### Modified Capabilities
- `dashboard-reporting-entry`: Dashboard requirements shift to analytics-first financial cockpit behavior and remove quick-entry-first composition.
- `economic-state-reporting`: Add expense evolution parity and expanded summary requirements (monthly net list + annual Income vs Expense bars).
- `monthly-balance-evolution-reporting`: Extend scope support for expense-total evolution and enforce exact selected-month balance semantics in group evolution list context.
- `monthly-reporting-charts`: Monthly `Income vs Expense` chart behavior is extended to dashboard usage under constrained fixed-height layout.
- `annual-reporting-charts`: Add annual Income vs Expense comparison bars and migrate group evolution annual visualization to bar-based month results.
- `reporting-insights`: Dashboard usage is focused on chart-oriented insight aggregation, including expense Top-N plus `Others` composition.
- `system`: Navigation IA is updated to include `Quick Entry` destination while keeping report access in menu and language controls in Settings only.

## Impact

- Affected frontend areas:
  - `src/FamilyFinances.Web/Components/Pages/Dashboard/*`
  - `src/FamilyFinances.Web/Components/Pages/QuickEntry/*` (or equivalent route host components)
  - `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`
  - `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/AccountTotalsPage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor`
  - shared reporting chart components/styles used by dashboard cards
  - dashboard-related layout CSS and responsive density rules
- Potential backend/application impact:
  - aggregation/query composition for current-vs-previous month dashboard snapshot, monthly net trend, and expense Top-N plus `Others` composition datasets
  - monthly evolution scope parsing/mapping for expense-total where not already covered
  - possible reuse/extension of existing reporting endpoints for dashboard datasets
- Affected tests:
  - dashboard UI tests (layout contract, no-tabs assertion, data-sufficiency states, at-a-glance cards)
  - navigation tests for `Quick Entry` route and dashboard/report separation
  - Accounts tests for dual-balance rendering
  - report/chart tests for expense tab, annual bar semantics, selected-month balance list semantics, and sortable headers
- Process impact:
  - this proposal supersedes overlap from `reports-and-accounts-presentation-refresh` and `dashboard-reports-hub-quick-entry-separation`.

## Non-Goals

- No migration of full detailed report tables into Dashboard (chart-first glance-oriented cockpit).
- No advanced/professional forecasting models.
- No replacement of existing report pages as deep-dive analysis surfaces.
- No requirement to guarantee zero scroll on every viewport; target is no-scroll on the current baseline desktop (2560x1440).

## Rollback Plan

- Keep dashboard analytical blocks and quick-entry route wiring behind dedicated composition boundaries for fast reversal.
- Restore previous dashboard composition and quick-entry placement if regressions appear.
- Revert Accounts/Reports presentation changes incrementally (expense tab, bar visualizations, sortable headers, selected-month balance list update) while preserving endpoint compatibility.
- Keep `/reports/*` routes unchanged so rollback can remain UI-structure-focused.
- Re-run dashboard/accounts/reports web tests and reporting API tests after rollback to confirm baseline behavior.
