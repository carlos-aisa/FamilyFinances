## ADDED Requirements

### Requirement: Dashboard SHALL Render An At-A-Glance Household Financial Overview
The Dashboard MUST render an analytics-first household overview that prioritizes status visibility over navigation shortcuts.

#### Scenario: Dashboard renders required overview blocks
- **WHEN** an authenticated user opens `/`
- **THEN** the Dashboard MUST render a KPI strip and the following chart blocks in the primary viewport:
  - month-focused `Income vs Expense` line chart,
  - annual `Income vs Expense` month-result bar chart,
  - monthly net-balance line chart (`Income - Expense`),
  - asset total evolution chart,
  - annual account-group evolution chart,
  - expense composition pie chart using `Top N + Others`
- **AND** each block MUST be visible without requiring tab interaction

#### Scenario: Dashboard does not duplicate report navigation cards
- **WHEN** the Dashboard overview is rendered
- **THEN** it MUST NOT include report shortcut card entries
- **AND** report deep-dive navigation MUST remain routed through the main navigation menu

### Requirement: Dashboard SHALL Provide Current-Month Versus Previous-Month Comparison Semantics
The Dashboard MUST present current-month values and previous-month deltas for core household indicators.

#### Scenario: KPI strip shows mandatory comparison metrics
- **WHEN** KPI data is available for the selected/current month
- **THEN** the Dashboard MUST display `Income`, `Expense`, `Net Result`, and `Net Worth`
- **AND** each KPI MUST include a deterministic delta against the previous month

#### Scenario: Net Result semantics are explicit
- **WHEN** the Dashboard renders period net values
- **THEN** net result MUST be computed as `Income - Expense`
- **AND** positive net result MUST indicate income exceeds expense

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
Dashboard expense composition MUST remain readable as category count grows.

#### Scenario: Expense composition pie applies Top-N bucketing
- **WHEN** expense composition is rendered for the selected month
- **THEN** only the top `N` expense contributors (`N` between 8 and 10, configurable) SHALL be rendered as individual slices
- **AND** all remaining contributors SHALL be aggregated in a single `Others` slice

#### Scenario: Top-N expense ordering is deterministic
- **WHEN** multiple expense contributors are eligible for Top-N
- **THEN** ordering MUST be deterministic by absolute expense amount descending
- **AND** tie-breaking MUST be stable by contributor label/key

### Requirement: Dashboard SHALL Prefer Trend Charts Over Growing Monthly Tables
Dashboard monthly trend interpretation MUST avoid vertically growing table blocks.

#### Scenario: Monthly net trend replaces monthly net list table
- **WHEN** dashboard monthly net data is rendered
- **THEN** it MUST be shown as a line chart across months (`Income - Expense`)
- **AND** the dashboard MUST NOT depend on a month-by-month net table that can grow in height with historical data
