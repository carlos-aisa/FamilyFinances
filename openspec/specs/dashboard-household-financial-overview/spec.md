# dashboard-household-financial-overview Specification

## Purpose
TBD - created by archiving change dashboard-household-financial-overview. Update Purpose after archive.
## Requirements
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

### Requirement: Dashboard SHALL Provide Current-Month Versus Previous-Month Comparison Semantics
The Dashboard MUST present current-month values and previous-month deltas for core household indicators.

#### Scenario: KPI strip shows mandatory comparison metrics
- **WHEN** KPI data is available for the selected/current month
- **THEN** the Dashboard MUST display `Income`, `Expense`, `Net Result`, `Net Worth`, and `YTD Net`
- **AND** each KPI MUST include a deterministic delta against the previous month

#### Scenario: Net Result semantics are explicit
- **WHEN** the Dashboard renders period net values
- **THEN** net result MUST be computed as `Income - Expense`
- **AND** positive net result MUST indicate income exceeds expense

#### Scenario: YTD Net shows variation versus previous year-end baseline
- **WHEN** the Dashboard displays YTD Net KPI
- **THEN** the value MUST be computed as:
  - current `AssetTotal.ValueCents`
  - minus asset total at `31/12` of the previous year
- **AND** the delta MUST use the current month stock delta (`AssetTotal.DeltaVsPreviousMonthCents`)

#### Scenario: YTD Net is visually distinct from period KPIs
- **WHEN** the Dashboard renders the YTD Net KPI card
- **THEN** it MUST use a distinct border color (warning/yellow) to differentiate it from period-focused KPIs
- **AND** it MUST follow the same card structure as other KPIs (label, value, delta)
- **AND** it MUST display after the Net Worth KPI in the visual order

### Requirement: Dashboard SHALL Expose Data-Sufficiency States For Historical Comparison
The Dashboard MUST explicitly communicate historical sufficiency when year-over-year or comparison-dependent data is missing.

#### Scenario: Insufficient history state is explicit
- **WHEN** same-month last-year data is not available
- **THEN** comparison-dependent blocks MUST show an `insufficient history` state
- **AND** the UI MUST NOT substitute missing values with zero defaults

#### Scenario: Partial history state preserves layout stability
- **WHEN** only part of required comparison data is available
- **THEN** the Dashboard MUST show a `partial history` state
- **AND** dashboard block structure MUST remain stable without layout collapse

### Requirement: Dashboard Desktop Layout SHALL Minimize Scroll For Standard Viewports
The Dashboard MUST follow a fixed analytical layout contract optimized for standard desktop usage.

#### Scenario: Desktop layout targets 2560x1440 no-scroll glanceability
- **WHEN** the Dashboard is rendered at a 2560x1440 viewport in default browser zoom
- **THEN** the KPI strip and the primary analytical rows MUST be visible without vertical page scroll
- **AND** primary analytical interpretation MUST be possible without opening additional pages

#### Scenario: Mobile layout preserves readability without desktop contract assumptions
- **WHEN** the Dashboard is rendered on tablet or mobile breakpoints
- **THEN** blocks MAY stack vertically
- **AND** semantic ordering of KPI-first then chart analysis MUST remain preserved

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

### Requirement: Dashboard SHALL Prefer Trend Charts Over Growing Monthly Tables
Dashboard monthly trend interpretation MUST avoid vertically growing table blocks.

#### Scenario: Monthly net trend replaces monthly net list table
- **WHEN** dashboard monthly net data is rendered
- **THEN** it MUST be shown as a line chart across months (`Income - Expense`)
- **AND** the dashboard MUST NOT depend on a month-by-month net table that can grow in height with historical data

### Requirement: Dashboard SHALL Derive YTD Baseline Using Asset Total As-Of Endpoint
The Dashboard MUST derive YTD baseline from existing asset-total endpoint data, without introducing new backend APIs.

#### Scenario: Previous year-end asset total is queried using existing report endpoint
- **WHEN** YTD Net KPI is calculated
- **THEN** the client MUST request asset total balance for `asOf = 31/12` of previous year
- **AND** the request MUST use existing reports API client infrastructure

#### Scenario: YTD Net value uses stock baseline formula
- **WHEN** both current dashboard overview and previous year-end asset totals are available
- **THEN** YTD Net value MUST be `currentAssetTotal - previousYearEndAssetTotal`
- **AND** the UI MUST format value and delta with signed money formatter and semantic color classes

### Requirement: Dashboard KPI Layout SHALL Accommodate Five KPIs
The Dashboard KPI strip MUST scale to display five KPIs while maintaining responsive behavior and visual consistency.

#### Scenario: Five KPIs display without horizontal overflow
- **WHEN** the Dashboard renders the KPI strip with five KPIs
- **THEN** all five KPIs MUST be visible without horizontal scrolling at any supported breakpoint
- **AND** the layout MUST adapt gracefully from mobile (vertical stack) to desktop (horizontal layout)

#### Scenario: KPI responsive layout handles fifth KPI
- **WHEN** viewport is at small breakpoint (<768px)
- **THEN** all five KPIs MUST stack vertically (one per row)
- **WHEN** viewport is at medium breakpoint (768-1199px)
- **THEN** KPIs MUST display in a 2-2-1 layout (two rows of two, one row of one)
- **WHEN** viewport is at xl breakpoint (>=1200px)
- **THEN** all KPI cards MUST remain visible in the first viewport row/flow with equal-width distribution
- **AND** the layout MUST remain overflow-safe without requiring custom horizontal scrolling

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

