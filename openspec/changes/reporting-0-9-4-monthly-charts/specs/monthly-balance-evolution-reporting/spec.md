## MODIFIED Requirements

### Requirement: Web Reports UI SHALL Provide Monthly Evolution View
The Web UI MUST provide a dedicated monthly evolution report experience reachable from Reports index, and support monthly focused charts for the selected month context.

#### Scenario: Monthly evolution report is reachable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show a `Monthly Evolution` report entry
- **AND** selecting it MUST navigate to `/reports/monthly-evolution`

#### Scenario: User can switch year and scope in one page
- **WHEN** the user is on `/reports/monthly-evolution`
- **THEN** the page MUST provide a year selector and scope tabs (`Accounts`, `Asset Total`, `Account Groups`)
- **AND** changing year or scope MUST reload the report data for the selected filters

#### Scenario: User can select focused month for monthly chart context
- **WHEN** the user selects a focused month in monthly evolution controls
- **THEN** the report MUST update month-focused chart datasets to the selected month
- **AND** the focused month selection MUST persist while switching expandable table details

#### Scenario: Month-focused chart and table context are consistent
- **WHEN** month-focused charts and summary table rows are shown together
- **THEN** both MUST reference the same selected month context
- **AND** labels MUST clearly indicate the selected month

