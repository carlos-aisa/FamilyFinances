## MODIFIED Requirements

### Requirement: Web Reports UI SHALL Provide Monthly Evolution View
The Web UI MUST provide a dedicated monthly evolution report experience reachable from Reports index, with explicit metric scope semantics for summary cards.

#### Scenario: Monthly evolution report is reachable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show a `Monthly Evolution` report entry
- **AND** selecting it MUST navigate to `/reports/monthly-evolution`

#### Scenario: User can switch year and scope in one page
- **WHEN** the user is on `/reports/monthly-evolution`
- **THEN** the page MUST provide a year selector and scope tabs (`Accounts`, `Asset Total`, `Account Groups`)
- **AND** changing year or scope MUST reload the report data for the selected filters

#### Scenario: Accounts scope summary cards are asset-explicit
- **WHEN** the user views `/reports/monthly-evolution` with scope `Accounts`
- **THEN** top summary cards MUST represent `Asset`-scope end balance and deltas when asset series are available
- **AND** card labels MUST explicitly indicate `Asset` semantics
- **AND** the view MUST avoid silently presenting all-account netted totals as if they were current cash/asset balance

#### Scenario: Non-equivalent metric semantics are disclosed
- **WHEN** the monthly evolution page shows values that are not directly comparable to period net result metrics
- **THEN** the page MUST display an explicit informational disclaimer describing the semantic difference

