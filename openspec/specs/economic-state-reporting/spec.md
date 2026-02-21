# economic-state-reporting Specification

## Purpose
TBD - created by archiving change reporting-0-9-2-economic-state-and-dashboard-entry. Update Purpose after archive.
## Requirements
### Requirement: API SHALL Provide Economic State Read Model
The API MUST expose a dedicated economic-state report endpoint that combines stock and flow KPIs for a selected as-of date.

#### Scenario: Economic state endpoint returns combined KPI payload
- **WHEN** an authorized user calls `GET /api/v1/reports/economic-state?asOf=YYYY-MM-DD`
- **THEN** the API MUST return `200 OK`
- **AND** the payload MUST include `AsOf`, `AssetsTotalCents`, `LiabilitiesTotalCents`, `NetWorthCents`, `IncomeTotalCents`, `ExpenseTotalCents`, and `PeriodNetResultCents`

#### Scenario: Missing or invalid as-of value is rejected
- **WHEN** a client calls `GET /api/v1/reports/economic-state` with missing or invalid `asOf`
- **THEN** the API MUST return `400 BadRequest`

### Requirement: Web UI SHALL Provide Economic State Report Page
The Web UI MUST provide a dedicated report page to visualize the economic state snapshot.

#### Scenario: Economic state report page is reachable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show an `Economic State` report entry
- **AND** selecting it MUST navigate to `/reports/economic-state`

#### Scenario: Economic state report defaults to current date
- **WHEN** the user opens `/reports/economic-state`
- **THEN** the as-of date filter MUST default to the current date
- **AND** KPI cards MUST be loaded for that date without requiring extra navigation

#### Scenario: Economic state report shows explicit KPI semantics
- **WHEN** economic-state data is rendered
- **THEN** KPI cards MUST display explicit labels for `Assets`, `Liabilities`, `Net Worth`, `Income`, `Expense`, and `Period Net Result`
- **AND** stock metrics and flow metrics MUST be visually identifiable as different semantic families

