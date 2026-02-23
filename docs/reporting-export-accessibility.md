# Reporting Export & Accessibility Notes

## Export Behavior

## CSV
- CSV export is available in table-based report views:
  - `Category Totals`
  - `Account Totals` (Period Totals)
  - `Account Group Totals` (Period Totals)
  - `Economic State > Asset Evolution` (monthly overview table)
  - `Economic State > Income Evolution` (monthly overview table)
  - `Account Totals > State Evolution` (accounts overview)
  - `Account Group Totals > State Evolution` (group overview)
- CSV includes:
  - report metadata (report name + UTC timestamp)
  - active filter context
  - table headers + rows
- Numeric values are exported with the same visual formatting used in the UI (`MoneyFormatter` output).

## PNG Chart Export
- All chart cards using `AnnualLineChart`, `MonthlyLineChart`, or `AnnualCompositionChart` include an `Export` button.
- The downloaded image reflects the currently active chart state (filters, selected month/year, active mode).

## Limits
- CSV export is generated client-side.
- CSV output is capped at `10,000` rows per export to prevent browser-memory spikes.
- If row cap is reached, output includes a truncation warning comment.
- If no rows exist for current filters, CSV includes an explicit no-data comment.

## Accessibility Baseline
- Critical report controls use explicit `aria-label` attributes.
- Chart canvases expose:
  - `role="img"`
  - `tabindex="0"`
  - descriptive `aria-label` from chart title
- Export buttons are keyboard-operable native `<button>` elements with explicit labels.

## Responsive Baseline
- Report layouts use responsive Bootstrap grids (`col-12`, `col-md-*`, `col-xxl-*`).
- Export button layout adapts for narrow viewports (stacked/full-width behavior on small screens).
