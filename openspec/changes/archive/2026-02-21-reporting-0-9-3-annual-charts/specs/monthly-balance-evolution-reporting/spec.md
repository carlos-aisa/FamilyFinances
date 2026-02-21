## MODIFIED Requirements

### Requirement: Web Reports UI SHALL Provide Integrated State Evolution Views
The Web UI MUST provide annual state evolution views integrated in reporting pages and include chart visualizations consistent with selected scope and year.

#### Scenario: Reports index does not expose a standalone monthly evolution card
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST expose `Economic State`, `Account Totals`, and `Account Group Totals` entries
- **AND** the UI MUST NOT require a dedicated `/reports/monthly-evolution` entry for annual evolution access

#### Scenario: Account totals state evolution provides accounts scope controls
- **WHEN** the user opens `/reports/account-totals` and switches to `State Evolution`
- **THEN** the view MUST provide a year selector for accounts evolution datasets
- **AND** changing year MUST reload chart/table data deterministically

#### Scenario: Economic state asset evolution provides asset-total scope controls
- **WHEN** the user opens `/reports/economic-state` and switches to `Asset Evolution`
- **THEN** the view MUST provide a year selector for asset-total evolution datasets
- **AND** changing year MUST reload chart/table data deterministically

#### Scenario: Account group totals state evolution provides account-group scope controls
- **WHEN** the user opens `/reports/account-group-totals` and switches to `State Evolution`
- **THEN** the view MUST provide a year selector for account-group evolution datasets
- **AND** changing year MUST reload chart/table data deterministically

#### Scenario: Chart fallback is shown when series is empty
- **WHEN** no evolution series exists for the selected scope/year
- **THEN** the chart area MUST display an explicit empty-state message
- **AND** the view MUST remain usable for year/tab/filter changes
