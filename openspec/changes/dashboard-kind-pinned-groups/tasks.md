## 1. OpenSpec and contract preparation

- [ ] 1.1 Confirm the change deltas against the active dashboard, annual-chart, account-kind-catalog, and system specifications.
- [ ] 1.2 Update `openspec/api-spec.yaml` with the additive general account-group PATCH request/operation and additive account-group/dashboard schemas.
- [ ] 1.3 Add English and Spanish localization keys for kind ranking, Others, pinned-group labels, operational-result labels, empty state, and management toggle.

## 2. Account-group pin persistence and update API

- [ ] 2.1 Add `IsDashboardPinned` and `SetDashboardPinned(bool)` to `AccountGroup`, defaulting creation to false.
- [ ] 2.2 Map the property in `AccountGroupConfiguration` with required/default-false semantics.
- [ ] 2.3 Create the Ledger EF Core migration, generated designer, and model snapshot update.
- [ ] 2.4 Extend account-group list/detail DTOs and mapping handlers with the pin state.
- [ ] 2.5 Add partial-update request/handler/controller/client plumbing for `PATCH /api/v1/account-groups/{id}`.
- [ ] 2.6 Keep `PATCH /account-groups/{id}/rename` unchanged and retain its tests.
- [ ] 2.7 Add a simple pin toggle to account-group detail management and reload persisted state after update.

## 3. Dashboard reporting data

- [ ] 3.1 Define dashboard DTOs for expense-kind ranking and pinned-group operational results.
- [ ] 3.2 Add a repository projection aggregating selected-month expense splits by `AccountKindCatalog` identity and display name.
- [ ] 3.3 In `GetDashboardOverviewHandler`, shape non-zero kind totals into deterministic Top 6 + localized Others.
- [ ] 3.4 Add a bounded repository projection for all pinned groups’ selected-month and YTD operational results.
- [ ] 3.5 Restrict pinned-group projection to Income and Expense account natures, preserving reporting display signs.
- [ ] 3.6 Extend dashboard overview response additively; do not add `Kind` to `ReportingInsightDimension` or modify insight endpoints.

## 4. Dashboard composition and annual mixed chart

- [ ] 4.1 Extend `AnnualBarChart` and `reportCharts.js` with an optional compatible line-series configuration.
- [ ] 4.2 Build dashboard annual series for income bars, expense bars, and result line from existing monthly net points.
- [ ] 4.3 Remove the redundant standalone dashboard monthly-net chart only after the mixed chart renders correctly.
- [ ] 4.4 Replace dashboard group-evolution and group-based expense-composition blocks with expense-kind horizontal bars and pinned-group table.
- [ ] 4.5 Keep the current-month daily evolution, five KPI cards, asset-total evolution, data-sufficiency messaging, image export behavior, shared color tokens, and responsive ordering.
- [ ] 4.6 Render a localized no-pinned-groups state; do not add upcoming movements, forecasts, maps, or generic cross-filtering.

## 5. Tests

- [ ] 5.1 Add Domain unit tests for the default and state transitions of `IsDashboardPinned`.
- [ ] 5.2 Add Application unit tests for pin updates, Top 6 + Others ordering/aggregation, and pinned-group nature filtering.
- [ ] 5.3 Add relational EF integration tests for migration application, default false persistence, dashboard kind aggregate, flow-only group result, and overlapping memberships.
- [ ] 5.4 Add API integration tests for general PATCH authorization/404/persistence, additive account-group responses, and dashboard overview response semantics.
- [ ] 5.5 Update Web/API-client tests for account-group pin management, annual mixed-chart payload, dashboard block order, ranking, pinned empty state, and responsive layout.
- [ ] 5.6 Keep and run regression coverage for existing rename, dashboard history-state, annual bars, reports, and exports.

## 6. Validation and documentation

- [ ] 6.1 Run OpenSpec validation for this change.
- [ ] 6.2 Run affected unit, Web, API integration, and migration tests using the repository’s standard .NET commands.
- [ ] 6.3 Run a Release build for affected solution scope.
- [ ] 6.4 Update implementation-era OpenSpec proposal/design/tasks if discoveries materially change behavior or architecture.
- [ ] 6.5 Review OpenAPI and user-facing documentation for accuracy before completion.

