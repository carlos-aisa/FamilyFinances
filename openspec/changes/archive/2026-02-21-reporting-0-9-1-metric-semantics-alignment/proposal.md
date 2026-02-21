## Why

Reporting currently exposes valid numbers but not always with a single, explicit semantic contract across pages. Users can compare values from different report scopes (monthly summary, monthly evolution, asset total) and interpret them as equivalent even when they measure different accounting dimensions. Before expanding charts and new report views in `0.9.x`, metric semantics must be aligned and made explicit.

## What Changes

- Define a canonical reporting metric taxonomy for `Assets`, `Liabilities`, `Net Worth`, `Income`, `Expense`, and `Period Net Result`.
- Define mandatory formula contracts and naming rules for every KPI card and chart series shown in reporting pages.
- Align existing report outputs so equivalent labels always represent the same formula.
- Add explicit scope labels/warnings where metrics are intentionally different (for example, asset evolution versus period net result).
- Add test coverage for cross-report semantic consistency and regression protection.

### Non-Goals

- No redesign of report page layout and visual style in this change.
- No new chart components in this change.
- No new persistence tables or schema migrations.
- No localization overhaul.

### Rollback Plan

- Keep API routes and DTO shape backward compatible while introducing semantic metadata and label alignment.
- If semantic alignment causes regressions, rollback by restoring previous label mapping in web pages and disabling stricter semantic assertions in tests introduced by this change.
- Re-run full reporting test suite after rollback and keep historical behavior until a corrected semantic mapping is released.

## Capabilities

### New Capabilities
- `reporting-metric-semantics`: Canonical definitions and formulas for reporting metrics, including chart/KPI naming contracts and scope disclaimers.

### Modified Capabilities
- `monthly-balance-evolution-reporting`: Clarify and enforce which summary cards in `Accounts` scope represent asset-only evolution versus all-account ledger netting.
- `system`: Extend baseline reporting requirements with explicit metric-equivalence and non-equivalence rules across report pages.

## Impact

- Affected Web pages: `MonthlyEvolution`, `MonthlySummary`, reports index descriptors, and future chart labels.
- Affected tests: Web report feature tests, reporting API contract tests, and application-level reporting tests.
- Affected documentation/specs: `openspec/specs/system/spec.md`, `openspec/specs/monthly-balance-evolution-reporting/spec.md`, plus new capability spec for metric semantics.
