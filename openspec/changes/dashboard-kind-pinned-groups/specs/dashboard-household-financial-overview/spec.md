## MODIFIED Requirements

### Requirement: Dashboard SHALL Render An At-A-Glance Household Financial Overview

The Dashboard MUST render an analytics-first household overview that prioritizes status visibility over navigation shortcuts.

#### Scenario: Dashboard renders required overview blocks

- **WHEN** an authenticated user opens `/`
- **THEN** the Dashboard MUST render a five-KPI strip and these analytical blocks without tab interaction:
  - current-month `Income vs Expense` daily evolution;
  - annual `Income vs Expense vs Monthly Result` mixed chart;
  - current-month expense-kind ranking with Top 6 + Others;
  - user-pinned account-group operational-result table;
  - asset-total evolution.
- **AND** it MUST NOT require report shortcut cards to reach a financial overview.

#### Scenario: Dashboard preserves distinct monthly and annual questions

- **WHEN** dashboard data is available
- **THEN** the current-month chart MUST retain its daily cumulative progression semantics
- **AND** the annual chart MUST show monthly income, expense, and result in one visual without replacing daily progression.

### Requirement: Dashboard Expense Composition SHALL Aggregate Tail Categories Into Others

The Dashboard MUST show current-month expense composition by account kind, not account group.

#### Scenario: Expense kinds use a fixed Top 6 plus Others

- **WHEN** expense-kind data is rendered for the selected month through its as-of date
- **THEN** the six highest non-zero account kinds MUST appear as individual horizontal bars
- **AND** all remaining non-zero kinds MUST be aggregated into exactly one localized `Others` row when any remain.

#### Scenario: Expense-kind ordering is deterministic

- **WHEN** multiple kinds are eligible for the ranking
- **THEN** rows MUST order by absolute expense amount descending
- **AND** equal amounts MUST use a stable case-insensitive name tie-breaker.

#### Scenario: Expense composition does not use group membership

- **WHEN** an expense account belongs to one or more account groups
- **THEN** its ranking contribution MUST be assigned only to its catalog-backed account kind
- **AND** group membership MUST NOT alter its kind-ranking amount.

### Requirement: Dashboard SHALL Monitor User-Pinned Account Groups

The Dashboard MUST show only account groups explicitly selected for monitoring by the user.

#### Scenario: Pinned group rows show operational result

- **WHEN** one or more groups have `IsDashboardPinned = true`
- **THEN** the Dashboard MUST display each pinned group’s current-month and YTD operational result
- **AND** each result MUST include only Income and Expense member accounts.

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

