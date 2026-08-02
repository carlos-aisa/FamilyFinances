# Annual account-group heatmap design

## Context and goal

The Dashboard and the Account Group Totals `State Evolution` view currently present account-group monthly results through `AnnualBarChart`. Their dense grouped series make individual bars too narrow to identify, and Chart.js' hover tooltip obscures the same compact plotting area it is meant to explain.

These views must let a household quickly identify which account groups dominate spending in each month. The replacement must show every available account group without requiring a floating legend or a separate data fetch. Annual Income versus Expense comparison controls are explicitly out of scope and remain bar charts.

## Chosen design

Replace the dense account-group `AnnualBarChart` instances with a dedicated reusable `AnnualGroupHeatmap` component. It will render a semantic, keyboard-operable grid in the Dashboard and Account Group Totals `State Evolution` mode:

```text
                       Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec
Food                  [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...
Rent and utilities    [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...
Transport             [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...
...

Selected: May · Food · 982,08 €
```

- Rows contain every account group returned by the existing annual group-evolution response, ordered by total absolute monthly movement descending and then label. The current Dashboard `maxSeries: 8` and Account Group Totals `maxSeries: 12` truncation will be removed for these blocks.
- Columns are the twelve month buckets. Future months after the host's `DataUntilMonth` are visibly unavailable, not interpreted as zero expense.
- A cell's fill intensity represents the absolute monthly movement relative to the largest absolute movement in the displayed matrix. The cell retains a clear border and focus ring; colour is never the only way to identify it.
- Negative movements (for example refunds or corrections) use the existing negative semantic colour family; positive expense movements use the expense family. Both retain intensity scaling, so the matrix does not hide sign semantics.
- The first available cell is selected initially. Pointer click, Enter, and Space select a cell. Arrow keys move across valid cells and update the same selection.
- A fixed one-line detail region below the grid shows the selected month, group label, and signed EUR amount. It replaces the obstructive Chart.js tooltip rather than duplicating it.
- Group labels have a fixed, readable first column and truncate only visually. Their full names remain available through accessible labels. The grid uses compact cells, not a scrollable canvas, so all supplied groups remain visible in the card.

The component will keep each host's title, subtitle, selected-year badge, empty state, and PNG export affordance. PNG export captures the heatmap container through the existing chart-export interop, without adding an export format or an API contract.

## Architecture and data flow

1. `DashboardPage` and `AccountGroupStateEvolutionPanel` continue to load their existing `MonthlyEvolutionScope.AccountGroups` annual data.
2. The existing adapter continues to map every monthly delta into `AnnualChartSeries`; both host invocations stop truncating their series.
3. `AnnualGroupHeatmap` receives `Title`, `Subtitle`, `Year`, `DataUntilMonth`, and the annual series. It builds an internal immutable matrix of labelled rows and monthly cells and calculates the absolute maximum for visual intensity.
4. The component owns only selected-cell UI state. Each host remains the owner of report loading, loading/error states, and data scope.
5. Rendering uses Razor and CSS grid rather than Chart.js. This removes the overlay tooltip entirely and makes selection state deterministic in bUnit tests.

No new endpoint, DTO, persistence change, transaction calculation, or aggregation formula is required.

## Layout and visual direction

The heatmap uses the existing premium dark report surface and chart tokens. It is intentionally quieter than the neighbouring charts: sparse month headers, strong row labels, and a single selected-cell outline are the signature rather than another palette of competing series colours.

At wide Dashboard and report widths, the group-label column remains fixed while the twelve month columns share remaining width. At narrower widths, the component maintains cell hit targets and permits horizontal scrolling within its own matrix only; labels stay visually associated with their rows. The fixed detail region remains visible below the scroller.

## Error and empty states

- The existing `ReportingChartEmptyState` remains when no group series exists.
- A group with only zero movement remains a visible neutral row, so absence and zero do not collapse into the same state.
- A future month has an unavailable treatment and cannot be selected.
- If all valid cells are zero, the grid renders neutral intensity and the detail amount still shows the selected signed zero value.

## Testing

- Extend `DashboardPageTests` and Account Group Totals state-evolution coverage to assert that the heatmap replaces each dense annual-bar instance and receives its year and data cutoff.
- Add focused component tests for row ordering, all-series inclusion beyond eight groups, month/unavailable-cell rendering, intensity/sign classes, pointer and keyboard selection, and the fixed detail text.
- Add accessibility assertions for grid roles, cell labels, focusability, and keyboard navigation.
- Keep the existing Dashboard loading and empty-state coverage; no API tests change because the response contract is reused unchanged.

## Alternatives rejected

1. **100% stacked bars:** shows monthly proportion but requires colour/legend decoding for every group and loses magnitude.
2. **Twelve mini-rankings:** makes individual months readable but breaks the ability to trace one group's evolution across the year.
3. **Keep grouped bars and relocate the tooltip:** avoids the overlay but leaves the primary problem, unreadable narrow bars, intact.

## Scope and acceptance criteria

- The Dashboard and Account Group Totals state-evolution view show every available account group and every valid month in the selected year.
- A user can identify dominant group-month combinations at a glance, then select one without an overlay obscuring the matrix.
- Selection exposes the exact signed EUR amount in a persistent detail region.
- Existing report data, financial semantics, and export formats remain unchanged.
- The component remains compact enough to occupy the existing Dashboard card position and report-panel position; annual Income versus Expense controls remain unchanged.
