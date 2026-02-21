## ADDED Requirements

### Requirement: Reporting UI SHALL Render Annual Evolution Chart for Core Metrics
The reporting UI MUST display an annual chart that visualizes year-to-date monthly evolution of core economic metrics.

#### Scenario: Annual evolution chart is shown with selected year data
- **WHEN** an authenticated user opens annual reporting views for year `Y`
- **THEN** the UI MUST show a chart visualizing monthly evolution across months `1..N` for that year
- **AND** chart points MUST use the same underlying monthly values shown in the corresponding table

### Requirement: Reporting UI SHALL Render Annual Account Group Evolution Chart
The reporting UI MUST provide a chart for annual account-group evolution.

#### Scenario: Group evolution chart reflects account-group series
- **WHEN** the user selects account-group scope in annual reporting
- **THEN** the UI MUST render a multi-series evolution chart using account-group monthly values
- **AND** each group series MUST remain stable by series key and display name

### Requirement: Reporting UI SHALL Render Annual Composition Charts for Expense and Income Groups
The reporting UI MUST display charted percentage composition for expense and income groups at annual level.

#### Scenario: Expense composition shows percentage split by group
- **WHEN** annual expense-group data is available
- **THEN** the UI MUST render a percentage composition chart for expense groups
- **AND** percentages MUST sum to 100% within rounding tolerance

#### Scenario: Income composition shows percentage split by group
- **WHEN** annual income-group data is available
- **THEN** the UI MUST render a percentage composition chart for income groups
- **AND** percentages MUST sum to 100% within rounding tolerance

