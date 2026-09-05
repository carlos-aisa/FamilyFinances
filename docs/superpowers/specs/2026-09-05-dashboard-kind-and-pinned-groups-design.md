# Dashboard Evolution: Expense Kinds and Pinned Groups

## Status

Approved conceptually. This document defines the implementation boundary; no application code is changed by this specification.

## Goal

Evolve the existing FamilyFinances dashboard without replacing its visual identity or reporting infrastructure. The dashboard remains a household-oriented, at-a-glance view and answers four distinct questions:

1. How did the current month develop day by day?
2. How much entered, left, and remained in each month of the current year?
3. What expense kinds consumed the most money this month?
4. How are the user-selected areas of the household economy performing?

The target layout retains the existing five KPI cards and uses four responsive rows:

1. Income, expense, monthly result, net worth, and YTD result.
2. Current-month daily income/expense evolution and annual income/expense/result evolution.
3. Expense-kind ranking and pinned account groups.
4. Asset-total evolution and an intentionally empty secondary slot; future known movements are not implemented in this change.

## Existing Assets to Reuse

- `DashboardPage.razor` remains the dashboard host and retains its loading, error, KPI, data-sufficiency, responsive-grid, and localization patterns.
- `GetDashboardOverviewHandler`, `DashboardOverviewDto`, and `GET /api/v1/reports/dashboard-overview` remain the dashboard aggregation boundary.
- `EvolutionChart` remains the current-month daily chart. Its existing cumulative end-of-day semantics are retained.
- `AnnualBarChart`, `AnnualChartSeries`, Chart.js, and `reportCharts.js` remain the annual chart stack.
- `AssetTotalEvolutionPanel` remains the asset-total evolution implementation.
- Account-group list/detail pages, `AccountGroupsApi`, and the existing account-group aggregate/repository follow the established group-management workflow.

## Data Semantics

### KPI row

The dashboard continues to display exactly five KPIs:

- current-month income;
- current-month expense;
- current-month result (`income - expense`);
- net worth;
- YTD result.

Previous-month deltas and the existing complete/partial/insufficient-history behavior remain unchanged. The UI calls `NetResult` “Monthly result”; no accounting semantics change.

### Current-month evolution

The existing daily income-versus-expense chart stays in place. Its end-of-day cumulative values correctly answer how the selected current month is progressing. It is not replaced by the annual chart.

### Annual income, expense, and result

`DashboardOverviewDto.YtdSummary.MonthlyNetPoints` already contains per-month income, absolute expense, and net result. The dashboard will create three annual series from those values:

- income: grouped green bars;
- expense: grouped red bars using absolute display values;
- result: cyan line using `income - expense`.

Future months remain null/visually marked as they are today. The independent monthly-net chart is removed from the dashboard after this combined chart is in place.

### Expense kind ranking

An expense kind is the assigned `AccountKindCatalog` for an account, not the historical `AccountKind` enum and not an account group. Each account has exactly one `KindId`, which makes kinds suitable for a non-overlapping expense distribution.

The dashboard ranking is calculated for the selected month through `asOf`:

- filter transaction splits to `AccountNature.Expense`;
- join each account to its `AccountKindCatalog`;
- aggregate using the catalog ID and catalog name;
- normalize displayed expense totals to positive absolute values;
- order by amount descending, then by name ordinal-ignore-case;
- retain exactly the top six non-zero kinds;
- aggregate every remaining non-zero kind into one localized `Others` row.

The dashboard renders the result as ordered horizontal bars. It does not use a pie or donut chart.

This change deliberately uses a dashboard-specific repository query and DTO. It does **not** add `Kind` to `ReportingInsightDimension`: that enum drives reporting insights, Pareto/anomaly behavior, and API surfaces, and a partial implementation would create an inconsistent public reporting dimension.

### Pinned account groups

`AccountGroup.IsDashboardPinned` is an explicit user preference with default `false`. Groups are never selected automatically by expenditure, size, or recency.

For each pinned group, the dashboard displays:

- group name;
- current-month operational result through `asOf`;
- YTD operational result through `asOf`.

Operational result includes **only** member accounts with `Income` or `Expense` nature. Asset, liability, and equity member accounts are excluded. This is intentional: balance and debt are separate future concepts and must not be presented as operating result.

Groups may overlap. A split from an account belonging to multiple groups contributes to every corresponding group; rows are independent monitoring views and are never added together or expressed as shares of total expenditure. Rows are deterministically ordered by group name.

The repository performs one projected aggregate query for all pinned groups, joining splits, accounts, memberships, and pinned groups. It uses the reporting display sign convention (`-split.Amount.Cents`) so income is positive and expense is negative. It must not reuse the current group-evolution query, because that query is designed for balance evolution and deliberately excludes liabilities.

If no groups are pinned, the panel shows a localized empty state pointing to Account Group management.

## Contract Changes

### Dashboard overview (additive)

Add the following records in the reporting DTO layer:

```csharp
public sealed record DashboardExpenseKindRankDto(
    Guid? KindId,
    string Label,
    long AmountCents,
    decimal Percentage,
    bool IsOthers);

public sealed record DashboardPinnedGroupDto(
    Guid GroupId,
    string GroupName,
    long MonthOperationalResultCents,
    long YtdOperationalResultCents);
```

Append `ExpenseKindRanking` and `PinnedGroups` to `DashboardOverviewDto`. The existing endpoint stays at `GET /api/v1/reports/dashboard-overview`; this is an additive response change, not a new dashboard endpoint.

### Account-group update pattern

Introduce an additive general partial-update endpoint:

```http
PATCH /api/v1/account-groups/{id}
Content-Type: application/json

{ "isDashboardPinned": true }
```

The endpoint accepts a dedicated partial-update request DTO for this increment and returns `204 No Content`. It is protected by `CanWrite`, validates the group ID through the application handler, and returns `404` if the group does not exist.

`PATCH /api/v1/account-groups/{id}/rename` remains unchanged for backward compatibility. Existing clients and tests keep working; no endpoint is removed or redirected. Future group fields can converge on the general PATCH only when a dedicated backwards-compatible migration is planned.

Both `AccountGroupDto` and `AccountGroupDetailsDto` add the `IsDashboardPinned` boolean so the management UI can display the persisted state.

The OpenAPI source of truth (`openspec/api-spec.yaml`) must document the new PATCH operation, request schema, and the added response fields.

## Layered Implementation Design

### Domain

- Add `IsDashboardPinned` with default `false` to `AccountGroup`.
- Add `SetDashboardPinned(bool isDashboardPinned)`.
- Existing creation and name/description invariants remain intact.

### Application

- Add a request and handler to update the pinned state using `IAccountGroupRepository` and `ILedgerUnitOfWork`.
- Extend account-group list/detail DTO mappings.
- Extend the reporting repository abstraction with one dashboard-specific query for expense-kind ranks and one for pinned-group operational results, or a single dashboard supplemental-data method returning both projections.
- `GetDashboardOverviewHandler` builds the Top 6 + Others list and attaches the pinned-group rows. The fixed value `6` is a named constant in the handler.

### Infrastructure

- Map `IsDashboardPinned` as required with default `false` in `AccountGroupConfiguration`.
- Add an EF Core migration named with the project timestamp convention, for example `AddAccountGroupDashboardPin`, plus its designer and `LedgerDbContextModelSnapshot` update.
- Use read-only projections and a bounded number of aggregate queries. Do not issue a query per pinned group.

### API and Web

- Add the general PATCH endpoint and typed client method `SetDashboardPinnedAsync`.
- Add the toggle in the account-group detail page. It is a simple persisted preference, not a draggable widget/layout configuration.
- Replace the annual income/expense plus monthly-net dashboard blocks with the mixed annual chart.
- Replace current dashboard group evolution and expense composition blocks with expense-kind ranking and pinned-group table.
- Preserve existing color tokens and responsive Bootstrap grid behavior.

## Chart Component Change

Extend `AnnualBarChart` rather than add a dependency or duplicate a chart wrapper. Add an optional parameter identifying the one line-series key (for example `result`). The Razor component forwards each dataset’s rendering type; `reportCharts.js` maps that dataset to Chart.js mixed-chart configuration:

- bar datasets retain current border radius, tooltip, and export behavior;
- the result dataset uses `type: "line"`, cyan semantic color, non-filled line, and existing tooltip/index interaction;
- all three use the same euro axis because the values share the same unit and scale.

The component remains backward-compatible: callers that do not supply a line key continue rendering grouped bars exactly as today.

## Navigation and Filtering Scope

No generic cross-filtering interaction is introduced in this increment. Existing Chart.js wrappers do not publish click selections, and the present reports do not accept an `AccountKindCatalog` filter. Adding it would require navigation state, report contracts, API filters, and interaction/accessibility design beyond the approved dashboard scope.

The pinned-groups panel may link to the existing account-group report only after verifying that its requested period and all-nature semantics match the row. Expense-kind rows do not claim a drill-down destination until a report accepts a real catalog-kind filter.

## Explicitly Deferred

- known upcoming movements: there is no recurring, scheduled, or planning model;
- forecasts or predictions;
- geographical map: transaction geolocation does not exist;
- generic visual click-to-filter framework;
- extending `ReportingInsightDimension` with `Kind` and all dependent Pareto, anomaly, API, localization, and reporting UI work;
- custom group ordering, drag and drop, dashboard widgets, or per-user dashboard layouts;
- broad redesign of report pages.

## Test Plan

### Unit tests

- `AccountGroup` defaults to unpinned and changes state through `SetDashboardPinned`.
- the account-group update handler persists the requested boolean and returns not found for a missing ID.
- Top 6 selection, stable ties, non-zero filtering, and Others aggregation.
- pinned-group operational result excludes asset, liability, and equity accounts and includes income/expense accounts.

### EF/repository integration tests

- migration applies to a real relational provider and existing groups receive `false`;
- pinned-state persistence and projection are correct;
- a group containing all account natures returns only flow-account results;
- overlapping groups independently receive the shared account’s contribution;
- dashboard supplemental query has no per-group query behavior.

### API integration tests

- PATCH requires write authorization, persists true/false, and returns 404 for an unknown group;
- list/detail DTOs expose pin state;
- dashboard endpoint returns rank rows and pinned-group rows with the defined semantics;
- existing dashboard and rename endpoints retain their contracts.

### Web tests

- dashboard renders five KPIs, daily chart, annual mixed chart, Top 6 + Others ranking, pinned groups, and asset evolution in the intended responsive order;
- annual mixed-chart payload marks only the result as line;
- empty pinned-group state and management toggle work;
- existing tests for loading, insufficient history, image export, and mobile layout remain valid or are adapted without reducing coverage.

## Documentation and Validation

Implementation must update `openspec/api-spec.yaml`, relevant OpenSpec change records, localizations, and any dashboard documentation affected by observable behavior. Before completion, run the affected unit, web, API integration, and migration tests using the repository’s normal .NET test commands.

