## Why

Annual charts help with macro trends, but users also need month-level visibility to understand short-term behavior and intra-month dynamics. After `0.9.3`, annual evolution is integrated across report tabs, so `0.9.4` must extend those same tabs with intra-month charts instead of reintroducing a separate Monthly Evolution page.

## What Changes

- Add month-level chart visualizations focused on intra-month evolution in integrated report tabs:
  - `Economic State` -> `Asset Evolution`
  - `Account Group Totals` -> `State Evolution`
- Implement charts for:
  - Daily evolution of asset balance inside selected month.
  - Daily comparison of total asset balance vs account-group trajectories for selected month.
- Add focused-month selector behavior to these tab panels.
- Keep existing annual charts and tables as complementary views.

### Non-Goals

- No anomaly detection or forecasting (handled in `0.9.5`).
- No replacement of annual charts.
- No reintroduction of a dedicated `/reports/monthly-evolution` page.
- No introduction of write-path or transaction editing changes.

### Rollback Plan

- Keep monthly chart features additive and behind clear UI sections.
- If issues appear, disable monthly chart panels while preserving existing annual chart/table behavior.
- Roll back month-level endpoint additions independently from state-evolution annual contracts.

## Capabilities

### New Capabilities
- `monthly-reporting-charts`: Month-level daily balance and balance-vs-group charting behavior.

### Modified Capabilities
- `monthly-balance-evolution-reporting`: Extend integrated state-evolution tabs with focused-month monthly chart interaction.
- `system`: Extend reporting baseline with month-level chart data contracts aligned with primary `state-evolution` semantics.

## Impact

- Reporting API endpoints/queries for month-level chart datasets.
- Blazor integrated report tabs and filters for focused-month selection.
- Web/API/Application tests validating dataset correctness and month filter semantics.
