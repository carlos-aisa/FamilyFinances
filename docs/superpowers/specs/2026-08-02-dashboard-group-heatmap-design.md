# Dense annual reporting visualizations design

## Context and goal

The Dashboard and the Account Group Totals `State Evolution` view currently present account-group monthly results through `AnnualBarChart`. Their dense grouped series make individual bars too narrow to identify, and Chart.js' hover tooltip obscures the same compact plotting area it is meant to explain. The annual multi-account evolution view has the equivalent problem with intersecting lines and the same overlay tooltip.

These views must let a household quickly identify which account groups dominate spending in each month, and follow the annual trajectory of several accounts. The replacements must not require a floating legend or a separate data fetch. Annual Income versus Expense comparison controls, single-series annual lines, and daily charts are explicitly out of scope and remain unchanged.

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

## Account trajectories: chosen design

Replace the multi-series annual `EvolutionChart` in `AccountStateEvolutionPanel` with `AnnualAccountTrajectoryStrips`. The component renders one compact line strip per account, aligned to the same twelve month headers:

```text
Account                         Jan Feb Mar Apr May Jun Jul Aug ... Dec   Current
Main account                     ╱╲────╱╲─────────────── ···········     1,245 €
Card                             ──╲──────╲─────╱──────── ·········      -620 €
Savings                          ╱────╱──────────╱──────── ·········    3,780 €

Selected: Savings · August · 3,420 €
```

- Every account series currently supplied by the selected account context appears as a labelled row. When the host explicitly filters to one account, the same component renders that one trajectory rather than reintroducing the legacy chart.
- Every row has its own vertical scale so that meaningful changes in a small account remain visible beside high-balance accounts. The fixed `Current` value makes magnitude explicit, and the component does not claim that line height is comparable across rows.
- Actual values use a solid line through `DataUntilMonth`. The remaining months repeat the final known balance only as a muted dashed continuation, preserving the current chart's context without representing it as observed data.
- Rows share month positions, so a vertical selection marker aligns the selected month across all strips. Selecting a row or point with pointer, Enter, Space, or arrow keys updates a persistent detail region beneath the list.
- Each row exposes an accessible name containing account, month, signed EUR amount, and observed-versus-carried-forward state. Colour supplements but never replaces these labels.
- The component retains the source title, subtitle, period badge, empty state, and PNG export behaviour. It uses Razor/SVG or CSS-drawn lines rather than Chart.js, so no hover overlay can obscure a trajectory.

## Account-trajectory testing

- Extend `AccountStateEvolutionPanel` coverage to assert that its dense annual `EvolutionChart` is replaced only in the multi-account annual context.
- Add focused component tests for one-row and multi-row rendering, independent per-row scales, actual versus dashed carried-forward segments, current-value formatting, pointer/keyboard selection, and persistent selected-point detail.
- Add accessibility assertions for row and point labels, selection state, keyboard navigation, and reduced-motion-safe rendering.

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

For account trajectories, overlaying only the selected account on the existing multi-line chart was rejected because it hides the other trajectories; a horizon chart was rejected because it makes ordinary household balance changes harder to read than isolated line strips.

## Scope and acceptance criteria

- The Dashboard and Account Group Totals state-evolution view show every available account group and every valid month in the selected year.
- A user can identify dominant group-month combinations at a glance, then select one without an overlay obscuring the matrix.
- Selection exposes the exact signed EUR amount in a persistent detail region.
- Existing report data, financial semantics, and export formats remain unchanged.
- The component remains compact enough to occupy the existing Dashboard card position and report-panel position; annual Income versus Expense controls remain unchanged.
- The annual multi-account view shows one independently readable trajectory per available account with exact selected-point detail and no obstructive overlay.
