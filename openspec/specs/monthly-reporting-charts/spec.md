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
Month-level charts MUST render with a shared premium visual contract for chart container, typography, axes, grid, tooltip presentation, and semantic palette resolution sourced from shared token governance.

#### Scenario: Month-focused chart panels use premium chart framing
- **WHEN** a monthly report chart is rendered in integrated report tabs
- **THEN** chart container, title/subtitle, and action controls MUST use shared premium chart panel styles
- **AND** chart framing MUST remain consistent across monthly chart contexts
- **AND** chart panel sizing and legend sizing MUST resolve from shared tokens rather than per-view literals

#### Scenario: Monthly chart axis and tooltip styling are tokenized
- **WHEN** monthly chart canvases are rendered
- **THEN** axis ticks, grid lines, and tooltip visuals MUST derive from shared design tokens
- **AND** style choices MUST preserve readability in dark mode by default
- **AND** runtime fallback values MUST be sourced through shared semantic chart mappings

#### Scenario: Monthly chart Y-axis zero baseline is visually emphasized
- **WHEN** a monthly chart Y axis includes tick value `0`
- **THEN** the grid line at `0` MUST be rendered with stronger emphasis than non-zero Y-grid lines
- **AND** non-zero Y-grid lines MUST preserve baseline readability style

#### Scenario: Monthly chart money labels use EUR suffix format
- **WHEN** Y-axis ticks or tooltip values render monetary values
- **THEN** values MUST use European numeric formatting and trailing euro symbol (`XXX,XX €`)
- **AND** sign semantics MUST remain unchanged

### Requirement: Monthly Chart Styling SHALL Preserve Existing Data Semantics
Visual upgrades to monthly charts MUST NOT alter the dataset semantics, day ordering, or sign interpretation provided by existing monthly chart endpoints.

#### Scenario: Month selection still drives deterministic chart reload and ordering
- **WHEN** the selected month changes in a report view
- **THEN** monthly chart requests MUST still reload for the selected month and render points in payload order
- **AND** premium chart styling MUST not modify series value meaning or ordering

### Requirement: Monthly Income-Expense Charts SHALL Be Reusable In Dashboard And Report Contexts
Monthly Income vs Expense chart contracts MUST be reusable across Dashboard and report pages without semantic divergence and without style-contract divergence.

#### Scenario: Dashboard and report render equivalent monthly semantics
- **WHEN** monthly Income vs Expense charts are rendered in Dashboard and Economic State contexts
- **THEN** both views MUST represent equivalent month-relative evolution semantics for the same period
- **AND** both views MUST derive rendered values from the same underlying monthly source values
- **AND** chart series meaning MUST remain consistent across contexts
- **AND** both contexts MUST resolve semantic series colors through shared palette helpers

#### Scenario: Reused chart contract preserves deterministic ordering
- **WHEN** monthly chart datasets are loaded for a selected month
- **THEN** points MUST render in payload order
- **AND** frontend style layers MUST NOT alter day ordering or value interpretation

### Requirement: Monthly Dashboard Chart SHALL Respect Fixed-Height Analytical Layout Contract
The dashboard monthly chart container MUST support constrained fixed-height rendering for glanceability.

#### Scenario: Fixed-height chart remains readable in dashboard contract
- **WHEN** the dashboard monthly chart is rendered in the defined desktop analytical layout
- **THEN** the chart MUST remain readable without requiring internal scroll
- **AND** axis/tooltip styling MUST preserve contrast and legibility in dark mode

#### Scenario: Constrained chart layout avoids tab-based overflow patterns
- **WHEN** dashboard analytical blocks are rendered together
- **THEN** monthly chart placement MUST not depend on tabbed grouping
- **AND** core chart interpretation MUST remain available in the primary dashboard viewport

### Requirement: Dashboard SHALL Provide Monthly Net-Balance Trend As A Line Chart
Dashboard monthly net interpretation MUST be available as a charted month series.

#### Scenario: Monthly net trend renders deterministic month ordering
- **WHEN** monthly net trend is rendered for selected year `Y`
- **THEN** points MUST be ordered from January through December
- **AND** each point value MUST represent `Income - Expense` for the corresponding month

#### Scenario: Monthly net trend replaces growth-prone monthly net tables
- **WHEN** dashboard monthly net information is displayed in cockpit mode
- **THEN** it MUST be displayed as a line chart block
- **AND** it MUST NOT require a vertically growing month-list table as the primary dashboard representation

### Requirement: Month-Focused Daily Evolution SHALL Normalize Against Month Opening Balance
Month-focused daily evolution charts MUST use month opening balance as the baseline so evolution starts at zero while preserving daily movement semantics.

#### Scenario: Day-1 movement is preserved while baseline starts at zero
- **WHEN** a monthly chart includes transactions on day 1
- **THEN** the rendered value at day 1 MUST include day-1 movement relative to month opening balance
- **AND** the chart MUST NOT suppress day-1 movement by subtracting the first rendered point as baseline

#### Scenario: Opening balance metadata is additive and deterministic
- **WHEN** monthly chart payloads are prepared for frontend rendering
- **THEN** payloads MAY include additive `OpeningBalanceCents` metadata for baseline normalization
- **AND** this metadata MUST NOT alter point ordering, day buckets, or source value determinism

