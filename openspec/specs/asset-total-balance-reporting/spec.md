# Asset Total Balance Reporting Specification

## Purpose
Define the reporting capability that returns the aggregated balance of all `Asset` accounts as of a selected date.

## Requirements

### Requirement: System SHALL Compute Total Asset Balance As-Of a Date
The system MUST compute the total balance of all accounts with `AccountNature.Asset` using all posted splits booked on or before the requested as-of date.

#### Scenario: Total is calculated from asset-account splits only
- **WHEN** a client requests the asset total balance for date `D`
- **THEN** the system MUST include only splits whose account nature is `Asset`
- **AND** the system MUST exclude splits from `Liability`, `Income`, `Expense`, and `Equity` accounts

#### Scenario: As-of date is inclusive
- **WHEN** a split belongs to a transaction with `BookedOn = D`
- **THEN** that split MUST be included in the total for as-of date `D`

#### Scenario: No asset data returns zero
- **WHEN** there are no asset-account splits booked on or before date `D`
- **THEN** the system MUST return `TotalCents = 0`

### Requirement: API SHALL Expose Asset Total Balance Report Contract
The API MUST expose a dedicated read endpoint for this report.

#### Scenario: Successful report retrieval
- **WHEN** an authorized user calls `GET /api/v1/reports/asset-total-balance?asOf=YYYY-MM-DD`
- **THEN** the API MUST return `200 OK`
- **AND** the response MUST contain `AsOf`, `TotalCents`, and `AssetAccountsCount`

#### Scenario: Missing as-of date is rejected
- **WHEN** a client calls the endpoint without `asOf`
- **THEN** the API MUST return `400 BadRequest`

### Requirement: Web Reports UI SHALL Provide Asset Total Balance View
The Web UI MUST provide a dedicated report screen for this capability.

#### Scenario: Report is reachable from Reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the page MUST show an entry for `Asset Total Balance`
- **AND** selecting it MUST navigate to `/reports/asset-total-balance`

#### Scenario: User can query a date and see total
- **WHEN** the user selects an as-of date and executes the report
- **THEN** the UI MUST call the corresponding API endpoint
- **AND** the UI MUST display the returned total as currency
