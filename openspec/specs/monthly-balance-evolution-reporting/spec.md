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
The Web UI MUST provide month-focused chart behavior inside integrated state-evolution tabs and MUST NOT require a dedicated Monthly Evolution route. When month-focused charts and summary rows are shown together in an integrated tab, both MUST reference the same selected month context, and labels MUST clearly indicate the selected month. In the Economic State Asset, Income, and Expense Evolution tabs, the Monthly Overview table and CSV export MUST be bounded by the global focused month.

#### Scenario: Asset Evolution overview uses the focused-month cutoff
- **WHEN** a user selects focused month `M` in `/reports/economic-state` and opens Asset Evolution
- **THEN** the daily chart MUST load data for month `M`
- **AND** the Monthly Overview MUST render evolution points for months `1` through `M` only
- **AND** the CSV export MUST contain the same ordered month rows `1` through `M` only

#### Scenario: Income and Expense Evolution overviews use the focused-month cutoff
- **WHEN** a user selects focused month `M` in `/reports/economic-state` and opens Income Evolution or Expense Evolution
- **THEN** the active panel's daily chart MUST load data for month `M`
- **AND** its Monthly Overview and CSV export MUST contain months `1` through `M` only
- **AND** no month after `M` from the annual evolution payload MUST be rendered or exported by that overview

#### Scenario: Historical-year focused month remains an explicit cutoff
- **WHEN** a user selects a past year and focused month `M`
- **THEN** the overview MUST use `M` as its final displayed and exported month even though the annual evolution endpoint contains points through December
- **AND** the context label MUST identify the selected period rather than the system current month

#### Scenario: Income and Expense composition uses selected-month movement
- **WHEN** a user views Income or Expense composition for focused month `M` in `/reports/economic-state`
- **THEN** every composition slice MUST use that entity's absolute `DeltaVsPreviousMonthCents` for month `M`
- **AND** the composition MUST NOT use the entity's cumulative end balance as the monthly slice value

### Requirement: Evolution Contract SHALL Be Graph-Ready
The response contract MUST remain machine-friendly and stable for future chart rendering.

#### Scenario: Points remain ordered and numeric
- **WHEN** monthly evolution data is returned
- **THEN** each series MUST expose points ordered by ascending month
- **AND** all balances and deltas MUST be represented in integer cents

#### Scenario: Series identifiers are stable
- **WHEN** a series is returned for the same entity and scope across requests
- **THEN** its `SeriesKey` and entity reference fields MUST remain stable for deterministic client-side chart binding

### Requirement: System SHALL Provide Monthly YTD Evolution Series For Expense Total
The system MUST provide a monthly year-to-date evolution series for aggregate expense scope.

#### Scenario: Expense total scope returns single deterministic series
- **WHEN** an authorized user requests monthly evolution with `scope=expense-total` for year `Y`
- **THEN** the system MUST return exactly one expense-total series
- **AND** each point MUST include `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

#### Scenario: Expense total scope rejects invalid query combinations
- **WHEN** a request includes invalid `year` or unsupported scope token for expense-total evolution
- **THEN** the API MUST return `400 BadRequest`
- **AND** no partial payload MUST be emitted

### Requirement: Group Evolution Read Models SHALL Expose Exact Selected-Month Balance Context
Group-evolution read models MUST support exact selected-month balance interpretation for list/chart coordination.

#### Scenario: Selected-month list context uses exact month balance
- **WHEN** group evolution is rendered for selected month `M`
- **THEN** the list context MUST expose exact balance for month `M`
- **AND** the value MUST align with the underlying evolution series for that same month

#### Scenario: Selected-month context remains deterministic across refresh
- **WHEN** selected month changes and data reload occurs
- **THEN** list and chart contexts MUST reference the same selected month
- **AND** month context labels MUST remain explicit

