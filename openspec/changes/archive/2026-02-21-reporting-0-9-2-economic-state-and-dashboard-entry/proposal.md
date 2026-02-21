## Why

Users need one explicit place to answer "how am I today?" without navigating across multiple report pages. Current reporting pages are powerful but fragmented for a quick economic-state snapshot at a specific date.

## What Changes

- Introduce a dedicated `Economic State` report centered on an as-of date (default: today).
- Expose explicit KPIs for `Assets`, `Liabilities`, `Net Worth`, plus current-period `Income`, `Expense`, and `Period Net Result`.
- Add a direct access entry from the application navigation menu to the new `Economic State` report.
- Ensure labels and formulas follow the canonical semantic contracts introduced in `0.9.1`.

### Non-Goals

- No complex chart set in this change (handled by `0.9.3` and `0.9.4`).
- No multi-currency implementation.
- No redesign of the full dashboard information architecture.

### Rollback Plan

- Keep existing report routes unchanged and add `Economic State` as additive behavior.
- If regressions appear, disable dashboard shortcut and hide the new report card/page while retaining underlying existing reports.
- Revert KPI composition to pre-change routes and values if semantic mismatches are detected.

## Capabilities

### New Capabilities
- `economic-state-reporting`: As-of-date economic state report with explicit stock and flow KPIs in a single page.
- `dashboard-reporting-entry`: Dashboard shortcut card/entry for direct navigation to the economic-state report.

### Modified Capabilities
- `system`: Extend reporting baseline to include the new `Economic State` report behavior and dashboard integration expectations.

## Impact

- Web reporting pages and reports index navigation.
- Dashboard page UI and navigation handlers.
- Reporting service/application queries for assembling unified economic-state read model.
- API and web tests for new route behavior and KPI consistency.
