## ADDED Requirements

### Requirement: API SHALL Provide Month-Level Balance Evolution Dataset
The API MUST expose month-level dataset contracts for chart rendering in integrated state-evolution tabs.

#### Scenario: Month-level endpoint returns daily balance points
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-charts/balance?year=YYYY&month=MM`
- **THEN** the API MUST return `200 OK`
- **AND** the response MUST include ordered daily points for the selected month with deterministic carry-forward semantics for no-activity days

#### Scenario: Invalid month-level query inputs are rejected
- **WHEN** request parameters are missing or invalid (`year`, `month`, or scope)
- **THEN** the API MUST return `400 BadRequest`

### Requirement: API SHALL Provide Month-Level Balance-vs-Group Dataset
The API MUST provide a dataset that compares total balance evolution and account-group evolution for a selected month.

#### Scenario: Balance-vs-group endpoint returns comparable series
- **WHEN** an authorized user calls `GET /api/v1/reports/monthly-charts/balance-vs-groups?year=YYYY&month=MM`
- **THEN** the API MUST return `200 OK`
- **AND** the payload MUST include one total-balance series and one series per account group with aligned day buckets

### Requirement: Web UI SHALL Render Month-Level Charts
The reporting UI MUST render month-level charts in integrated report tabs using the dedicated datasets.

#### Scenario: Focused month selector refreshes monthly charts
- **WHEN** the user changes the selected month in an integrated state-evolution tab
- **THEN** monthly chart requests MUST reload for the selected month
- **AND** rendered chart points MUST match the response payload ordering and values
