# Economic state as-of-date design

## Goal

Provide a compact, standalone report that shows the household's economic state on an exact calendar date. It must make the distinction between balances as of that date and the current month's flows to that date explicit.

## User experience

- Add an independent Reports entry named **Economic state as of date**.
- Default the date selector to the current local date and set its maximum value to that same date, so future dates cannot be requested.
- Reload the report when the user applies a valid selected date.
- Render only six summary indicators: assets, liabilities, net worth, month-to-date income, month-to-date expenses, and month-to-date net result.
- Show two explicit contexts with every result:
  - **Balance as of: DD-MM-YYYY** for assets, liabilities, and net worth.
  - **Period: 01-MM-YYYY to DD-MM-YYYY** for income, expenses, and net result.
- Do not include tabs, charts, annual evolution, or monthly-history controls.

## Data and architecture

The page will reuse the existing `ReportsApi.GetEconomicStateAsync(DateOnly asOf)` contract. Its application handler already derives the inclusive flow period from the first day of the selected date's month through the selected date and calculates balance values as of that date. No API endpoint, request/response contract, domain calculation, or persistence change is required.

The new Razor page owns only the selected date, loading state, error state, and loaded `EconomicStateDto`. The existing Economic State page remains unchanged and continues to provide month-focused analysis and annual evolution tabs.

## Scope

Implementation will update:

- the Reports index navigation;
- the new report page and localized resource strings;
- component tests for the default date, future-date limit, historical-date load, and six summary indicators;
- a new OpenSpec change documenting the report behavior.

The work explicitly excludes changes to the existing Economic State report, chart components, reporting API, and report-calculation semantics.

## Testing

- Verify the page initially selects the current date and exposes it as the maximum selectable date.
- Verify a historical date requests the matching `asOf` value and renders the corresponding balance and flow contexts.
- Verify the page renders all six indicators and handles loading and error states consistently with other reports.
- Run focused Web component tests, the complete solution test suite, and strict OpenSpec validation for the implementation change.
