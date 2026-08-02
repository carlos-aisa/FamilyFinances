# Dense reporting visualizations design

## Context and goal

The Dashboard and the Account Group Totals `State Evolution` view currently present account-group monthly results through `AnnualBarChart`. Their dense grouped series make individual bars too narrow to identify, and Chart.js' hover tooltip obscures the same compact plotting area it is meant to explain.

The application also uses `EvolutionChart` where several account or account-group trajectories share one plot. Intersecting lines make individual paths hard to follow, while the overlay tooltip hides the values it is meant to clarify. This affects the annual account evolution in `AccountStateEvolutionPanel` and the focused-month daily account-group comparison in `AccountGroupStateEvolutionPanel`.

These views must let a household quickly identify which account groups dominate spending in each month, and follow each available account or account-group trajectory without a floating legend. The replacements must not require a new data fetch. Income-versus-Expense comparison controls and any control that renders one trajectory remain unchanged.

## Chosen design: annual group heatmap

Replace the dense account-group `AnnualBarChart` instances with a dedicated reusable `AnnualGroupHeatmap` component. It will render a semantic, keyboard-operable grid in the Dashboard and Account Group Totals `State Evolution` mode:

```text
                       Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec
Food                  [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...
Rent and utilities    [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...
Transport             [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ...

Selected: May · Food · 982.08 EUR
```

- Rows contain every account group returned by the existing annual group-evolution response, ordered by total absolute monthly movement descending and then label. The current Dashboard `maxSeries: 8` and Account Group Totals `maxSeries: 12` truncation will be removed for these blocks.
- Columns are the twelve month buckets. Future months after the host's `DataUntilMonth` are visibly unavailable, not interpreted as zero expense.
- A cell's fill intensity represents the absolute monthly movement relative to the largest absolute movement in the displayed matrix. The cell retains a clear border and focus ring; colour is never the only way to identify it.
- Negative movements, such as refunds or corrections, use the existing negative semantic colour family; positive expense movements use the expense family. Both retain intensity scaling, so the matrix does not hide sign semantics.
- The first available cell is selected initially. Pointer click, Enter, and Space select a cell. Arrow keys move across valid cells and update the same selection.
- A fixed one-line detail region below the grid shows the selected month, group label, and signed EUR amount. It replaces the obstructive Chart.js tooltip rather than duplicating it.
- Group labels have a fixed, readable first column and truncate only visually. Their full names remain available through accessible labels. The grid uses compact cells, not a scrollable canvas, so all supplied groups remain visible in the card.

The component will keep each host's title, subtitle, selected-year badge, empty state, and PNG export affordance. PNG export captures the heatmap container through the existing chart-export interop, without adding an export format or an API contract.

## Chosen design: multi-series trajectory strips

Replace only `EvolutionChart` instances that receive two or more non-Income-versus-Expense series with compact trajectory strips. A strip is an isolated miniature line chart, so each account can be followed without competing line crossings:

```text
Account                         Jan Feb Mar Apr May Jun Jul Aug ... Dec   Current
Main account                     /\____/\__________                         1,245 EUR
Card                             __/_______\______/                          -620 EUR
Savings                          /____/________/____                        3,780 EUR

Selected: Savings · August · 3,420 EUR
```

- `AccountStateEvolutionPanel` uses annual strips for every unfiltered account series. If the user filters to one account, the existing single-series `EvolutionChart` remains in place.
- `AccountGroupStateEvolutionPanel` uses daily strips for the focused-month comparison of account groups. It includes all available non-total groups rather than limiting the view to twelve series.
- The annual and daily variants use their existing strongly typed datasets (`AnnualChartSeries` and `MonthlyChartSeries`) rather than changing report contracts or inventing a cross-domain chart model.
- Each row has its own vertical scale so that meaningful movement in a small account or group remains visible beside a high-balance one. A fixed `Current` value and the selected-point detail expose magnitude explicitly; line height is not presented as cross-row magnitude comparison.
- Rows align to the same month or day positions. Selecting a row or point by pointer, Enter, Space, or arrow keys updates a persistent detail region beneath the list.
- Values up to the host's data cutoff use solid lines. For annual account trajectories, later months retain the current muted dashed carried-forward continuation. For focused-month daily trajectories, days after `DataUntilDay` are unavailable rather than reported as observed values.
- Each row exposes accessible labels with account/group, period, signed EUR amount, and observed-versus-carried-forward state. Colour supplements but never replaces these labels.
- The components retain the source title, subtitle, period badge, empty state, and PNG export behaviour. They use Razor/SVG or CSS-drawn lines rather than Chart.js, so no hover overlay can obscure a trajectory.

## Architecture and data flow

1. `DashboardPage` and `AccountGroupStateEvolutionPanel` continue to load their existing `MonthlyEvolutionScope.AccountGroups` annual data.
2. The existing annual adapter continues to map every monthly delta into `AnnualChartSeries`; both heatmap host invocations stop truncating their series.
3. `AnnualGroupHeatmap` receives `Title`, `Subtitle`, `Year`, `DataUntilMonth`, and the annual series. It builds an internal immutable matrix of labelled rows and monthly cells and calculates the absolute maximum for visual intensity.
4. `AccountStateEvolutionPanel` continues to create annual account series and selects an annual-strip component only when the collection contains multiple series. Its current twelve-series cap is removed in that context.
5. `AccountGroupStateEvolutionPanel` continues to create focused-month daily group series, excludes the existing `asset-total` aggregate, removes its twelve-series cap, and selects the daily-strip component when multiple groups remain.
6. The new presentation components own only selected-point UI state. The hosts remain owners of report loading, loading/error states, and data scope.
7. Rendering uses Razor and CSS/SVG rather than Chart.js. This removes the overlay tooltip and makes selection state deterministic in bUnit tests.

No new endpoint, DTO, persistence change, transaction calculation, or aggregation formula is required.

## Layout and visual direction

The heatmap uses the existing premium dark report surface and chart tokens. It is intentionally quieter than the neighbouring charts: sparse month headers, strong row labels, and a single selected-cell outline are the signature rather than another palette of competing series colours.

Trajectory strips use the same surface but reserve a compact row for each series: a readable label, a low-height plot, and a right-aligned current value. At wide widths, the time headers align across rows. At narrower widths, the list scrolls horizontally within its own card while labels and the fixed detail region remain associated with the selected row. The component does not add a floating legend.

## Error and empty states

- The existing `ReportingChartEmptyState` remains when no group or account series exists.
- A group with only zero movement remains a visible neutral heatmap row, so absence and zero do not collapse into the same state.
- Future heatmap months and daily-strip days are unavailable and cannot be selected.
- If all valid heatmap cells are zero, the grid renders neutral intensity and the detail amount still shows the selected signed zero value.
- A host with one non-Income-versus-Expense series continues to render its existing chart rather than an unnecessary one-row strip.

## Testing

- Extend `DashboardPageTests` and Account Group Totals state-evolution coverage to assert that the heatmap replaces each dense annual-bar instance and receives its year and data cutoff.
- Add focused heatmap component tests for row ordering, all-series inclusion beyond eight groups, month/unavailable-cell rendering, intensity/sign classes, pointer and keyboard selection, and fixed detail text.
- Extend `AccountStateEvolutionPanel` and `AccountGroupStateEvolutionPanel` coverage to assert that only their multi-series contexts render strips, that all eligible series are passed through, and that a single-series context retains `EvolutionChart`.
- Add focused annual and daily strip tests for independent per-row scales, observed versus unavailable/carried-forward segments, current-value formatting, pointer/keyboard selection, and persistent selected-point detail.
- Add accessibility assertions for heatmap grid roles, strip row and point labels, selection state, focusability, and keyboard navigation.
- Keep existing loading and empty-state coverage. No API tests change because response contracts are reused unchanged.

## Alternatives rejected

1. **100% stacked bars:** shows monthly proportion but requires colour/legend decoding for every group and loses magnitude.
2. **Twelve mini-rankings:** makes individual months readable but breaks the ability to trace one group's evolution across the year.
3. **Keep grouped bars and relocate the tooltip:** avoids the overlay but leaves the primary problem, unreadable narrow bars, intact.
4. **Highlight only one line in the existing multi-line chart:** makes that path clearer but hides the requirement to follow all available trajectories.
5. **Horizon charts:** can be compact, but make ordinary household balance changes harder to interpret than isolated line strips.

## Scope and acceptance criteria

- The Dashboard and Account Group Totals state-evolution view show every available account group and every valid month in the selected year.
- A user can identify dominant group-month combinations at a glance, then select one without an overlay obscuring the matrix.
- Every eligible non-Income-versus-Expense multi-line evolution control shows independently readable trajectories for all supplied accounts or groups, with exact selected-point detail and no obstructive overlay.
- Income-versus-Expense controls and single-trajectory controls remain unchanged.
- Existing report data, financial semantics, and export formats remain unchanged.
- The replacements remain compact enough to occupy their existing Dashboard and report-panel positions.
