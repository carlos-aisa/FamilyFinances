# Unified Evolution Chart Design (2026-03-12)

## Context
The app currently has two separate line-chart components for evolution visuals:
- `MonthlyLineChart` (daily points inside a month)
- `AnnualLineChart` (monthly points inside a year)

This creates style drift risk and forces manual propagation of visual contracts.

## Goal
Create one reusable evolution chart control that supports both modes with a flag, so any new evolution chart automatically inherits the same style and behavior.

## Scope
In scope:
- New unified chart component for line-based evolution charts.
- Migration of all current monthly/annual evolution charts that currently use `MonthlyLineChart` or `AnnualLineChart`.
- Keep the same JS runtime (`renderAnnualLineChart`) and tokenized styling contract.

Out of scope:
- `AnnualBarChart` and `AnnualCompositionChart`.
- Non-evolution visuals.

## Component Contract
`EvolutionChart` with key parameters:
- `Mode`: `DailyInMonth` | `MonthlyInYear`
- `Title`, `Subtitle`, `TestId`
- `Year`, optional `Month`
- `DailySeries` (for `DailyInMonth`)
- `AnnualSeries` (for `MonthlyInYear`)
- `YTickMaxTicks`
- `ShowFullRange`
- `DataUntilDay`, `DataUntilMonth`
- `CarryForwardAfterMarker`
- `EnableImageExport`

## Behavior
Shared behavior:
- Same card chrome, export button, badge contract.
- Same cutoff marker and future-area de-emphasis via JS payload.
- Same tooltip/axis token contract.

Mode-specific behavior:
- `DailyInMonth`: x-axis labels are days; payload emits `markerDay` + `totalDays`.
- `MonthlyInYear`: x-axis labels are months; payload emits `markerMonth` + `totalMonths`.

## Migration Plan
1. Implement `EvolutionChart` and mode enum.
2. Migrate all current usages of `MonthlyLineChart` and `AnnualLineChart` in dashboard/reports pages.
3. Keep bars/composition unchanged.
4. Update chart tests to validate unified component payloads for both modes.
5. Validate with `dotnet build` and web tests.

## Risks and Mitigation
- Risk: accidental change in data semantics.
  - Mitigation: preserve existing payload schema and add tests for marker/carry-forward behavior.
- Risk: partial migration leaving old components in active use.
  - Mitigation: repository grep check for remaining `MonthlyLineChart`/`AnnualLineChart` usages.

## Acceptance Criteria
- All existing line-based evolution charts use the unified control.
- Daily and monthly-in-year modes render with identical style contract.
- Tests pass and no regression in chart payload structure.
