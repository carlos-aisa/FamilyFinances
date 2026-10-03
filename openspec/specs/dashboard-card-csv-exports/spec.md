# dashboard-card-csv-exports Specification

## Purpose
TBD - created by archiving change dashboard-latest-expenses-and-csv-exports. Update Purpose after archive.
## Requirements
### Requirement: Dashboard Third-Row Data Cards SHALL Export CSV

The highlighted-groups, expense-kind-ranking, and latest-expenses Dashboard cards MUST each provide a functional CSV export button using their visible data columns.

#### Scenario: User exports highlighted groups

- **WHEN** a user activates the highlighted-groups export button
- **THEN** the browser MUST receive a CSV download containing group, month, and annual-accumulation columns
- **AND** Expense-kind group amounts MUST be exported as non-negative magnitudes

#### Scenario: User exports expense-kind ranking

- **WHEN** a user activates the expense-kind-ranking export button
- **THEN** the browser MUST receive a CSV download containing description and amount columns

#### Scenario: User exports latest expenses

- **WHEN** a user activates the latest-expenses export button
- **THEN** the browser MUST receive a CSV download containing date, description, and amount columns
- **AND** dates MUST use the stable `yyyy-MM-dd` representation

### Requirement: Dashboard Card Exports SHALL Include Selected Period Context

Each third-row Dashboard CSV export MUST include the currently selected Dashboard period as export context.

#### Scenario: CSV identifies the selected period

- **WHEN** a third-row Dashboard CSV file is generated
- **THEN** its metadata MUST include a localized period label and the Dashboard period in `MM-yyyy` form

### Requirement: Dashboard Card CSV Exports SHALL Match The Selected Period Data

Dashboard CSV exports MUST contain the same selected-period rows that are visible in their source cards.

#### Scenario: Latest expenses export is scoped to the selected month

- **WHEN** a user exports latest expenses after selecting a Dashboard month
- **THEN** every exported movement MUST belong to that calendar month
- **AND** the export filename and metadata MUST identify the same selected period.

#### Scenario: Dashboard period change refreshes card export data

- **WHEN** a user changes the Dashboard selected period before exporting a card
- **THEN** the generated CSV MUST use the refreshed card data for the newly selected period
- **AND** it MUST NOT include rows from the previously selected period.
