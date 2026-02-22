## MODIFIED Requirements

### Requirement: Reporting Endpoints SHALL Provide Aggregated Read Models
The system SHALL provide aggregated report read models, including `state-evolution` yearly contracts and month-level chart datasets required for intra-month balance evolution and balance-vs-group comparisons.

#### Scenario: Monthly summary returns summary DTO
Given valid date query inputs  
When the client calls `GET /api/v1/reports/monthly-summary`  
Then the API returns `200 OK`  
And the response is `MonthlySummaryDto { From, To, IncomeTotal, ExpenseTotal, Net, TransactionsCount }`

#### Scenario: Account-group totals may return not found
Given a group id that does not exist  
When the client calls `GET /api/v1/reports/account-groups/{groupId}/totals`  
Then the API returns `404 NotFound`  
And mapped not-found errors use `{ error }`

#### Scenario: Asset total balance report returns as-of aggregated asset balance
Given a valid as-of date and authorized user  
When the client calls `GET /api/v1/reports/asset-total-balance?asOf=YYYY-MM-DD`  
Then the API returns `200 OK`  
And the response contains `AsOf`, `TotalCents`, and `AssetAccountsCount`

#### Scenario: State evolution report returns scoped monthly series
Given a valid `year` and `scope` and an authorized user  
When the client calls `GET /api/v1/reports/state-evolution?year=YYYY&scope=<scope>`  
Then the API returns `200 OK`  
And the response contains ordered monthly points with `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

#### Scenario: State evolution report rejects invalid query parameters
Given a request with missing or invalid `year` or `scope`  
When the client calls `GET /api/v1/reports/state-evolution`  
Then the API returns `400 BadRequest`

#### Scenario: Legacy monthly-evolution alias remains available
Given a valid `year` and `scope` and an authorized user  
When the client calls `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<scope>`  
Then the API returns `200 OK`  
And the payload shape matches the primary state-evolution contract

#### Scenario: Month-level chart endpoints return intra-month series
Given valid month-level chart query parameters and authorized user  
When the client calls `GET /api/v1/reports/monthly-charts/balance` or `GET /api/v1/reports/monthly-charts/group-evolution`  
Then the API returns ordered day-bucket series for selected month  
And day buckets MUST be aligned across compared series in the same response
And the backward-compatible alias `GET /api/v1/reports/monthly-charts/balance-vs-groups` remains available
