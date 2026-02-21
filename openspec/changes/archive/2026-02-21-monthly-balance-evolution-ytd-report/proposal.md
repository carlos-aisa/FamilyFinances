## CRITICAL IMPLEMENTATION CONSTRAINTS

### FORBIDDEN
- Do not merge this behavior into existing report pages (`monthly-summary`, `account-totals`, `account-group-totals`) as ad-hoc extensions.
- Do not introduce chart libraries in this change.
- Do not return presentation-only payloads that would require a future API contract break to support charts.

### REQUIRED
- Implement a dedicated report experience with a year selector and three views: Accounts, Asset Total, and Account Groups.
- Define a graph-ready monthly dataset contract (stable, ordered, machine-friendly fields) even if UI initially renders tables/cards only.
- Keep delta semantics explicit and consistent for every month:
  - Delta vs previous month
  - Delta vs start of year (YTD baseline)

## Why

Users need to understand monthly balance evolution from the start of the year, not only point-in-time totals. This is needed now to support operational financial review and to prepare a stable data contract for upcoming chart-based visualization.

## What Changes

- Add a new dedicated report capability for **monthly YTD evolution** with one report entry and three tabs/views:
  - `Accounts` (one series per account)
  - `Asset Total` (single aggregated asset series)
  - `Account Groups` (one series per account group)
- Add a year selector to query evolution from January of the selected year.
- Compute and expose, for each month, these values:
  - `EndBalanceCents`
  - `DeltaVsPreviousMonthCents`
  - `DeltaVsYearStartCents`
- Define graph-ready API response models with ordered monthly points and stable identifiers per series (account, asset-total, account-group).
- Add a dedicated Web report page under `/reports` for this capability, using table/card-first presentation while keeping data ready for chart rendering.
- Update tests and OpenAPI documentation for the new endpoint(s) and DTOs.

## Non-goals

- No chart rendering in this change (only chart-ready data contract and presentation layout).
- No arbitrary custom date-range evolution (scope is year-to-date by selected year).
- No transaction-level drilldown or anomaly analysis in this change.
- No multi-currency conversion or currency-specific formatting changes.

## Rollback Plan

- Remove the new monthly-evolution API endpoint(s), query/handler, DTOs, and repository methods.
- Remove the Web report page/tab navigation entry for monthly evolution.
- Revert OpenAPI additions related to monthly evolution contracts.
- Keep existing reporting endpoints and behavior unchanged.

## Capabilities

### New Capabilities
- `monthly-balance-evolution-reporting`: Provide monthly year-to-date evolution datasets for accounts, total assets, and account groups, including deltas vs previous month and year start.

### Modified Capabilities
- `system`: Extend reporting behavior and Web reports navigation with a dedicated monthly evolution report route and contract.

## Impact

- Application layer (`src/FamilyFinances.Application/Reporting`):
  - New monthly evolution DTOs, query objects, and handlers.
- Infrastructure layer (`src/FamilyFinances.Infrastructure/Persistence/Repositories`):
  - New aggregation methods for monthly end balances and delta computation by series type.
- API layer (`src/FamilyFinances.Api/Controllers/V1/ReportsController.cs`):
  - New monthly evolution endpoint(s) with year parameter.
- Web layer (`src/FamilyFinances.Web/Api`, `src/FamilyFinances.Web/Components/Pages/Reports`):
  - New API client method(s) and dedicated report page with Accounts / Asset Total / Account Groups tabs.
- Documentation:
  - `openspec/api-spec.yaml` updates for new contracts.
- Tests:
  - Application tests, API integration tests, and Web client/page tests for monthly datasets and delta semantics.
