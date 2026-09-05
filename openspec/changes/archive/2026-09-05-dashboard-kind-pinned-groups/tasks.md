## 1. OpenSpec and contract preparation

- [x] 1.1 Confirm the change deltas against the active dashboard, annual-chart, account-kind-catalog, and system specifications.
- [x] 1.2 Update `openspec/api-spec.yaml` with the additive general account-group PATCH request/operation and additive account-group schemas.
- [x] 1.3 Add English and Spanish localization keys for kind ranking, Others, pinned-group labels, operational-result labels, empty state, and management toggle.

## 2. Account-group pin persistence and update API

- [x] 2.1 Add `IsDashboardPinned` and `SetDashboardPinned(bool)` to `AccountGroup`, defaulting creation to false.
- [x] 2.2 Map the property in `AccountGroupConfiguration` with required/default-false semantics.
- [x] 2.3 Create the Ledger EF Core migration, generated designer, and model snapshot update.
- [x] 2.4 Extend account-group list/detail DTOs and mapping handlers with the pin state.
- [x] 2.5 Add partial-update request/handler/controller/client plumbing for `PATCH /api/v1/account-groups/{id}`.
- [x] 2.6 Keep `PATCH /account-groups/{id}/rename` unchanged and retain its tests.
- [x] 2.7 Add a simple pin toggle to account-group detail management and reload persisted state after update.

## 3. Dashboard reporting data

- [x] 3.1 Define dashboard DTOs for expense-kind ranking and pinned-group operational results.
- [x] 3.2 Add a repository projection aggregating selected-month expense splits by `AccountKindCatalog` identity and display name.
- [x] 3.3 In `GetDashboardOverviewHandler`, shape non-zero kind totals into deterministic Top 6 + Others.
- [x] 3.4 Add a bounded repository projection for all pinned groups’ selected-month and YTD operational results.
- [x] 3.5 Restrict pinned-group projection to Income and Expense account natures, preserving reporting display signs.
- [x] 3.6 Extend dashboard overview response additively; do not add `Kind` to `ReportingInsightDimension` or modify insight endpoints.

## 4. Dashboard composition and annual mixed chart

- [x] 4.1 Extend `AnnualBarChart` and `reportCharts.js` with an optional compatible line-series configuration.
- [x] 4.2 Build dashboard annual series for income bars, expense bars, and result line from existing monthly net points.
- [x] 4.3 Remove the redundant standalone dashboard monthly-net chart only after the mixed chart renders correctly.
- [x] 4.4 Replace dashboard group-evolution and group-based expense-composition blocks with expense-kind horizontal bars and pinned-group table.
- [x] 4.5 Keep the current-month daily evolution, five KPI cards, asset-total evolution, data-sufficiency messaging, image export behavior, shared color tokens, and responsive ordering.
- [x] 4.6 Render a localized no-pinned-groups state; do not add upcoming movements, forecasts, maps, or generic cross-filtering.

## 5. Tests

- [x] 5.1 Add Domain unit tests for the default and state transitions of `IsDashboardPinned`.
- [x] 5.2 Add Application unit tests for pin updates and Top 6 + Others ordering/aggregation.
- [x] 5.3 Add relational EF integration tests for migration application, default false persistence, dashboard kind aggregate, flow-only group result, and overlapping memberships.
- [x] 5.4 Add API integration tests for general PATCH authorization/404/persistence, additive account-group responses, and dashboard overview response semantics.
- [x] 5.5 Update Web/API-client tests for account-group pin management, annual mixed-chart payload, dashboard block order, ranking, pinned empty state, and responsive layout.
- [x] 5.6 Keep and run regression coverage for existing rename, dashboard history-state, annual bars, reports, and exports.

## 6. Validation and documentation

- [x] 6.1 Run OpenSpec validation for this change.
- [x] 6.2 Run affected unit, Web, and API integration tests using the repository’s standard .NET commands.
- [x] 6.3 Run a Release build for affected solution scope.
- [x] 6.4 Update implementation-era OpenSpec proposal/design/tasks if discoveries materially change behavior or architecture.
- [x] 6.5 Review OpenAPI and user-facing documentation for accuracy before completion.

## 7. Pinned group card visibility refinement

- [x] 7.1 Render a localized, semantic dashboard-pin badge on pinned account-group cards only.
- [x] 7.2 Add a Web component test covering pinned and unpinned card states.
- [x] 7.3 Record the refinement in the OpenSpec proposal and design.

## 8. Spanish annual accumulation terminology refinement

- [x] 8.1 Change the compact annual KPI and pinned-group table header to `Acum. anual` in Spanish.
- [x] 8.2 Add localized dashboard component coverage for both labels.
- [x] 8.3 Record the refinement in the OpenSpec proposal and design.

## 9. Dashboard row composition and deterministic monthly summary

- [x] 9.1 Move asset-total evolution into the second dashboard row without changing its data or chart behavior.
- [x] 9.2 Add the third-row monthly textual summary block using existing dashboard overview data only.
- [x] 9.3 Add a focused builder with a four-insight maximum, reliable-history guard, expense-kind, pinned-group, and no-data coverage.
- [x] 9.4 Verify desktop row composition and responsive stacking through dashboard component tests.
- [x] 9.5 Record the refinement in the OpenSpec proposal and design.
