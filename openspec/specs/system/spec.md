# System Specification

## Purpose
Define the as-built baseline behavior of FamilyFinances, a ledger-first personal finance system implemented as a layered .NET modular monolith with a versioned REST API, Blazor web UI, and EF Core persistence.

Implemented capabilities include:
- Authentication with JWT token issuance and protected API policies (`CanRead`, `CanWrite`).
- Ledger operations for accounts, payees, transactions, and account groups.
- Reporting endpoints for monthly summary, category totals, account totals, and account-group totals.
- Web host session endpoints (`POST /auth/session`, `GET /auth/session`, `DELETE /auth/session`).
- Health endpoint exposure and EF Core migration-based initialization.

Known unresolved contracts:
- Exact `401/403` payload body shape for unauthorized/forbidden responses.
- Exact `/health` response body/status contract.
- Exact global unhandled-exception payload contract outside explicit domain/conflict/not-found mappings.

## Requirements

### Requirement: API Login SHALL Issue Access Tokens
#### Scenario: Valid credentials return access token
Given a user exists with valid credentials  
When the client calls `POST /api/v{version:apiVersion}/auth/login` with `LoginRequest`  
Then the API returns `200 OK`  
And the response contains `{ accessToken }`

#### Scenario: Invalid credentials are rejected
Given credentials that do not match a user  
When the client calls `POST /api/v{version:apiVersion}/auth/login`  
Then the API returns `401 Unauthorized`

### Requirement: Protected API Endpoints SHALL Enforce Authorization
#### Scenario: Read endpoint requires read authorization
Given a request to a read-protected endpoint (example: `GET /api/v{version:apiVersion}/accounts`) without valid authorization  
When the request is processed  
Then the response is `401` or `403`  
And the exact body contract is `UNKNOWN` (see Purpose known unresolved contracts)

### Requirement: Accounts SHALL Support Full Lifecycle Operations
#### Scenario: Account creation returns account DTO
Given a valid `CreateAccountRequest`  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `200 OK`  
And the response is `AccountDto { Id, Name, Nature, Kind, OpenedOn, IsClosed, ClosedOn }`

#### Scenario: Rename of non-existing account returns not found
Given an account id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/rename`  
Then the API returns `404 NotFound`

### Requirement: Account Movements View SHALL Display Running Balance Evolution
#### Scenario: Running balance shown per movement row
Given an authenticated user opens `/accounts/{id}/movements` and movements are returned  
When the movements list is rendered  
Then each movement row displays its `RunningBalance` value in a dedicated running-balance column

#### Scenario: Running balance uses backend-provided value
Given movement data is rendered in the account movements table  
When the running-balance value is displayed  
Then the value comes from `AccountMovementDto.RunningBalance` without frontend recomputation

#### Scenario: Running balance formatting remains monetary
Given a running-balance value is displayed  
When the value is rendered  
Then it is formatted as currency with sign-preserving semantics consistent with movement amount formatting

#### Scenario: Historical movement browsing is read-only
Given an authenticated user opens a historical movements route for a selected year/account  
When movement rows are shown  
Then running balance is displayed for each row  
And mutation actions (create/edit/delete) are not available from that historical view

### Requirement: Account Reconciliation SHALL Return Reconciliation Result
#### Scenario: Reconcile existing account
Given an existing account id and valid `ReconcileAccountRequest`  
When the client calls `POST /api/v{version:apiVersion}/accounts/{id}/reconcile`  
Then the API returns `200 OK`  
And the response is `ReconcileAccountResponse { AdjustmentCreated, TransactionId, ComputedBalance, ActualBalance, Difference, Message }`

#### Scenario: Reconcile request in closed fiscal year is rejected
Given a valid account id and `ReconcileAccountRequest` where `AsOfDate.Year` is closed  
When the client calls `POST /api/v{version:apiVersion}/accounts/{id}/reconcile`  
Then the API returns `400 BadRequest`  
And the response body uses `{ error }` describing the closed-year restriction

### Requirement: Payees SHALL Support CRUD-Like Management
#### Scenario: Payee creation returns DTO
Given a valid `CreatePayeeRequest`  
When the client calls `POST /api/v{version:apiVersion}/payees`  
Then the API returns `200 OK`  
And the response is `PayeeDto { Id, Name }`

#### Scenario: Payee rename endpoint can return not found
Given a payee id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/payees/{id}/rename`  
Then the API returns `404 NotFound`

### Requirement: Transactions SHALL Enforce Balanced Splits and Support Query/Mutation Endpoints
#### Scenario: Unbalanced transaction creation is rejected
Given a `CreateTransactionRequest` where split cents do not sum to zero  
When the client calls `POST /api/v{version:apiVersion}/transactions`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` (domain exception mapping)

#### Scenario: Existing transaction can be deleted
Given a transaction id that exists  
When the client calls `DELETE /api/v{version:apiVersion}/transactions/{id}`  
Then the API returns `204 NoContent`

#### Scenario: Closed-year transaction creation is rejected
Given a `CreateTransactionRequest` where `BookedOn.Year` is closed  
When the client calls `POST /api/v{version:apiVersion}/transactions`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction

#### Scenario: Closed-year transaction update is rejected
Given an update request targeting a transaction booked in closed year `Y`  
When the client calls `PUT /api/v{version:apiVersion}/transactions/{id}` or `PUT /api/v{version:apiVersion}/transactions/{id}/multi-split`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction

#### Scenario: Closed-year transaction deletion is rejected
Given a transaction id whose stored `BookedOn.Year` is closed  
When the client calls `DELETE /api/v{version:apiVersion}/transactions/{id}`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction

### Requirement: Account Groups SHALL Support Group CRUD and Membership Management
#### Scenario: Add account to group returns no content
Given a group id and account id  
When the client calls `POST /api/v{version:apiVersion}/account-groups/{id}/accounts/{accountId}`  
Then the API returns `204 NoContent`

#### Scenario: Get account group details returns group DTO
Given a group id that exists  
When the client calls `GET /api/v{version:apiVersion}/account-groups/{id}`  
Then the API returns `200 OK`  
And the response is `AccountGroupDetailsDto { Id, Name, Description, Accounts }`

### Requirement: Reporting Endpoints SHALL Provide Aggregated Read Models
The system SHALL provide aggregated report read models, including monthly summary, account-group totals, as-of asset total balance, and monthly evolution contracts, with explicit semantic distinction between stock and flow metrics.

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

#### Scenario: Monthly evolution report returns scoped monthly series
Given a valid `year` and `scope` and an authorized user  
When the client calls `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<scope>`  
Then the API returns `200 OK`  
And the response contains ordered monthly points with `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

#### Scenario: Monthly evolution report rejects invalid query parameters
Given a request with missing or invalid `year` or `scope`  
When the client calls `GET /api/v1/reports/monthly-evolution`  
Then the API returns `400 BadRequest`

#### Scenario: Stock and flow semantics are explicitly distinguished in reporting contract usage
Given a reporting view combines values from monthly summary and monthly evolution  
When the system renders or documents those values together  
Then period flow metrics (`IncomeTotal`, `ExpenseTotal`, `Net`) MUST be treated as period-result semantics  
And balance/delta metrics (`EndBalanceCents`, evolution deltas, as-of asset totals) MUST be treated as stock semantics  
And non-equivalent metrics MUST NOT be labeled as equivalent indicators

### Requirement: Health Endpoint SHALL Be Exposed
#### Scenario: Health endpoint route exists
Given the API is running  
When the client calls `GET /health`  
Then the endpoint is reachable  
And exact response body/status contract is `UNKNOWN` (see Purpose known unresolved contracts)

### Requirement: Web Host SHALL Provide Session Endpoints
#### Scenario: Session login stores token and returns payload
Given valid credentials  
When the client calls `POST /auth/session`  
Then the endpoint returns `200 OK`  
And returns `{ accessToken }`  
And sets auth cookie `ff_access_token`

#### Scenario: Session query without cookie returns no content
Given no `ff_access_token` cookie  
When the client calls `GET /auth/session`  
Then the endpoint returns `204 NoContent`

### Requirement: Domain/Conflict/NotFound Exceptions SHALL Be Mapped
#### Scenario: Domain exception maps to bad request
Given a request that triggers a domain rule violation  
When the exception reaches API middleware  
Then the API returns `400 BadRequest`  
And response body is `{ error }`

#### Scenario: Conflict exception maps to conflict
Given a request that triggers a conflict exception  
When the exception reaches API middleware  
Then the API returns `409 Conflict`  
And response body is `{ error }`

## Non-Goals
- Multi-currency support.
- Dedicated payee-category entity/model.
- Additional transaction link types beyond currently implemented values.
- Fixed/documented payload contracts for `401/403`, `/health`, and global unhandled exceptions.
- Custom identity-domain fields beyond default ASP.NET Identity types.

## Known Limitations
- Single-currency ledger is hard-coded to EUR.
- Password policy is intentionally developer-friendly and marked to tighten later.
- HTTPS redirection is skipped outside Development in current runtime setup.
- Transaction link types are limited, with future expansion noted in code comments.
- `Payee.DefaultCategory` remains a string rather than a dedicated model.
- `Money.EnsureNotOverflowSafe()` contains a TODO and is not implemented.
