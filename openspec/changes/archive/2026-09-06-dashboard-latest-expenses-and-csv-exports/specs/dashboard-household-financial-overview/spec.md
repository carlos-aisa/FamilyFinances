## MODIFIED Requirements

### Requirement: Dashboard SHALL Render An At-A-Glance Household Financial Overview

The Dashboard MUST render an analytics-first household overview that prioritizes status visibility over navigation shortcuts.

#### Scenario: Dashboard renders required overview blocks

- **WHEN** an authenticated user opens `/`
- **THEN** the Dashboard MUST render a five-KPI strip and these analytical blocks without tab interaction:
  - current-month `Income vs Expense` daily evolution;
  - annual `Income vs Expense vs Monthly Result` mixed chart;
  - asset-total evolution;
  - current-month expense-kind ranking with Top 6 + Others;
  - user-pinned account-group operational-result table; and
  - a latest Expense movement list.
- **AND** it MUST NOT require report shortcut cards to reach a financial overview.

#### Scenario: Dashboard uses balanced analytical rows

- **WHEN** the dashboard renders at a wide desktop breakpoint
- **THEN** the daily, annual, and asset-total evolutions MUST share the second row
- **AND** the expense-kind ranking, pinned groups, and latest Expense movements MUST share the third row.

#### Scenario: Dashboard latest expenses use dedicated movement data

- **WHEN** the Dashboard renders its latest Expense movements
- **THEN** it MUST obtain them from the dedicated latest-expenses source
- **AND** it MUST NOT derive them from monthly textual insight rows.

#### Scenario: Dashboard preserves distinct monthly and annual questions

- **WHEN** dashboard data is available
- **THEN** the current-month chart MUST retain its daily cumulative progression semantics
- **AND** the annual chart MUST show monthly income, expense, and result in one visual without replacing daily progression.

### Requirement: Dashboard SHALL Monitor User-Pinned Account Groups

The Dashboard MUST show only account groups explicitly selected for monitoring by the user.

#### Scenario: Pinned group rows show operational result

- **WHEN** one or more groups have `IsDashboardPinned = true`
- **THEN** the Dashboard MUST display each pinned group’s current-month and YTD operational result
- **AND** each result MUST include only Income and Expense member accounts.

#### Scenario: Pinned groups order by current-month operational result

- **WHEN** pinned group rows are returned for the Dashboard
- **THEN** they MUST be ordered by monthly operational result ascending.

#### Scenario: Expense group metrics use neutral magnitude display

- **WHEN** a pinned group is classified as an Expense metric
- **THEN** its current-month and YTD values MUST be shown as non-negative magnitudes
- **AND** those values MUST NOT use unfavorable-result color semantics.

#### Scenario: Balance-account natures are excluded from group operational result

- **WHEN** a pinned group includes Asset, Liability, or Equity accounts
- **THEN** those accounts MUST NOT contribute to its dashboard operational result.

#### Scenario: Overlapping groups remain independent monitoring views

- **WHEN** one account belongs to multiple pinned groups
- **THEN** its eligible flow contribution MUST appear in each relevant group row
- **AND** the Dashboard MUST NOT show group percentages or an aggregate total across group rows.

#### Scenario: No pinned groups has an actionable empty state

- **WHEN** no group is pinned
- **THEN** the Dashboard MUST render a localized compact empty state
- **AND** it MUST direct the user to account-group management.
