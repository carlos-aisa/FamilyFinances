# monthly-reporting-charts Specification

## Purpose
TBD - created by archiving change reporting-0-9-4-monthly-charts. Update Purpose after archive.
## Requirements
### Requirement: API SHALL Provide Month-Level Balance Evolution Dataset
The API MUST expose month-level dataset contracts for chart rendering in integrated state-evolution tabs.

#### Scenario: Month-level endpoint returns daily balance points
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-charts/balance?year=YYYY&month=MM`
- **THEN** the API MUST return `200 OK`
- **AND** the response MUST include ordered daily points for the selected month with deterministic carry-forward semantics for no-activity days

#### Scenario: Invalid month-level query inputs are rejected
- **WHEN** request parameters are missing or invalid (`year`, `month`, or scope)
- **THEN** the API MUST return `400 BadRequest`

### Requirement: API SHALL Provide Month-Level Balance-vs-Group Dataset
The API MUST provide a dataset that compares total balance evolution and account-group evolution for a selected month.

#### Scenario: Balance-vs-group endpoint returns comparable series
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-charts/group-evolution?year=YYYY&month=MM`
- **THEN** the API MUST return `200 OK`
- **AND** the payload MUST include one total-balance series and one series per account group with aligned day buckets
- **AND** the backward-compatible alias `GET /api/v1/reports/monthly-charts/balance-vs-groups?year=YYYY&month=MM` remains available

### Requirement: Web UI SHALL Render Month-Level Charts
The reporting UI MUST render month-level charts in integrated report tabs using the dedicated datasets.

#### Scenario: Focused month selector refreshes monthly charts
- **WHEN** the user changes the selected month in an integrated state-evolution tab
- **THEN** monthly chart requests MUST reload for the selected month
- **AND** rendered chart points MUST match the response payload ordering and values

### Requirement: Monthly Report Charts SHALL Use A Shared Premium Visual Contract
Month-level charts MUST render with a shared premium visual contract for chart container, typography, axes, grid, and tooltip presentation.

#### Scenario: Month-focused chart panels use premium chart framing
- **WHEN** a monthly report chart is rendered in integrated report tabs
- **THEN** chart container, title/subtitle, and action controls MUST use shared premium chart panel styles
- **AND** chart framing MUST remain consistent across monthly chart contexts

#### Scenario: Monthly chart axis and tooltip styling are tokenized
- **WHEN** monthly chart canvases are rendered
- **THEN** axis ticks, grid lines, and tooltip visuals MUST derive from shared design tokens
- **AND** style choices MUST preserve readability in dark mode by default

### Requirement: Monthly Chart Styling SHALL Preserve Existing Data Semantics
Visual upgrades to monthly charts MUST NOT alter the dataset semantics, day ordering, or sign interpretation provided by existing monthly chart endpoints.

#### Scenario: Month selection still drives deterministic chart reload and ordering
- **WHEN** the selected month changes in a report view
- **THEN** monthly chart requests MUST still reload for the selected month and render points in payload order
- **AND** premium chart styling MUST not modify series value meaning or ordering

