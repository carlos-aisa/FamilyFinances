## ADDED Requirements

### Requirement: Dashboard SHALL Render An At-A-Glance Household Financial Overview
The Dashboard MUST render an analytics-first household overview that prioritizes status visibility over navigation shortcuts.

#### Scenario: Dashboard renders required overview blocks
- **WHEN** an authenticated user opens `/`
- **THEN** the Dashboard MUST render a KPI strip, a monthly Income vs Expense chart, an account-group current-state chart, an annual YTD accumulation block, and one compact analytical list
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

#### Scenario: Desktop layout targets 1920x1080 glanceability
- **WHEN** the Dashboard is rendered at a 1920x1080 viewport in default browser zoom
- **THEN** the KPI strip and the primary analytical rows MUST be visible with minimized vertical scroll
- **AND** primary analytical interpretation MUST be possible without opening additional pages

#### Scenario: Mobile layout preserves readability without desktop contract assumptions
- **WHEN** the Dashboard is rendered on tablet or mobile breakpoints
- **THEN** blocks MAY stack vertically
- **AND** semantic ordering of KPI-first then chart/list analysis MUST remain preserved
