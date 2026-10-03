## MODIFIED Requirements

### Requirement: Dashboard SHALL Render An At-A-Glance Household Financial Overview

The Dashboard MUST render an analytics-first household overview for the selected calendar month that prioritizes status visibility over navigation shortcuts.

#### Scenario: Dashboard renders required overview blocks

- **WHEN** an authenticated user opens `/`
- **THEN** the Dashboard MUST render a five-KPI strip and these analytical blocks without tab interaction:
  - selected-month `Income vs Expense` daily evolution;
  - annual `Income vs Expense vs Monthly Result` mixed chart through the selected month;
  - asset-total evolution through the selected month;
  - selected-month expense-kind ranking with Top 6 + Others;
  - user-pinned account-group operational-result table for the selected month and year-to-date period; and
  - a latest Expense movement list for the selected month.
- **AND** it MUST NOT require report shortcut cards to reach a financial overview.

#### Scenario: Dashboard uses balanced analytical rows

- **WHEN** the dashboard renders at a wide desktop breakpoint
- **THEN** the daily, annual, and asset-total evolutions MUST share the second row
- **AND** the expense-kind ranking, pinned groups, and latest Expense movements MUST share the third row.

#### Scenario: Dashboard latest expenses use dedicated movement data

- **WHEN** the Dashboard renders its latest Expense movements
- **THEN** it MUST obtain them from the dedicated latest-expenses source for the selected month
- **AND** it MUST NOT derive them from monthly textual insight rows.

#### Scenario: Dashboard preserves distinct monthly and annual questions

- **WHEN** dashboard data is available
- **THEN** the selected-month chart MUST retain its daily cumulative progression semantics
- **AND** the annual chart MUST show monthly income, expense, and result in one visual without replacing daily progression.

## ADDED Requirements

### Requirement: Dashboard SHALL Maintain A Shared Selected Calendar Month

The Dashboard MUST provide one year and month selection that applies to every analytical Dashboard block and persists in the Dashboard URL.

#### Scenario: Dashboard defaults to the active reporting period

- **WHEN** an authenticated user opens the Dashboard without a valid period in the URL
- **THEN** the Dashboard MUST select the current year and current month
- **AND** it MUST request the Dashboard overview using that period.

#### Scenario: Dashboard constrains selectable months

- **WHEN** a user selects the current year
- **THEN** the Dashboard MUST offer calendar months from January through the current month
- **WHEN** a user selects a previous year
- **THEN** the Dashboard MUST offer all twelve calendar months.

#### Scenario: Dashboard period is shareable and reloadable

- **WHEN** a user changes the Dashboard selected year or month
- **THEN** the Dashboard URL MUST contain the selected `year` and `month` query parameters
- **AND** reloading that URL MUST restore the same reporting period.

### Requirement: Dashboard SHALL Preserve Period Context In Analytical Navigation

The Dashboard MUST include its selected year and month when navigating to a compatible analytical report.

#### Scenario: Pinned group opens its selected-period report

- **WHEN** a user activates a pinned account-group name in the Dashboard
- **THEN** the Dashboard MUST navigate to the account-group totals report with the group identifier and selected year and month query parameters.

#### Scenario: Operational navigation remains current-time oriented

- **WHEN** a user activates Dashboard Quick Entry
- **THEN** the application MUST open Quick Entry without applying the Dashboard selected reporting period.
