## Why

After tables and charts are in place, users still need actionable interpretation support: what explains most spending/income, where concentration risk exists, and which months or groups are anomalous. This is required to make reporting truly decision-oriented before closing `0.9.x`.

## What Changes

- Add reporting insights for Pareto and concentration analysis.
- Add outlier/anomaly detection indicators for expense behavior at group level.
- Provide user-facing insight panels linked to existing report periods/filters.
- Keep insight results explainable (transparent formulas and thresholds).

### Non-Goals

- No machine-learning forecasting pipeline.
- No automated budgeting recommendations.
- No external analytics service integration.

### Rollback Plan

- Keep insights additive in UI and API so they can be disabled independently.
- If thresholds cause false positives or confusion, hide insight panels and keep charts/tables unchanged.
- Roll back insights endpoints without changing core reporting contracts.

## Capabilities

### New Capabilities
- `reporting-insights`: Pareto, concentration, and anomaly insight generation for reporting.

### Modified Capabilities
- `system`: Extend reporting baseline with insight generation and explainability requirements.

## Impact

- Application reporting layer (insight calculations and thresholds).
- API reporting endpoints/DTOs for insights payloads.
- Web report pages displaying insight cards/sections.
- Tests for deterministic insight outputs and threshold behavior.
