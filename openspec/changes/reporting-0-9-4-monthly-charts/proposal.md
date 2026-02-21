## Why

Annual charts help with macro trends, but users also need month-level visibility to understand short-term behavior and intra-month dynamics. The product roadmap requires explicit monthly chart views for balance evolution and comparison versus account groups.

## What Changes

- Add month-level chart visualizations focused on intra-month evolution.
- Implement charts for:
  - Daily evolution of balance within the selected month.
  - Daily/period evolution of balance versus account-group trajectories.
- Add month selector and filtering behavior for monthly chart context.
- Keep existing tables and annual charts intact as complementary views.

### Non-Goals

- No anomaly detection or forecasting (handled in `0.9.5`).
- No replacement of annual charts.
- No introduction of write-path or transaction editing changes.

### Rollback Plan

- Keep monthly chart features additive and behind clear UI sections.
- If issues appear, disable monthly chart panels while preserving all existing report behavior.
- Roll back monthly chart endpoint additions independently from annual chart features.

## Capabilities

### New Capabilities
- `monthly-reporting-charts`: Month-level balance and balance-vs-group charting behavior.

### Modified Capabilities
- `monthly-balance-evolution-reporting`: Extend monthly evolution report with month-focused chart interaction.
- `system`: Extend reporting baseline with month-level chart data contracts.

## Impact

- Reporting API endpoints/queries for month-level chart datasets.
- Blazor report pages and filters for month selection.
- Web/API/Application tests validating dataset correctness and month filter semantics.
