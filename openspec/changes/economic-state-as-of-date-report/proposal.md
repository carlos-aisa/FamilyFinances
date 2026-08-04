# Economic State As-Of-Date Report

## Why

The current Economic State report is designed for a month-focused snapshot and annual evolution analysis. Users also need a concise answer to a different question: what was the economic state on a specific calendar day? Combining that purpose with the existing tabs and focused-month controls would make both reports harder to interpret.

## What Changes

- Add a standalone report entry and page for an exact as-of date.
- Default the date to today and prevent future date selections.
- Show only six summary metrics: assets, liabilities, net worth, month-to-date income, month-to-date expenses, and month-to-date net result.
- Clearly distinguish balance-as-of-date values from the month-to-date flow period.
- Reuse the existing economic-state API contract and calculations without changing backend behavior.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `economic-state-reporting`: add a compact, date-specific economic-state report alongside the existing month-focused report.

## Impact

- Web UI: a new Razor report page, Reports index navigation, and localized copy.
- Tests: focused Web component coverage for date handling, rendering, loading, and errors.
- APIs, application handlers, database schema, and financial calculations: no changes.

## Non-Goals

- Do not alter the existing `/reports/economic-state` page, its month selection, tabs, charts, or evolution behavior.
- Do not introduce an API endpoint, DTO, calculation, export, or persistence change.
- Do not allow future dates or create a cross-month custom-period report.

## Release Impact

Type: minor
Rationale: adds a backward-compatible report route and navigation entry without changing existing report contracts or calculations.
