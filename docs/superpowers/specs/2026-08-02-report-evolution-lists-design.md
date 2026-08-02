# Report evolution lists design

## Context and goal

The attempted heatmap does not communicate meaningful values, and the trajectory-strip treatment is visually noisy for routine household reporting. Both controls occupy the space of a compact dashboard/report card without providing an operational next action.

The replacement is a pair of concise, data-led lists. They make the selected reporting period and annual accumulation explicit, preserve semantic sign and colour meaning, and let a user open the relevant detail report from the row that prompted the question.

## Chosen design

### Account-group annual evolution

The Dashboard and Account Group State annual-evolution card will render one compact table/list with these columns:

```text
Group                         Month             Year to date
Food                          -982.08 EUR       -5,401.22 EUR
Salary                        +2,140.00 EUR     +14,820.00 EUR
Main accounts                 +300.00 EUR       +1,050.00 EUR
```

- The list contains every supplied account group; it does not hide non-expense groups or apply a presentation top-N limit.
- `Month` is the selected/data-cutoff month's existing `DeltaVsPreviousMonthCents`. `Year to date` is that month's existing `DeltaVsYearStartCents`. No financial calculation changes.
- Income-oriented groups use the existing positive/green semantic presentation, expense-oriented groups use the existing negative/red semantic presentation, and groups outside those families use the existing neutral treatment. The displayed signed amount remains the source of truth; colour is supplementary.
- Rows sort by absolute monthly movement descending, then group label, so the groups that matter most in the active period remain first.
- Every row is a semantic link. It opens Account Group Totals with `groupId`, `year`, and `month` in the URL. The destination initializes the selected group and the whole selected calendar month as its reporting range.
- The control is a table export, so its existing image action becomes `Export CSV` and exports exactly the currently shown rows and period context.

### Account annual evolution

Account State annual evolution will render a compact list with these columns:

```text
Account                       Current balance    Month change      Year to date
Main account                  +1,245.00 EUR     +120.00 EUR       +845.00 EUR
Card                           -620.00 EUR      -80.00 EUR       -340.00 EUR
```

- `Current balance` is the selected/data-cutoff month's `EndBalanceCents`; `Month change` and `Year to date` use the same point's existing delta fields.
- The list contains every supplied account, ordered by absolute month change descending and then account name.
- An account row links to Account Totals using `accountId`, `year`, and `month` in the URL. The destination initializes the selected calendar month and displays the selected account's totals without changing report calculations.
- This is also a table export and uses `Export CSV` rather than `Export PNG`.

### Deliberate scope boundary

- The daily multi-group comparison in Account Group State stays unchanged for now.
- Income-versus-Expense charts, all single-series charts, composition charts, APIs, DTOs, persistence, financial formulas, and existing export formats outside these two replacement lists remain unchanged.
- The rejected heatmap and trajectory-strip components are removed rather than retained as alternate modes.

## Navigation and data flow

1. Dashboard and Account Group State keep loading the existing account-group annual evolution response and retain their year/month cutoff logic.
2. A reusable group-evolution list receives the already adapted group data, selected year, and cutoff month. It derives the two displayed values from the cutoff point and derives the row URL from the stable group entity id.
3. Account State keeps loading its existing account annual evolution response. A reusable account-evolution list derives its three displayed values from the cutoff point and its URL from the stable account entity id.
4. Account Group Totals and Account Totals accept optional URL parameters. When all identifiers are valid, each derives the first day of `year-month` and its exclusive next-month boundary, initializes its normal filters, and loads the same existing report flow as a manually selected month would.
5. Invalid, missing, or unavailable query values leave the destination's normal defaults and filters intact. The URLs remain shareable and reloadable.

No endpoint, DTO, report aggregation, or persistence model changes are required.

## Layout and interaction

The lists retain the existing premium dark cards, heading, subtitle, period badge, loading state, error state, and empty state. A row has a visible hover/focus affordance and a clear link name containing the group/account, period, and key values. The rightmost numeric columns use tabular figures and remain readable at narrow widths through horizontal table scrolling inside the card.

The signature interaction is direct drill-down, not a tooltip: the user scans the list, identifies a meaningful value, and activates its row to continue the investigation. CSV export is adjacent to the period badge and exports the same rows and columns the user sees.

## Error and edge states

- A missing cutoff point leaves the row visible with an explicit unavailable value; it is not treated as zero.
- A zero value remains a real zero and receives neutral numeric treatment.
- A destination query whose group/account no longer exists shows the ordinary selection UI and does not create a phantom filter or error.
- A destination query for a future month is clamped by the existing report-period rules before loading.

## Testing

- Add focused component tests for group and account list ordering, selected-period value mapping, semantic colour classes, accessible link labels, unavailable values, and CSV export behaviour.
- Extend Dashboard and Account State/Group State host tests to assert the list replaces only the approved annual controls; retain coverage that the daily multi-group and Income-versus-Expense charts are unchanged.
- Add Account Group Totals and Account Totals query-initialization tests for valid links, invalid identifiers, selected-month date boundaries, and ordinary no-query defaults.
- Keep adapter/API tests unchanged except for assertions that presentation no longer truncates the affected annual lists.

## Alternatives rejected

1. **Heatmap:** rejected after visual review because blank/intensity cells do not convey operational values clearly.
2. **Trajectory strips:** rejected because they remain chart-like and consume attention without supporting the desired drill-down action.
3. **A row-expansion chart:** rejected because it restores visual complexity instead of making the period and annual values immediately scannable.
4. **Session-only navigation state:** rejected because a URL with group/account and period context can be reloaded, bookmarked, and shared.

## Acceptance criteria

- Every account group is visible in both annual group-list hosts, with selected-month and year-to-date values in existing financial semantics.
- Every account is visible in annual Account State list with current balance, selected-month change, and year-to-date values.
- Income and expense rows retain green and red semantic treatment, respectively; other groups retain neutral treatment.
- Activating a list row opens its appropriate detail report with the same selected year and calendar month applied.
- The two list controls export CSV, while untouched chart controls continue exporting PNG.
- The daily multi-group comparison remains unchanged in this increment.
