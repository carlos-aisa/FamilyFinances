## ADDED Requirements

### Requirement: Economic State Filters SHALL Reload Without Explicit Load Action
Economic State filter changes MUST trigger deterministic reload without requiring a manual load button.

#### Scenario: Global load button is not required for economic state refresh
- **WHEN** a user interacts with year or focused-month selectors on `/reports/economic-state`
- **THEN** report data MUST reload automatically after the selection is committed
- **AND** a dedicated `Load report` button MUST NOT be required in that page workflow

### Requirement: Economic State Evolution Tabs SHALL Use A Three-Column Analytical Layout
Asset, Income, and Expense evolution tabs MUST present table, composition, and chart analysis in a consistent three-column layout.

#### Scenario: Evolution tabs render table, composition, and charts in parallel
- **WHEN** the user opens `Asset Evolution`, `Income Evolution`, or `Expense Evolution`
- **THEN** the tab MUST render three analytical columns: monthly table, composition pie, and chart stack
- **AND** each column MUST remain visible in desktop layouts without hiding key chart content below unrelated overflow areas

#### Scenario: Monthly table hides future months for current year
- **WHEN** the selected year is the current year
- **THEN** the monthly table MUST display months only up to the current month
- **AND** months beyond the current month MUST NOT be displayed in that table

#### Scenario: Monthly table keeps full-year list for past years
- **WHEN** the selected year is earlier than the current year
- **THEN** the monthly table MUST include all twelve months
- **AND** ordering MUST remain January through December

#### Scenario: Evolution table uses balance naming and order
- **WHEN** the evolution monthly table is rendered
- **THEN** the previous `Variation vs previous month` column MUST be labeled `Balance`
- **AND** the `Balance` column MUST appear before `End balance`

#### Scenario: Composition pie applies Top-10 plus Others aggregation
- **WHEN** account composition is rendered for the selected month
- **THEN** at most the top ten contributors by absolute magnitude MUST be shown as individual slices
- **AND** remaining contributors MUST be grouped in one `Others` slice
