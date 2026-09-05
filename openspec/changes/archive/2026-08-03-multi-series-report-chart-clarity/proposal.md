## Why

Dense annual group bars and overlapping account lines hide the information a household needs. The attempted heatmap and trajectory-strip alternatives did not make the values practical to inspect in the available dashboard space.

## What Changes

- Replace the annual account-group evolution chart on the Dashboard with a compact, complete list of groups.
- Simplify Account Totals State Evolution to its expense and income composition analysis.
- Use the Account Group Totals State Evolution right panel as account-level detail for the group and historical month selected in the left summary table.
- Make every available Dashboard group row a link to its corresponding totals report for the selected year and month.
- Export the displayed list as CSV.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `annual-reporting-charts`: annual group and account evolution are presented as complete, drill-down lists rather than dense multi-series charts.

## Impact

- Web presentation, report navigation query handling, component tests, and OpenSpec documentation.
- No changes to reporting APIs, financial calculations, persistence, OpenAPI, or income-versus-expense controls.

## Non-Goals

- Changing daily account-group comparisons.
- Changing the existing account-movement navigation outside the new account-detail list.
- Changing income-versus-expense controls.
- Changing report values, aggregation, or sign calculations.

## Release Impact

Type: patch. This is a backwards-compatible reporting presentation and navigation improvement.
