## Why

The Dashboard currently always shows the current month, even though the dashboard overview API can already calculate a historical month-end state. Users need to inspect the same at-a-glance financial information for a selected month and retain that context when opening a related report.

## What Changes

- Add global year and month selectors to the Dashboard, defaulting to the current period.
- Persist the selected period in Dashboard query parameters and apply it to all analytical cards, charts, badges, and CSV exports.
- Make the latest-expenses card return the six latest expense movements within the selected month.
- Preserve the selected period when navigating from Dashboard analytics to a compatible report, including account-group totals.
- Keep Quick Entry as a current-time action that is independent from the selected reporting period.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `dashboard-household-financial-overview`: Dashboard overview data and analytic navigation will use an explicit selected period.
- `dashboard-latest-expense-movements`: Latest expense movements will be scoped to the Dashboard selected month.
- `dashboard-card-csv-exports`: CSV exports will contain data from the Dashboard selected period.

## Impact

- `DashboardPage` and its component tests.
- Existing dashboard-overview API client usage and Dashboard URL state.
- Latest-expenses API, application handler, repository query, OpenAPI contract, and integration tests.
- Existing Dashboard period localization resources, which are reused for the controls.

## Non-Goals

- Creating a separate historical-dashboard report or duplicating the Dashboard layout in the reports area.
- Changing Quick Entry behavior or making it create transactions in a historical month.
- Adding arbitrary date-range filtering; this change selects complete calendar months only.

## Rollback Plan

If a regression is found, remove the Dashboard period controls and restore the current-month default request. The latest-expenses endpoint will retain backward-compatible no-parameter behavior, so existing callers continue to receive the global current implementation until the Dashboard change is redeployed.
