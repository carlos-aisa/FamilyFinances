# annual-reporting-charts Specification

## Purpose
TBD - created by archiving change reporting-0-9-3-annual-charts. Update Purpose after archive.
## Requirements
### Requirement: Reporting UI SHALL Render Annual Evolution Charts for Implemented State-Evolution Scopes

The reporting UI MUST display annual charts only for implemented state-evolution scopes that retain a chart visualization (`asset-total`).

#### Scenario: Annual evolution chart is shown with selected year data

- **WHEN** an authenticated user opens an annual reporting view with charted evolution for year `Y`
- **THEN** the UI MUST show a chart visualizing monthly evolution across months `1..N` for that year
- **AND** chart points MUST use the same underlying monthly values shown in the corresponding table

### Requirement: Reporting UI SHALL Render Annual Composition Charts for Implemented Scopes
The reporting UI MUST display charted percentage composition in annual reporting views where composition is supported. Annual composition charts MUST keep percentage values for pie geometry while presenting each legend entry as a EUR amount.

#### Scenario: Composition legend exposes monetary slice value
- **WHEN** an annual composition chart renders a side legend
- **THEN** each legend value MUST show the slice's EUR amount using the active UI culture
- **AND** pie slices MUST continue to use percentage values whose total is 100% within rounding tolerance

### Requirement: Annual Report Charts SHALL Follow Premium Comparative Visualization Standards
Annual charts MUST adopt a premium comparative visualization style that is consistent across annual state-evolution and composition contexts, and MUST consume shared token-governed chart primitives.

#### Scenario: Annual chart containers and legends are visually consistent
- **WHEN** annual evolution and composition charts are rendered
- **THEN** chart cards, headers, badges, and legends MUST use shared premium chart styling primitives
- **AND** interaction affordances (hover/focus/export controls) MUST remain consistent across annual chart components
- **AND** annual chart container and legend dimensions MUST resolve via shared sizing tokens

#### Scenario: Annual chart readability is preserved on dashboard and report dark surfaces
- **WHEN** annual charts are displayed in dark mode
- **THEN** axis, grid, and dataset contrast MUST remain readable on premium dark surfaces
- **AND** labels and legends MUST remain legible without changing chart data semantics
- **AND** runtime chart style fallback values MUST come from shared semantic token mappings

#### Scenario: Annual chart Y-axis zero baseline is visually emphasized
- **WHEN** an annual chart Y axis includes tick value `0`
- **THEN** the grid line at `0` MUST be rendered with stronger emphasis than non-zero Y-grid lines
- **AND** non-zero Y-grid lines MUST preserve baseline readability style

#### Scenario: Annual chart money labels use EUR suffix format
- **WHEN** annual chart Y-axis ticks or tooltip values render monetary values
- **THEN** values MUST use European numeric formatting and trailing euro symbol (`XXX,XX €`)
- **AND** sign semantics MUST remain unchanged

### Requirement: Annual Chart Restyling SHALL Preserve Existing Series Contracts
The premium redesign of annual charts MUST preserve existing series identity contracts for key, display name, and point semantics while removing page-specific style literal drift.

#### Scenario: Annual account-group series identity remains stable
- **WHEN** account-group annual evolution charts are rendered with premium styling
- **THEN** each group series MUST keep stable keys and labels
- **AND** restyling MUST NOT alter the mapping between rendered series and underlying monthly values

#### Scenario: Annual semantic colors are resolved centrally
- **WHEN** annual chart datasets are prepared for rendering
- **THEN** semantic series colors and indexed fallback colors MUST be resolved by shared palette helpers
- **AND** page/component-level hardcoded chart color literals MUST NOT be required for default annual chart rendering

### Requirement: Dashboard SHALL Provide Annual Income Versus Expense Month-Result Bars
Dashboard annual comparison MUST visualize monthly operational magnitude for Income and Expense.

#### Scenario: Dashboard annual bars render all month buckets
- **WHEN** dashboard annual Income vs Expense chart is rendered for selected year `Y`
- **THEN** the chart MUST include month buckets January through December in deterministic order
- **AND** each month MUST render paired Income and Expense bars for comparison

#### Scenario: Dashboard annual bars preserve magnitude comparability
- **WHEN** annual Income and Expense bars are rendered in dashboard context
- **THEN** both series MUST use absolute magnitude values to keep bar-height comparability
- **AND** chart subtitles/labels MUST explicitly communicate month-result semantics

### Requirement: Economic State SHALL Provide Annual Month-By-Month Income Versus Expense Bar Comparison
Annual Income versus Expense comparison in Economic State MUST be rendered as month-by-month bars.

#### Scenario: Annual Income vs Expense bars render per month
- **WHEN** annual comparison data is available for selected year `Y`
- **THEN** the UI MUST render bars for Income and Expense for each month bucket
- **AND** the chart MUST use consistent month ordering from January to December

#### Scenario: Annual comparison bars preserve flow semantics
- **WHEN** Income and Expense bars are displayed
- **THEN** series semantics MUST preserve existing income/expense meaning
- **AND** visual restyling MUST NOT alter underlying values

### Requirement: Account Group Annual Evolution SHALL Support Non-Cumulative Month-Result Bar Visualization
Account-group annual evolution in designated group-evolution context MUST support non-cumulative month-result bars.

#### Scenario: Group annual bars show month-result values
- **WHEN** account-group annual evolution is rendered in the updated state-evolution context
- **THEN** each month bar MUST represent that month's result value (not cumulative running total)
- **AND** series identity by group key and label MUST remain stable

#### Scenario: Group annual bar context remains synchronized with selected-month list
- **WHEN** selected-month context is displayed alongside annual group bars
- **THEN** list/chart interpretation MUST remain coherent
- **AND** month-result semantics MUST be clearly labeled to users

### Requirement: Reporting UI SHALL Render Annual Account Group Evolution

The reporting UI MUST present annual account-group evolution on the Dashboard as a complete list of supplied groups. Account Group Totals State Evolution MUST retain its complete group summary on the left and use the right Evolution panel for the selected group's account detail. Each group row MUST show its signed selected-month amount and the UI MUST NOT hide groups through a presentation top-N cap.

#### Scenario: Group list shows every supplied group

- **WHEN** the user opens annual account-group evolution with group data
- **THEN** the Dashboard list and Account Group Totals summary MUST render one row per supplied group series
- **AND** rows MUST be ordered by absolute selected-month amount, then label and stable key
- **AND** every row MUST retain its source label and series identity

#### Scenario: Group nature uses semantic colour

- **WHEN** a group contains income accounts only
- **THEN** its monetary values MUST use the income semantic colour
- **WHEN** a group contains expense accounts only
- **THEN** its monetary values MUST use the expense semantic colour
- **WHEN** a group is mixed or its nature is unavailable
- **THEN** its monetary values MUST use a neutral colour

#### Scenario: Group row opens the matching monthly totals report

- **WHEN** a user follows a Dashboard group row
- **THEN** Account Group Totals MUST open for that group and the annual list's selected year and month
- **AND** the destination MUST use that calendar month as its report range

#### Scenario: Historical group month exposes its member accounts

- **WHEN** a user expands a group history and clicks one of its month rows
- **THEN** the right Evolution panel MUST show only the accounts that belong to that group
- **AND** each account MUST display its signed monthly and year-to-date value from the exact selected month
- **AND** the existing expand/collapse control MUST continue to only control history visibility
- **AND** the account detail rows MUST NOT navigate to account movements

### Requirement: Account Totals State Evolution SHALL Provide Account Composition

Account Totals State Evolution MUST show its expense and income composition analysis directly. It MUST NOT render the annual account evolution list or an Evolution/Composition mode selector.

#### Scenario: Composition is the only chart mode

- **WHEN** a user opens Account Totals State Evolution
- **THEN** the UI MUST render the composition analysis directly
- **AND** it MUST NOT render an annual account evolution list
- **AND** it MUST NOT offer an Evolution/Composition mode selector

#### Scenario: User selects composition nature and month

- **WHEN** a user selects Expense or Income and an available focused month
- **THEN** the composition chart MUST use the existing values for that nature and month

