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
The accounts lifecycle MUST preserve existing create/read/update behavior while moving kind identity semantics from enum-based values to catalog-backed references.

#### Scenario: Account creation returns account DTO
Given a valid `CreateAccountRequest` with catalog-backed kind identity  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `200 OK`  
And the response includes account kind identity and display values derived from the kind catalog

#### Scenario: Rename of non-existing account returns not found
Given an account id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/rename`  
Then the API returns `404 NotFound`

#### Scenario: Account creation rejects unknown kind identity
Given a `CreateAccountRequest` referencing a non-existing or inactive catalog kind  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `400 BadRequest`  
And the error contract describes invalid account kind selection

#### Scenario: Account creation rejects kind identity incompatible with account nature
Given a `CreateAccountRequest` where selected catalog kind `Nature` does not match account `Nature`  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `400 BadRequest`  
And the error contract describes invalid account kind selection

#### Scenario: Existing account kind can be updated with compatibility checks
Given an existing account id and an active catalog kind compatible with that account `Nature`  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/kind`  
Then the API returns `204 NoContent`  
And subsequent account reads expose the updated catalog kind identity

### Requirement: Account Movements View SHALL Display Running Balance Evolution
The system MUST display the running account balance for each movement row in the account movements list so users can observe balance evolution across the selected period.

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

#### Scenario: Running balance remains correct across pages
Given account movements for the selected filters span multiple pages  
When any page of `/api/v1/accounts/{id}/movements` is requested  
Then each returned row's `RunningBalance` reflects the ledger balance immediately after that movement in chronological order for the full filtered range  
And running-balance correctness does not depend on how many rows are visible in the current page

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
The system SHALL provide aggregated report read models with release-ready reliability characteristics, including export compatibility, responsive web usability, and regression-safe behavior in reporting flows.

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

#### Scenario: Reporting flows remain export-compatible and regression-safe
Given final `0.9` reporting features are enabled  
When users execute supported report flows and export actions  
Then output values MUST remain consistent with report read models  
And release gates MUST block shipment on critical regression failures

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

### Requirement: System SHALL Provide First-Class Backup and Restore Operations
The system SHALL provide operational data protection and recovery capabilities through authenticated backup export and restore workflows.

#### Scenario: Backup and restore are exposed for authorized administrators
- **WHEN** an authenticated admin accesses application settings and backup endpoints
- **THEN** the system MUST expose backup export and restore actions
- **AND** those operations MUST be available through versioned API routes and Web UI entry points

#### Scenario: Backup and restore are unavailable to unauthorized actors
- **WHEN** an unauthenticated user or non-admin user attempts backup/restore operations
- **THEN** the system MUST block access using existing authorization policy enforcement

### Requirement: System SHALL Preserve Runtime Consistency During Restore
Restore workflows SHALL enforce validation-before-apply and consistency guarantees so failed restore attempts do not corrupt active runtime data.

#### Scenario: Incompatible package never reaches apply
- **WHEN** restore pre-check reports incompatible format, version, or structure
- **THEN** the system MUST reject apply execution
- **AND** current runtime data MUST remain unchanged

#### Scenario: Restore failures are deterministic and non-destructive
- **WHEN** restore apply encounters operational failure
- **THEN** the system MUST return a deterministic failure result
- **AND** active runtime data state MUST be preserved

### Requirement: Web Host SHALL Enforce A Dark-First Premium Presentation Baseline
The Web host MUST enforce dark mode as the default presentation baseline and MUST initialize premium visual primitives for Dashboard and Reports without requiring user interaction.

#### Scenario: Default startup keeps dark-first premium baseline
- **WHEN** a user opens the Web app with no persisted theme preference
- **THEN** the app MUST start in dark mode
- **AND** premium shell and surface primitives MUST be applied to the rendered UI baseline

### Requirement: Language Selection SHALL Remain Scoped To Settings
The system MUST keep language selection controls in the Settings page and MUST NOT expose language selection in top-level navigation chrome.

#### Scenario: Navigation does not expose language selector
- **WHEN** an authenticated user opens the main navigation menu
- **THEN** no language selector control MUST be present in the menu chrome
- **AND** navigation entries for Dashboard and Reports MUST remain focused on task navigation

#### Scenario: Settings keeps language switching capability
- **WHEN** the user opens `/settings`
- **THEN** language controls MUST remain available there
- **AND** changing language MUST preserve current live-switch behavior

### Requirement: Premium Styling SHALL Be Implemented Through Shared Primitives
Cross-page premium styling MUST be implemented through shared primitives, MUST cover all Web UI surfaces, and MUST avoid one-off page-level visual forks or newly hardcoded presentation literals.

#### Scenario: Shared primitives are applied before page-specific overrides
- **WHEN** premium styling is implemented or refactored in the Web UI
- **THEN** shared tokenized primitives MUST be the primary mechanism for cards, tables, tabs, panel framing, control sizing, and chart surfaces
- **AND** page-level custom overrides MUST be limited to documented exceptions only

#### Scenario: Regression guardrails prevent style hardcode drift
- **WHEN** frontend changes are validated in automated test gates
- **THEN** newly introduced disallowed hardcoded visual literals in protected paths MUST fail validation
- **AND** shared primitive consumption requirements MUST remain enforceable through deterministic checks

### Requirement: System Navigation IA SHALL Separate Analysis And Capture Surfaces
The system MUST separate dashboard analysis and quick-entry capture surfaces while keeping report navigation explicit in main menu.

#### Scenario: Navigation exposes dedicated capture and analysis routes
- **WHEN** authenticated navigation chrome is rendered
- **THEN** users MUST be able to access `/` for dashboard analysis and `/quick-entry` for capture workflows
- **AND** `Reports` menu access MUST remain available for deep-dive report pages

#### Scenario: Dashboard does not become a report-link launcher
- **WHEN** users open `/`
- **THEN** dashboard content MUST prioritize financial status analysis blocks
- **AND** report entry card grids MUST not be required to access report details

### Requirement: System SHALL Preserve Language-Control Scope In Settings During IA Changes
Language-selection scope MUST remain constrained to Settings while navigation IA changes are introduced.

#### Scenario: Navigation IA updates do not surface language selector
- **WHEN** dashboard and quick-entry navigation updates are deployed
- **THEN** main navigation MUST not include a language selector control
- **AND** Settings MUST remain the canonical language-switching surface

### Requirement: Developer Workflow SHALL Support Optional External Skill Orchestration Without Runtime Coupling
The developer workflow system MUST support optional external-skill orchestration in a way that does not modify runtime application behavior.

#### Scenario: Workflow integration remains tooling-only
- **WHEN** OpenSpec+gstack orchestration is enabled
- **THEN** orchestration effects MUST be limited to planning/implementation/verification workflow artifacts and outputs
- **AND** runtime API/database/UI contracts MUST remain unchanged unless an explicit product change requires otherwise

#### Scenario: Tooling fallback is deterministic
- **WHEN** external orchestration cannot run
- **THEN** workflow execution MUST remain deterministic in OpenSpec-only mode
- **AND** users MUST receive clear fallback messaging without blocking normal OpenSpec flows

### Requirement: Account Movements View SHALL Support Navigating Paginated Results
The system MUST let users navigate all filtered account movements, not only the first page.

#### Scenario: User navigates beyond first 50 movements
Given an authenticated user opens `/accounts/{id}/movements` with filters producing more than 50 rows  
When the movements view is rendered  
Then the UI shows pagination controls that allow moving to next and previous pages  
And selecting next page loads additional movements from the same filtered result set

#### Scenario: Filters reset pagination to first page
Given the user is browsing page `N` (`N > 1`) of account movements  
When the user changes date range presets, manual dates, or search criteria and applies filters  
Then the movements request uses page `1` for the new filter set  
And the rendered table reflects the first page of that updated filter set

#### Scenario: View communicates visible range and total
Given account movements are rendered for a paginated result set  
When header/footer counters are shown  
Then the UI displays the visible movement range and total filtered count  
And the user can determine that more pages exist when the visible range does not cover the total count

### Requirement: System SHALL Present Monetary Values With EUR Identity Across Web UI Surfaces
The system MUST present user-facing monetary values with EUR currency identity in all Web UI surfaces that render ledger amounts, including list rows, table cells, and summary amount labels.

Scope constraints:
- This requirement applies to presentation formatting only.
- Domain/accounting value semantics MUST remain unchanged.
- Storage and API numeric payload contracts MUST remain unchanged unless a preformatted display string is the only source of rendered symbol.

#### Scenario: Transactions and account movements surfaces render EUR symbol
- **WHEN** an authenticated user views transactions and account movements list rows
- **THEN** rendered monetary values MUST use the "€" symbol
- **AND** no "$" symbol MUST be rendered for monetary values on those surfaces

#### Scenario: Reporting list/table surfaces render EUR symbol
- **WHEN** a user opens reporting views that show tabular/list-like monetary values
- **THEN** rendered monetary values MUST use EUR symbol semantics
- **AND** symbol rendering MUST remain consistent with transactions/account movements surfaces

#### Scenario: Sign semantics remain unchanged while symbol is standardized
- **WHEN** positive and negative amounts are rendered after this change
- **THEN** sign-preserving semantics MUST remain unchanged
- **AND** only currency identity presentation is standardized to EUR

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
