## Why

The Dashboard's monthly textual summary did not give users a concise view of their most recent spending, and its third-row data panels did not offer a usable export action. This change records the delivered replacement with a deterministic expense feed, compact presentation, and CSV exports.

## What Changes

- Replace the Dashboard monthly textual summary with a compact list of the six latest transactions that contain Expense-nature splits.
- Add an authorized `GET /api/v1/transactions/latest-expenses` endpoint and document its response in the OpenAPI contract.
- Add a reusable movement-list component that is independent from the latest-expenses query.
- Add CSV download actions and period context to all third-row Dashboard data panels: highlighted groups, expense-kind ranking, and latest expenses.
- Preserve the expense-kind ranking as a compact visual list and present Expense metrics as positive, neutral magnitudes.
- Order highlighted groups by monthly operational result.

## Capabilities

### New Capabilities

- `dashboard-latest-expense-movements`: Deterministic retrieval and reusable presentation of the latest Expense movements.
- `dashboard-card-csv-exports`: CSV downloads for third-row Dashboard data panels.

### Modified Capabilities

- `dashboard-household-financial-overview`: Replace the monthly textual summary with latest expenses, add the third-row control contract, and clarify Expense metric presentation and highlighted-group ordering.

## Impact

- API, application, and infrastructure: a read-only transaction endpoint, handler, DTO, repository query, dependency-injection registration, and OpenAPI path/schema.
- Web: Dashboard composition, a reusable movement list, localized labels, and CSV export interop reuse.
- Tests: application, API integration, API client, shared component, and Dashboard component coverage.

## Non-Goals

- Do not introduce forecasting, recurrence management, configurable widgets, or a generic feed provider.
- Do not change transaction persistence, accounting calculations, or existing transaction-list behavior.
- Do not introduce a generic export framework beyond reuse of the existing CSV builder and download interop.

## Rollback Plan

- Remove the latest-expenses card and its endpoint/client integration, then restore the prior Dashboard summary only if a rollback is required.
- Remove the three CSV controls while retaining the underlying third-row panels if export behavior must be disabled.
- Re-run the full solution tests and OpenSpec validation after rollback.
