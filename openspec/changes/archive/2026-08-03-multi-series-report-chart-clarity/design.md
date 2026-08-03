## Context

The existing annual reporting responses already expose one series per account group or account, with end balance, month delta, and delta versus the start of the year. The initial heatmap and trajectory-strip implementation was rejected after visual review because it did not make the values more actionable.

## Decisions

### Present annual group evolution as a complete list

The Dashboard and Account Group Totals State Evolution view render every supplied group in a table with its selected-month result and year-to-date amount. Groups are ordered by the absolute monthly amount, then label and stable key. The existing group nature lookup assigns income-only groups the success colour, expense-only groups the danger colour, and mixed or unknown groups a neutral colour.

### Keep Account Totals State Evolution composition-only

Account Totals State Evolution no longer renders an annual account list or an Evolution/Composition selector. Its chart panel always shows the existing composition analysis, retaining the Expense/Income nature selector and focused-month selector.

### Use historical group-month selection for account detail

Account Group Totals State Evolution does not render a second annual group list in its right panel. The existing View months control continues to only expand or collapse a group's history. Clicking a historical month row selects that group and exact month, switches the right panel to Evolution, and shows only the member accounts with their existing monthly delta and year-to-date delta for that exact point. Before selection, the panel explains how to select a row. The account detail list deliberately does not navigate to account movements.

### Drill down through shareable report URLs

Dashboard group rows link to `account-group-totals`, supplying the selected entity, year, and month. The destination validates the period and loads the requested calendar month. Account Group State historical rows select local account detail instead of navigating. Account navigation is deferred to a future generic change.

### Use CSV for tabular exports

The lists use the existing CSV builder and download interop. The rejected SVG-to-PNG path is removed because no replacement is a chart.

## Boundaries

Daily group comparisons retain their existing multi-series chart. Income-versus-expense views remain unchanged. No report API or financial calculation changes are required.

## Risks

- A complete list can be taller than a chart for very large datasets; it remains readable, sortable by the meaningful current amount, and does not hide entries behind a legend.
- A destination entity might be deleted or unavailable by the time a link opens; the destination falls back to its ordinary, unfiltered report view.
