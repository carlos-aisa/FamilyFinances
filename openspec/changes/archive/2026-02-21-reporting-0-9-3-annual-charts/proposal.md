## Why

The current reports are table-first and require cognitive effort to identify trends over the year. Users explicitly requested yearly visualizations for balance evolution and composition percentages.

## What Changes

- Add annual chart visualizations to reporting pages using existing yearly report data.
- Implement charts for:
  - Asset total end-balance evolution.
  - Evolution by account groups.
  - Expense-oriented account-group composition.
  - Accounts composition by nature (`Expense` / `Income`).
- Keep table data as source of truth and place charts above corresponding tables.
- Ensure chart labels use canonical semantics from `0.9.1`.

### Non-Goals

- No monthly/daily charts in this change (handled in `0.9.4`).
- No predictive analytics or anomaly detection (handled in `0.9.5`).
- No replacement of existing tables.

### Rollback Plan

- Keep chart rendering as additive UI behavior with feature toggle fallback to table-only mode.
- If chart regressions appear, disable chart sections while preserving existing report functionality.
- Revert only chart components and bindings; leave reporting queries/endpoints untouched.

## Capabilities

### New Capabilities
- `annual-reporting-charts`: Year-level chart rendering for asset/account/account-group evolution and supported composition views.

### Modified Capabilities
- `monthly-balance-evolution-reporting`: Extend state evolution UI behavior with annual chart panels tied to selected scope/year.
- `system`: Extend reporting system behavior with annual chart availability and table-to-chart consistency requirements.

## Impact

- Blazor reporting pages (`EconomicState`, `AccountTotals`, `AccountGroupTotals`), chart components, and UI styling.
- Web-side data transformation/adapters from DTO points to chart datasets.
- Frontend tests for chart presence, dataset binding, and fallback behavior.
