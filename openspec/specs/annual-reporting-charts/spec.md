# annual-reporting-charts Specification

## Purpose
TBD - created by archiving change reporting-0-9-3-annual-charts. Update Purpose after archive.
## Requirements
### Requirement: Reporting UI SHALL Render Annual Evolution Charts for Implemented State-Evolution Scopes
The reporting UI MUST display annual charts that visualize year-to-date monthly evolution for implemented state-evolution scopes (`asset-total`, `accounts`, `account-groups`).

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

### Requirement: Reporting UI SHALL Render Annual Composition Charts for Implemented Scopes
The reporting UI MUST display charted percentage composition in annual reporting views where composition is supported.

#### Scenario: Account-group composition shows percentage split for expense-oriented groups
- **WHEN** annual expense-group data is available
- **THEN** the UI MUST render a percentage composition chart for expense-oriented account groups
- **AND** percentages MUST sum to 100% within rounding tolerance

#### Scenario: Accounts composition shows percentage split for expense and income natures
- **WHEN** annual accounts data is available
- **THEN** the UI MUST render composition charts for `Expense` and `Income` natures in accounts state evolution
- **AND** percentages MUST sum to 100% within rounding tolerance

### Requirement: Annual Report Charts SHALL Follow Premium Comparative Visualization Standards
Annual charts MUST adopt a premium comparative visualization style that is consistent across annual state-evolution and composition contexts.

#### Scenario: Annual chart containers and legends are visually consistent
- **WHEN** annual evolution and composition charts are rendered
- **THEN** chart cards, headers, badges, and legends MUST use shared premium chart styling primitives
- **AND** interaction affordances (hover/focus/export controls) MUST remain consistent across annual chart components

#### Scenario: Annual chart readability is preserved on dashboard and report dark surfaces
- **WHEN** annual charts are displayed in dark mode
- **THEN** axis, grid, and dataset contrast MUST remain readable on premium dark surfaces
- **AND** labels and legends MUST remain legible without changing chart data semantics

### Requirement: Annual Chart Restyling SHALL Preserve Existing Series Contracts
The premium redesign of annual charts MUST preserve existing series identity contracts for key, display name, and point semantics.

#### Scenario: Annual account-group series identity remains stable
- **WHEN** account-group annual evolution charts are rendered with premium styling
- **THEN** each group series MUST keep stable keys and labels
- **AND** restyling MUST NOT alter the mapping between rendered series and underlying monthly values

