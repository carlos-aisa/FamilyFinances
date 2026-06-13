## Why

Refund quick entry currently shows inconsistent date and money formatting in the original-expense picker, and report charts do not visually distinguish the `0` baseline on the Y axis. These inconsistencies reduce readability and can cause interpretation mistakes during data entry and analysis.

## What Changes

- Standardize original-expense date display in Refund quick entry to `dd/MM/yyyy`.
- Standardize monetary display to European style with euro symbol suffix (`XXX,XX €`) in the affected quick entry and chart surfaces.
- Improve chart readability by emphasizing the Y-axis zero grid line with greater thickness and stronger contrast than non-zero grid lines.
- Keep functional semantics unchanged: no change to accounting values, signs, API payload contracts, or chart data ordering.

## Release Impact

Type: minor
Rationale: Presentation/readability hardening without behavioral or contract-breaking changes.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `quick-entry-workspace`: refund original-expense list formatting contract is tightened for date and amount readability.
- `monthly-reporting-charts`: Y-axis baseline readability contract is tightened with a highlighted zero line and consistent EUR display in chart labels/tooltips.
- `annual-reporting-charts`: Y-axis baseline readability contract is tightened with a highlighted zero line and consistent EUR display in chart labels/tooltips.

## Non-Goals

- No redesign of quick-entry layout, report layout, or chart interaction model.
- No multi-currency support or currency conversion logic.
- No changes to backend report calculations or transaction creation semantics.

## Rollback Plan

- Revert chart Y-axis scriptable grid styling and restore previous uniform grid rendering.
- Revert date/amount formatter changes in quick-entry rendering paths.
- Re-run focused Web tests for quick-entry and report charts to confirm baseline restoration.

## Impact

- Frontend: quick-entry refund list rendering, shared money/date helpers, report chart JS rendering.
- Tests: update formatter expectations and add/adjust assertions for affected formatting behavior.
- API: no contract change expected.
