## MODIFIED Requirements

### Requirement: Web Reports UI SHALL Provide Monthly Evolution View
The Web UI MUST provide a dedicated monthly evolution report experience reachable from Reports index and include annual chart visualizations consistent with selected scope/year.

#### Scenario: Monthly evolution report is reachable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show a `Monthly Evolution` report entry
- **AND** selecting it MUST navigate to `/reports/monthly-evolution`

#### Scenario: User can switch year and scope in one page
- **WHEN** the user is on `/reports/monthly-evolution`
- **THEN** the page MUST provide a year selector and scope tabs (`Accounts`, `Asset Total`, `Account Groups`)
- **AND** changing year or scope MUST reload the report data for the selected filters

#### Scenario: Annual chart reflects currently selected scope and year
- **WHEN** the user changes monthly evolution scope or year
- **THEN** the annual chart dataset MUST be recalculated from the loaded monthly evolution series
- **AND** chart month points MUST match the table month rows for that scope/year

#### Scenario: Chart fallback is shown when series is empty
- **WHEN** no evolution series exists for the selected scope/year
- **THEN** the chart area MUST display an explicit empty-state message
- **AND** the page MUST remain usable for filter changes

