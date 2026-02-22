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

#### Scenario: Any selected year returns twelve ordered buckets
- **WHEN** monthly evolution data is requested for year `Y`
- **THEN** the response MUST include months `1` through `12` in ascending order

#### Scenario: Months with no activity are still returned
- **WHEN** no movements exist for a series in month `M`
- **THEN** month `M` MUST still be present in the response
- **AND** `EndBalanceCents` MUST carry forward from month `M-1`

### Requirement: API SHALL Expose Monthly Evolution Contract
The API MUST expose a state evolution endpoint with explicit `year` and `scope` query parameters, and keep a backward-compatible monthly-evolution alias.

#### Scenario: Valid request returns evolution payload through primary endpoint
- **WHEN** an authorized user calls `GET /api/v1/reports/state-evolution?year=YYYY&scope=<scope>`
- **THEN** the API MUST return `200 OK`
- **AND** the response MUST contain graph-ready series and ordered monthly points

#### Scenario: Valid request returns evolution payload through legacy alias
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<scope>`
- **THEN** the API MUST return `200 OK`
- **AND** the response payload shape MUST be equivalent to the primary state-evolution endpoint

#### Scenario: Missing or invalid query parameters are rejected
- **WHEN** a client calls the endpoint with missing or invalid `year` or `scope`
- **THEN** the API MUST return `400 BadRequest`

### Requirement: Web Reports UI SHALL Provide Integrated State Evolution Views
The Web UI MUST provide month-focused chart behavior inside integrated state-evolution tabs and MUST NOT require a dedicated Monthly Evolution route.

#### Scenario: Reports index keeps integrated entry points
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST expose integrated report entries (`Economic State`, `Account Totals`, `Account Group Totals`)
- **AND** month-focused chart flows MUST be reachable through those report tabs without a standalone `/reports/monthly-evolution` entry

#### Scenario: User can select focused month in asset evolution tab
- **WHEN** the user opens `/reports/economic-state` and selects `Asset Evolution`
- **THEN** the view MUST provide focused-month controls for month-level charts
- **AND** changing month MUST reload month-level chart datasets for the selected year/month

#### Scenario: User can select focused month in account group state evolution tab
- **WHEN** the user opens `/reports/account-group-totals` and selects `State Evolution`
- **THEN** the view MUST provide focused-month controls for month-level charts
- **AND** changing month MUST reload month-level chart datasets for the selected year/month

#### Scenario: Month-focused chart and table context are consistent
- **WHEN** month-focused charts and summary rows are shown together in an integrated tab
- **THEN** both MUST reference the same selected month context
- **AND** labels MUST clearly indicate the selected month

### Requirement: Evolution Contract SHALL Be Graph-Ready
The response contract MUST remain machine-friendly and stable for future chart rendering.

#### Scenario: Points remain ordered and numeric
- **WHEN** monthly evolution data is returned
- **THEN** each series MUST expose points ordered by ascending month
- **AND** all balances and deltas MUST be represented in integer cents

#### Scenario: Series identifiers are stable
- **WHEN** a series is returned for the same entity and scope across requests
- **THEN** its `SeriesKey` and entity reference fields MUST remain stable for deterministic client-side chart binding

