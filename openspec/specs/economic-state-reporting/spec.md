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

### Requirement: Economic State Snapshot SHALL Use A Premium Semantic Metric Layout
The economic-state snapshot view MUST render stock and flow metrics with a consistent premium card system while preserving current KPI semantics and values.

#### Scenario: Stock and flow families remain semantically separated
- **WHEN** `/reports/economic-state` snapshot data is displayed
- **THEN** stock metrics (`Assets`, `Liabilities`, `Net Worth`) and flow metrics (`Income`, `Expense`, `Period Net Result`) MUST remain visually distinguishable as separate semantic families
- **AND** card styling MUST use shared premium metric-card primitives

#### Scenario: Premium styling does not change KPI values
- **WHEN** snapshot KPI cards are rendered in premium mode
- **THEN** displayed values MUST match the existing economic-state payload values
- **AND** no additional transformation of report semantics MUST be introduced

### Requirement: Economic State Tabs SHALL Retain Existing Behavior With Premium Navigation Styling
The economic-state tabs MUST keep deterministic data-loading behavior while supporting snapshot, asset evolution, income evolution, and expense evolution views.

#### Scenario: Tab switching behavior remains deterministic with expense parity
- **WHEN** a user switches between `Snapshot`, `Asset Evolution`, `Income Evolution`, and `Expense Evolution` tabs
- **THEN** active tab behavior and rendered panel content MUST remain deterministic
- **AND** each tab MUST load the corresponding report data scope without semantic drift

### Requirement: Economic State Context Blocks SHALL Use Compact Premium Informational Presentation
The economic-state informational blocks MUST remain explicit and readable while using compact premium styling that reduces visual clutter.

#### Scenario: Metric explanation blocks stay explicit and readable
- **WHEN** the report renders explanatory stock/flow context text
- **THEN** semantic explanation content MUST remain visible and explicit
- **AND** informational blocks MUST use premium styling that preserves readability in dark mode

### Requirement: Economic State Snapshot SHALL Include Monthly Net List And Annual Income-Expense Comparison
The economic-state snapshot MUST include compact monthly net listing and annual Income vs Expense bar comparison for quick interpretation.

#### Scenario: Snapshot shows monthly net list semantics
- **WHEN** snapshot data is rendered for selected period context
- **THEN** the view MUST display a monthly net list computed as `Income - Expense`
- **AND** list values MUST preserve existing sign semantics

#### Scenario: Snapshot shows annual Income vs Expense bars
- **WHEN** annual comparison data is available in economic-state snapshot context
- **THEN** the UI MUST render a month-by-month Income vs Expense bar chart
- **AND** chart labels MUST clearly communicate monthly (non-aggregated-per-bar) semantics

### Requirement: Economic State Summary SHALL Preserve Readability Under Limited History
Economic-state summary blocks MUST remain explicit when historical comparison data is sparse.

#### Scenario: Sparse history shows explicit comparison state
- **WHEN** same-month-last-year data is unavailable for summary comparisons
- **THEN** summary comparison blocks MUST display an explicit insufficient-history state
- **AND** visual layout MUST remain stable and readable

