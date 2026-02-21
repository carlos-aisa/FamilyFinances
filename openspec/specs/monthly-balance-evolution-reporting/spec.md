# Monthly Balance Evolution Reporting Specification

## Purpose
Define the reporting capability that returns month-by-month year-to-date evolution series for accounts, asset total, and account groups.

## Requirements

### Requirement: System SHALL Provide Monthly YTD Evolution Series For Accounts
The system MUST provide monthly year-to-date evolution points for accounts from January of the selected year.

#### Scenario: Accounts scope returns one series per account
- **WHEN** an authorized user requests monthly evolution with `scope=accounts` for year `Y`
- **THEN** the system MUST return one evolution series per account included in the report scope
- **AND** each series MUST contain monthly points with `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

### Requirement: System SHALL Provide Monthly YTD Evolution Series For Asset Total
The system MUST provide a single aggregated monthly evolution series for all `Asset` accounts.

#### Scenario: Asset total scope aggregates only asset-account balances
- **WHEN** an authorized user requests monthly evolution with `scope=asset-total` for year `Y`
- **THEN** the system MUST return exactly one series
- **AND** that series MUST aggregate balances only from accounts where `AccountNature = Asset`

### Requirement: System SHALL Provide Monthly YTD Evolution Series For Account Groups
The system MUST provide monthly evolution series for account groups using group member accounts as aggregation sources.

#### Scenario: Account groups scope returns one series per group
- **WHEN** an authorized user requests monthly evolution with `scope=account-groups` for year `Y`
- **THEN** the system MUST return one series per account group
- **AND** each group series MUST aggregate monthly balances from member accounts

### Requirement: Evolution Points SHALL Use Deterministic Delta Semantics
The system MUST compute deltas consistently across all scopes and months.

#### Scenario: Delta versus previous month is month-over-month difference
- **WHEN** a monthly point is produced for month `M` (`M > 1`)
- **THEN** `DeltaVsPreviousMonthCents` MUST equal `EndBalanceCents(M) - EndBalanceCents(M-1)`

#### Scenario: Delta versus year start uses prior-year close baseline
- **WHEN** a monthly point is produced for selected year `Y`
- **THEN** `DeltaVsYearStartCents` MUST be computed against the year-start baseline derived from end balance at `Y-01-01` (equivalent to `Y-1-12-31` close)

### Requirement: Evolution Output SHALL Include Continuous Month Buckets
The system MUST return continuous monthly buckets in deterministic order for the selected year window.

#### Scenario: Current year returns months up to current month
- **WHEN** the selected year `Y` equals the current calendar year
- **THEN** the response MUST include months from `1` through the current month
- **AND** months beyond current month MUST NOT be returned

#### Scenario: Historical year returns all twelve months
- **WHEN** the selected year `Y` is before the current calendar year
- **THEN** the response MUST include months `1` through `12`

#### Scenario: Months with no activity are still returned
- **WHEN** no movements exist for a series in month `M`
- **THEN** month `M` MUST still be present in the response
- **AND** `EndBalanceCents` MUST carry forward from month `M-1`

### Requirement: API SHALL Expose Monthly Evolution Contract
The API MUST expose a dedicated monthly evolution endpoint with explicit `year` and `scope` query parameters.

#### Scenario: Valid request returns monthly evolution payload
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<scope>`
- **THEN** the API MUST return `200 OK`
- **AND** the response MUST contain graph-ready series and ordered monthly points

#### Scenario: Missing or invalid query parameters are rejected
- **WHEN** a client calls the endpoint with missing or invalid `year` or `scope`
- **THEN** the API MUST return `400 BadRequest`

### Requirement: Web Reports UI SHALL Provide Monthly Evolution View
The Web UI MUST provide a dedicated monthly evolution report experience reachable from Reports index.

#### Scenario: Monthly evolution report is reachable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show a `Monthly Evolution` report entry
- **AND** selecting it MUST navigate to `/reports/monthly-evolution`

#### Scenario: User can switch year and scope in one page
- **WHEN** the user is on `/reports/monthly-evolution`
- **THEN** the page MUST provide a year selector and scope tabs (`Accounts`, `Asset Total`, `Account Groups`)
- **AND** changing year or scope MUST reload the report data for the selected filters

### Requirement: Evolution Contract SHALL Be Graph-Ready
The response contract MUST remain machine-friendly and stable for future chart rendering.

#### Scenario: Points remain ordered and numeric
- **WHEN** monthly evolution data is returned
- **THEN** each series MUST expose points ordered by ascending month
- **AND** all balances and deltas MUST be represented in integer cents

#### Scenario: Series identifiers are stable
- **WHEN** a series is returned for the same entity and scope across requests
- **THEN** its `SeriesKey` and entity reference fields MUST remain stable for deterministic client-side chart binding
