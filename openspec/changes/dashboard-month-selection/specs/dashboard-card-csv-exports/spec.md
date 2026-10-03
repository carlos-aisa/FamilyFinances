## ADDED Requirements

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
