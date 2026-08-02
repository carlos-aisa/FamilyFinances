## MODIFIED Requirements

### Requirement: Month-focused chart and table context are consistent
When month-focused charts and summary rows are shown together in an integrated tab, both MUST reference the same selected month context, and labels MUST clearly indicate the selected month. In the Economic State Asset, Income, and Expense Evolution tabs, the Monthly Overview table and CSV export MUST be bounded by the global focused month.

#### Scenario: Asset Evolution overview uses the focused-month cutoff
- **WHEN** a user selects focused month `M` in `/reports/economic-state` and opens Asset Evolution
- **THEN** the daily chart MUST load data for month `M`
- **AND** the Monthly Overview MUST render evolution points for months `1` through `M` only
- **AND** the CSV export MUST contain the same ordered month rows `1` through `M` only

#### Scenario: Income and Expense Evolution overviews use the focused-month cutoff
- **WHEN** a user selects focused month `M` in `/reports/economic-state` and opens Income Evolution or Expense Evolution
- **THEN** the active panel's daily chart MUST load data for month `M`
- **AND** its Monthly Overview and CSV export MUST contain months `1` through `M` only
- **AND** no month after `M` from the annual evolution payload MUST be rendered or exported by that overview

#### Scenario: Historical-year focused month remains an explicit cutoff
- **WHEN** a user selects a past year and focused month `M`
- **THEN** the overview MUST use `M` as its final displayed and exported month even though the annual evolution endpoint contains points through December
- **AND** the context label MUST identify the selected period rather than the system current month
