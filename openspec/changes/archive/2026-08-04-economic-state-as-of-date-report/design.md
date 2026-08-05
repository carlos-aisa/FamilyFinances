# Design: Economic State As-Of-Date Report

## Context

`GetEconomicStateAsync(DateOnly asOf)` already returns both date-specific stock values and month-to-date flow values. The application handler derives the flow period from the first day of `asOf`'s month through `asOf`, inclusively. The missing capability is a focused Web presentation of that read model.

## Decision 1: Use a separate compact report route

Create a dedicated report page and entry in the Reports index rather than adding a mode to `/reports/economic-state`. The existing page remains responsible for month-focused analysis and annual evolution. The new route has one purpose: present an exact-date snapshot.

Alternative considered: add an exact-date mode to the existing page. Rejected because it would expose tabs and charts whose annual or month-focused semantics do not describe a single-day snapshot.

## Decision 2: Reuse the existing report contract

The page will call the existing `ReportsApi.GetEconomicStateAsync(DateOnly asOf)` method. It will not create an endpoint, query, DTO, or calculation. This retains the established inclusive as-of behavior and prevents duplicate financial logic.

## Decision 3: Make stock and flow contexts explicit

The page displays assets, liabilities, and net worth under a balance-as-of-date context. It displays income, expenses, and period net result under a period beginning on the first day of the selected date's month. Both contexts use the selected date and active culture formatting so users cannot mistake a balance for a flow.

## User Flow

1. The user opens the Reports index and selects the new as-of-date report.
2. The date input defaults to today's local date and uses today as its maximum value.
3. The page loads the report for that date automatically.
4. The user may select an earlier valid date and apply it to reload the six summary metrics.
5. The page renders established loading and error states while retaining the selected date.

## Testing

- Component tests assert the default and maximum date values.
- Component tests verify that applying a historical date calls the existing API with that exact date.
- Component tests assert all six metrics and both date contexts.
- Component tests cover loading and error presentation using existing report-page conventions.

## Scope Boundaries

- Do not change existing Economic State components, tabs, charts, or focused-month behavior.
- Do not change API, application, database, or OpenAPI contracts.
- Do not add export or custom date-range functionality.
