## Context

The dashboard currently receives one aggregate response from `GET /api/v1/reports/dashboard-overview`, then reuses annual-chart components and the asset-total evolution report. It also derives expense composition from group-oriented reporting insights and derives dashboard group state from account-group balance evolution. Those two existing sources are semantically unsuitable for the approved dashboard blocks.

The implementation remains layered: Domain owns the pin preference, Application defines update/report DTOs and handlers, Infrastructure owns EF projections, API exposes typed contracts, and Blazor consumes API services only.

## Decisions

### Decision 1: Keep `dashboard-overview` as the dashboard aggregation boundary

`DashboardOverviewDto` gains additive `ExpenseKindRanking` and `PinnedGroups` collections. A new dashboard endpoint is not introduced.

Rationale: the route already represents the selected month/dashboard snapshot and maintains established authorization, client, error, and test paths.

### Decision 2: Use dashboard-specific reporting projections

The repository adds dedicated dashboard projections for expense-kind ranking and pinned-group operational results. `ReportingInsightDimension` is not extended.

Rationale: `ReportingInsightDimension` is a public reporting concept used by Pareto/anomaly and API surfaces. Adding only a dashboard resolver would leave an incomplete dimension. A dedicated query gives the dashboard its required semantics without expanding unrelated scope.

### Decision 3: Expense ranking is fixed Top 6 plus Others

The handler uses a named `ExpenseKindTopCount = 6` constant. It aggregates non-zero expense accounts by `AccountKindCatalog.Id` and `Name`, sorts by absolute amount descending then name, preserves the first six, and adds one localized Others row for the remainder.

Expense kind is catalog identity, not legacy enum and not group membership. The response retains the nullable `KindId` only for the synthetic Others row.

### Decision 4: Pinned group rows measure operational flow only

Each pinned row contains `MonthOperationalResultCents` and `YtdOperationalResultCents`. Both aggregate only member accounts with `Income` or `Expense` nature, using display sign `-TransactionSplit.Amount.Cents`.

Asset, liability, and equity accounts are excluded. Overlapping memberships contribute independently to every relevant row. Rows must never be summed or converted into expense shares.

### Decision 5: One aggregate query for pinned groups

The reporting repository projects pinned groups, their flow-account memberships, and relevant transaction splits in one bounded query per period range (or one query grouped with a period discriminator). It does not call the existing account-group totals endpoint once per group and does not reuse balance-evolution series.

### Decision 6: Extend annual chart wrapper compatibly

`AnnualBarChart` accepts an optional line-series key. Its existing callers remain bars-only. For the dashboard caller, the JS payload identifies `result` as a line dataset while income and expense retain the current bar configuration. All series share the existing euro axis.

### Decision 7: Add a general, additive account-group PATCH

Introduce `PATCH /api/v1/account-groups/{id}` with a partial request DTO limited in this increment to `IsDashboardPinned`. It returns `204 No Content`; a missing ID returns `404`; authorization remains `CanWrite`.

The existing `PATCH /{id}/rename` endpoint remains intact for compatibility. The application may later consolidate mutations deliberately, but this change does not impose a client migration or remove a route.

### Decision 8: Make pinned status scannable in account-group cards

The account-group list renders a compact, text-bearing semantic badge only for groups whose `IsDashboardPinned` value is true. It uses the existing list payload and does not add a list-level toggle, a secondary request, or a placeholder for unpinned groups.

### Decision 9: Use family-oriented Spanish annual accumulation labels

The Spanish dashboard KPI for the annual net accumulation and the pinned-group annual column both use `Acum. anual`. The refinement is presentation-only: calculations, resource keys, English labels, and long-form explanatory labels remain unchanged.

## Data Flow

1. Dashboard requests its existing overview endpoint for `asOf`.
2. The repository calculates existing KPI/core data and new dashboard-specific collections.
3. The handler applies Top 6 + Others shaping and attaches ordered pinned-group rows.
4. The API serializes the additive response through the existing endpoint and client.
5. Blazor maps the annual monthly points into income, expense, and result chart series; it renders kind bars and pinned-group table from the new collections.
6. Account-group detail uses the new PATCH to persist the pin toggle, then reloads its details.

## Migration

Add non-null `AccountGroups.IsDashboardPinned` with default `false`. Existing rows therefore remain unpinned and dashboard behavior does not change until users choose groups. Update the Ledger context snapshot and verify migration application with a relational-provider test.

## Error Handling and Empty States

- Empty expense data yields the existing chart/section empty-state behavior; no synthetic zero kinds are created.
- No pinned groups yields a localized compact empty state with a link to group management.
- Missing group for PATCH returns `404`; client handling follows existing account-group API behavior.
- Existing dashboard partial/insufficient history handling remains unchanged.

## Risks and Mitigations

- **Ambiguous group metric:** explicitly name and localize the values as operational result; exclude balance-account natures in the query and cover with tests.
- **Overlapping groups:** never calculate percentages or a grand total; test duplicated membership explicitly.
- **Chart regression:** optional line-series configuration is backward-compatible and existing annual-bar payload tests stay valid.
- **Schema regression:** default false preserves all pre-existing group behavior and migration tests validate upgrade.
- **Scope creep into insights:** prohibit adding `Kind` to the reporting-insights enum/surfaces in this change.
