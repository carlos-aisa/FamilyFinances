## Why

Users need a simple way to know the total value of all `Asset` accounts at a specific date, without manually summing balances account by account. This is needed to quickly review financial position at historical checkpoints (month-end, year-end, or any custom date).

## What Changes

- Add a new reporting capability to calculate **total asset balance as-of a given date**.
- Expose the calculation through a new API endpoint in `ReportsController`.
- Add a new Web Reports page with a date selector and a single result card showing the computed total.
- Add navigation entry in Reports index for this new report.
- Add validation rules for the required date parameter and consistent error behavior.
- Add tests (application/repository integration/API/web API client) and update OpenAPI documentation.

### Non-goals

- No net-worth calculation (Assets minus Liabilities) in this change.
- No per-account breakdown table in this report (only aggregated total).
- No multi-currency conversion logic (single-currency EUR behavior remains).
- No change to transaction posting semantics or account classification rules.

### Rollback Plan

- Remove the new endpoint and related application handler/query/repository method.
- Remove the Web report page and Reports index navigation card.
- Revert OpenAPI changes for the new route and DTO.
- Keep all existing reporting endpoints and data model unchanged.

## Capabilities

### New Capabilities
- `asset-total-balance-reporting`: Calculate and expose the aggregated balance of all `Asset` accounts for a user-selected as-of date.

### Modified Capabilities
- `system`: Add a new report endpoint and Web report navigation/page for as-of asset total balance.

## Impact

- Application reporting layer: new query/handler/DTO and reporting repository contract.
- Infrastructure reporting repository: new aggregated balance query path using existing ledger data.
- API layer: new `GET /api/v1/reports/asset-total-balance` endpoint.
- Web layer: `ReportsApi` extension, new report page, and reports index update.
- Documentation: `openspec/api-spec.yaml` update for endpoint contract.
- Tests: reporting/API/web API client tests for success and validation scenarios.
