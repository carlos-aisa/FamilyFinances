## ADDED Requirements

### Requirement: Reports Index SHALL Expose Monthly Evolution Navigation
The Web Reports index MUST expose navigation to the dedicated monthly evolution report page.

#### Scenario: Reports index includes monthly evolution entry
- **WHEN** an authenticated user opens `/reports`
- **THEN** the page MUST display a `Monthly Evolution` entry
- **AND** selecting that entry MUST navigate to `/reports/monthly-evolution`

## MODIFIED Requirements

### Requirement: Reporting Endpoints SHALL Provide Aggregated Read Models
The system SHALL provide aggregated report read models, including monthly summary, account-group totals, and monthly evolution contracts.

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

#### Scenario: Monthly evolution report returns scoped monthly series
Given a valid `year` and `scope` and an authorized user  
When the client calls `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<scope>`  
Then the API returns `200 OK`  
And the response contains ordered monthly points with `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

#### Scenario: Monthly evolution report rejects invalid query parameters
Given a request with missing or invalid `year` or `scope`  
When the client calls `GET /api/v1/reports/monthly-evolution`  
Then the API returns `400 BadRequest`
