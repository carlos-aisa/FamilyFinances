## Context

The Dashboard is an at-a-glance view, but its Blazor page calls the dashboard-overview API without period parameters and independently loads globally recent expense movements. The dashboard-overview endpoint already accepts a year and month, resolving historical months to their final calendar day and the active month to today. Account-group navigation already accepts year and month query parameters.

## Goals / Non-Goals

**Goals:**

- Let authenticated users select a complete calendar month on the Dashboard.
- Keep every analytical Dashboard block, export, and compatible navigation target in the same period context.
- Make historical latest-expenses data deterministic and limited to the selected month.
- Preserve backward compatibility for callers that omit a latest-expenses period.

**Non-Goals:**

- Do not create a second dashboard or add arbitrary date ranges.
- Do not change the existing economic-state reports or their filters.
- Do not apply a historical period to Quick Entry.

## Decisions

### 1. Use Dashboard-wide year and month selectors beside the data-sufficiency notice

The Dashboard will place compact year and month selectors in the same horizontal row as the data-sufficiency notice, aligned to its right edge. At narrow breakpoints, the controls will move below the notice rather than compress the alert text or create horizontal overflow. When no data-sufficiency notice is needed, the controls will retain that dedicated context row below the page header.

The Dashboard will read and write `year` and `month` query parameters. The initial state is the current year and month when parameters are absent or invalid. The current year offers months through today; previous years offer all twelve months.

This is preferred over local, card-specific controls because Dashboard cards answer one shared financial question. The context row keeps the period visible before the KPI strip without competing with the title or Quick Entry action. URL state makes refresh, browser history, sharing, and navigation context deterministic.

### 2. Reuse the existing dashboard-overview period contract

The page will call `ReportsApi.GetDashboardOverviewAsync(selectedYear, selectedMonth)`. The existing API resolves historical periods to month end and the current period to today, so no duplicate report endpoint or calculation path is needed.

Creating a dedicated historical-dashboard endpoint was rejected because it would duplicate an established read model without adding different semantics.

### 3. Extend latest-expenses with an optional calendar-month scope

The latest-expenses endpoint, handler, and repository query will accept optional year and month parameters together. When supplied, the query returns at most six qualifying transactions inside that month, ordered by booked date and transaction identifier descending. With neither parameter, its existing caller-compatible behavior remains available.

Filtering in the client was rejected because the current endpoint only returns six global records and cannot accurately derive a historical month's six newest expenses.

### 4. Propagate the selected period through analytical navigation

Dashboard links to account-group totals will use the selected year and month. Any future Dashboard navigation that targets a period-aware report must use the same query parameters. Operational navigation, such as Quick Entry, remains outside the reporting context.

## Risks / Trade-offs

- [Historical months with no movements] → Render existing empty states and export an empty CSV with the selected period metadata.
- [Invalid or future query parameters] → Normalize to the current reporting period in the UI and retain API validation at the boundary.
- [Latest-expenses API contract expansion] → Keep no-parameter behavior and document both modes in OpenAPI.
- [Mixed-period navigation] → Centralize Dashboard report URL construction and cover it with component tests.

## Migration Plan

1. Deploy the additive latest-expenses query parameters and updated dashboard client.
2. Deploy the Dashboard selector and period-aware data loading together.
3. Verify current-month requests remain unchanged when no URL parameters are supplied.
4. If rollback is required, revert the Dashboard UI and continue serving the optional endpoint parameters without affecting existing callers.

## Open Questions

None.
