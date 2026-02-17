# System Specification (As-Built, OpenSpec)

## 1) Overview (what the system is)
FamilyFinances is a ledger-first personal finance system implemented as a layered modular monolith in .NET.  
It provides a versioned REST API, a Blazor web host that consumes that API, and persistence for finance and identity data through EF Core with SQLite.

## 2) Scope (what is implemented now)
Implemented now:
- API authentication via email/password login endpoint that returns a JWT access token.
- Role/policy-protected API access (`CanRead`, `CanWrite`).
- Ledger management for:
  - Accounts
  - Payees
  - Transactions (including multi-split update and expense search)
  - Account groups and memberships
- Reporting endpoints for:
  - Monthly summary
  - Category totals
  - Account totals
  - Account-group totals
- Health check endpoint at `/health`.
- Web-host auth session endpoints:
  - `POST /auth/session`
  - `GET /auth/session`
  - `DELETE /auth/session`
- EF Core migrations and startup initialization for ledger and identity contexts.

Known `UNKNOWN` contracts in current as-built scope:
- Exact `401/403` response payload contract.
  - Files that would confirm: `src/FamilyFinances.Api/Program.cs`, `src/FamilyFinances.Infrastructure/DependencyInjection.cs`, and integration tests asserting unauthorized/forbidden response bodies.
- Exact `/health` response body/status contract.
  - Files that would confirm: `src/FamilyFinances.Api/Program.cs` (`MapHealthChecks` options) and dedicated health integration tests.
- Full unhandled-exception response contract (outside explicitly mapped domain/conflict/not-found exceptions).
  - Files that would confirm: `src/FamilyFinances.Api/Program.cs` global exception pipeline and integration tests for unhandled exceptions.

## 3) Key Concepts (only proven)
- Ledger transaction model:
  - A `Transaction` has multiple `TransactionSplit` rows.
  - Splits must be balanced (sum of cents must equal `0`).
- Transaction classification is derived for list views (Expense, Income, Transfer, Refund, Other).
- Accounts:
  - Include `Nature`, `Kind`, lifecycle state (`IsClosed`, `ClosedOn`), and normalized name uniqueness for active accounts.
- Payees:
  - Include normalized-name uniqueness and optional `DefaultCategory` string.
- Account groups:
  - Group accounts through `AccountGroupMember` (many-to-many).
- Reconciliation:
  - Account reconciliation creates adjustment transactions instead of mutating historical rows.
- Money:
  - Stored in minor units (`cents`) and treated as single-currency (EUR).

## 4) Capabilities (bulleted)
- Authenticate users and return JWT access tokens (`POST /api/v{version}/auth/login`).
- Enforce read/write authorization policies across ledger and reporting endpoints.
- Create, list, rename, close, reopen, delete, and reconcile accounts.
- Create, list, rename, and delete payees.
- Create/list/get/update/delete transactions.
- Search expenses by query and optional expense account filter.
- Query whether any transactions exist.
- Create/list/get/rename/delete account groups.
- Add/remove accounts to/from groups.
- Retrieve reports for monthly summary, category totals, account totals, and group totals.
- Expose a health endpoint.
- Provide web session endpoints that proxy API auth and manage an HttpOnly token cookie.

## 5) Requirements

### Requirement: API Login Must Issue Access Tokens
#### Scenario: Valid credentials return access token
Given a user exists with valid credentials  
When the client calls `POST /api/v{version:apiVersion}/auth/login` with `LoginRequest`  
Then the API returns `200 OK`  
And the response contains `{ accessToken }`

#### Scenario: Invalid credentials are rejected
Given credentials that do not match a user  
When the client calls `POST /api/v{version:apiVersion}/auth/login`  
Then the API returns `401 Unauthorized`

### Requirement: Protected API Endpoints Must Enforce Authorization
#### Scenario: Read endpoint requires read authorization
Given a request to a read-protected endpoint (example: `GET /api/v{version:apiVersion}/accounts`) without valid authorization  
When the request is processed  
Then the response is `401` or `403`  
And the exact body contract is `UNKNOWN` (see Scope `UNKNOWN` list)

### Requirement: Accounts Must Support Full Lifecycle Operations
#### Scenario: Account creation returns account DTO
Given a valid `CreateAccountRequest`  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `200 OK`  
And the response is `AccountDto { Id, Name, Nature, Kind, OpenedOn, IsClosed, ClosedOn }`

#### Scenario: Rename of non-existing account returns not found
Given an account id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/rename`  
Then the API returns `404 NotFound`

### Requirement: Account Movements View Must Display Running Balance Evolution
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

### Requirement: Account Reconciliation Must Return Reconciliation Result
#### Scenario: Reconcile existing account
Given an existing account id and valid `ReconcileAccountRequest`  
When the client calls `POST /api/v{version:apiVersion}/accounts/{id}/reconcile`  
Then the API returns `200 OK`  
And the response is `ReconcileAccountResponse { AdjustmentCreated, TransactionId, ComputedBalance, ActualBalance, Difference, Message }`

### Requirement: Payees Must Support CRUD-Like Management
#### Scenario: Payee creation returns DTO
Given a valid `CreatePayeeRequest`  
When the client calls `POST /api/v{version:apiVersion}/payees`  
Then the API returns `200 OK`  
And the response is `PayeeDto { Id, Name }`

#### Scenario: Payee rename endpoint can return not found
Given a payee id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/payees/{id}/rename`  
Then the API returns `404 NotFound`

### Requirement: Transactions Must Enforce Balanced Splits and Support Query/Mutation Endpoints
#### Scenario: Unbalanced transaction creation is rejected
Given a `CreateTransactionRequest` where split cents do not sum to zero  
When the client calls `POST /api/v{version:apiVersion}/transactions`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` (domain exception mapping)

#### Scenario: Existing transaction can be deleted
Given a transaction id that exists  
When the client calls `DELETE /api/v{version:apiVersion}/transactions/{id}`  
Then the API returns `204 NoContent`

### Requirement: Account Groups Must Support Group CRUD and Membership Management
#### Scenario: Add account to group returns no content
Given a group id and account id  
When the client calls `POST /api/v{version:apiVersion}/account-groups/{id}/accounts/{accountId}`  
Then the API returns `204 NoContent`

#### Scenario: Get account group details returns group DTO
Given a group id that exists  
When the client calls `GET /api/v{version:apiVersion}/account-groups/{id}`  
Then the API returns `200 OK`  
And the response is `AccountGroupDetailsDto { Id, Name, Description, Accounts }`

### Requirement: Reporting Endpoints Must Provide Aggregated Read Models
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

### Requirement: Health Endpoint Must Be Exposed
#### Scenario: Health endpoint route exists
Given the API is running  
When the client calls `GET /health`  
Then the endpoint is reachable  
And exact response body/status contract is `UNKNOWN` (see Scope `UNKNOWN` list)

### Requirement: Web Host Must Provide Session Endpoints
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

### Requirement: Domain/Conflict/NotFound Exceptions Must Be Mapped
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

## 6) Non-goals / Out of scope
- Multi-currency support.
- Dedicated payee-category entity/model.
- Additional transaction link types beyond currently implemented values.
- Fixed, documented `401/403` response payload contract (currently `UNKNOWN`).
- Fixed, documented `/health` response contract (currently `UNKNOWN`).
- Fixed, documented global unhandled-exception response contract (currently `UNKNOWN`).
- Custom identity-domain fields beyond default ASP.NET Identity types (`UNKNOWN` from current as-built inventory).

For `UNKNOWN` identity-field confirmation:
- Files that would confirm:
  - `src/FamilyFinances.Infrastructure/Persistence/AppIdentityDbContext.cs`
  - `src/FamilyFinances.Infrastructure/Migrations/AppIdentityDbContextModelSnapshot.cs`
  - Any custom Identity user/role model files (if later introduced)

## 7) Known Limitations / TODOs (only proven)
- Single-currency ledger is hard-coded to EUR.
- Password policy is intentionally developer-friendly and marked to tighten later.
- HTTPS redirection is skipped outside Development in current runtime setup.
- Transaction link types are limited, with future expansion noted in code comments.
- `Payee.DefaultCategory` remains a string rather than a dedicated model.
- `Money.EnsureNotOverflowSafe()` contains a TODO and is not implemented.
