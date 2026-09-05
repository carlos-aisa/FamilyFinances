## Why

The current dashboard already provides household KPIs and useful trend charts, but its expense composition is based on account groups and its group block represents balance evolution. Those semantics do not match the two questions a household needs to answer quickly:

- “What types of expense consumed money this month?”
- “How are the areas I deliberately monitor performing operationally?”

Account kinds are non-overlapping economic classifications; account groups are overlapping monitoring lenses. The dashboard must make that distinction explicit while preserving its current visual identity, endpoints, and reporting infrastructure.

## What Changes

- Keep the existing five dashboard KPIs and current-month daily income-versus-expense evolution unchanged.
- Replace the two separate annual dashboard visuals (income/expense bars and monthly net line) with one mixed annual chart: income bars, expense bars, and a monthly-result line.
- Replace group-based expense composition with an ordered horizontal `Top 6 + Others` expense-kind ranking for the current selected month.
- Add `AccountGroup.IsDashboardPinned`, exposed through additive account-group contracts and a general partial-update endpoint.
- Replace the dashboard account-group evolution visual with a compact table of user-pinned groups showing current-month and YTD operational result.
- Keep asset-total evolution as a lower-priority dashboard visual.

## Capabilities

### New Capabilities

- `dashboard-pinned-account-groups`: User-selected account groups can be marked for dashboard monitoring and shown with operational results.

### Modified Capabilities

- `dashboard-household-financial-overview`: Dashboard composition distinguishes expense kinds from pinned account groups and consolidates annual flow visuals.
- `annual-reporting-charts`: The reusable annual bar chart supports a compatible mixed bar-and-line series configuration.
- `account-kind-catalog`: Catalog-backed kind identity is used for dashboard expense aggregation.
- `system`: Account-group resource representation and partial-update API behavior support dashboard pinning.

## Impact

- **Domain/persistence:** `AccountGroup`, EF configuration, one Ledger migration, and model snapshot.
- **Application/reporting:** account-group update handler, additive DTO fields, dashboard-specific expense-kind and pinned-group projections, and dashboard overview composition.
- **API:** additive `PATCH /api/v1/account-groups/{id}` plus additive fields in existing account-group and dashboard-overview responses.
- **Web:** dashboard layout and chart configuration, group-management toggle, typed API client method, localization, and responsive component tests.
- **Documentation:** OpenAPI contract and affected OpenSpec base specifications.

## Non-Goals

- No recurring, scheduled, planned-movement, or forecasting feature.
- No map/geolocation feature.
- No custom group ordering, drag and drop, configurable widgets, or per-user dashboard layouts.
- No generic chart click-to-filter mechanism or `AccountKindCatalog` report filter.
- No partial addition of `Kind` to `ReportingInsightDimension`; Pareto, anomaly, and reporting-insights API surfaces remain unchanged.
- No removal, redirect, or behavior change for `PATCH /api/v1/account-groups/{id}/rename`.
- No broad report-page redesign.

## Rollback Plan

- Revert dashboard composition independently while keeping existing reporting endpoints and group-management data intact.
- Revert the additive dashboard DTO fields and queries if data regressions occur; existing consumers retain their prior fields.
- Revert the account-group pin UI and general PATCH endpoint while retaining the migration column as an inert `false`/stored preference if database rollback is not safe.
- Re-run dashboard, account-group, reporting API, and migration tests after each rollback boundary.

