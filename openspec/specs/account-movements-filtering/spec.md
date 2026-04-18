# account-movements-filtering Specification

## Purpose
TBD - created by archiving change transaction-amount-range-filter. Update Purpose after archive.
## Requirements
### Requirement: Account Movements Web/API Contract SHALL Accept Optional Amount Range Parameters
The account movements flow MUST support optional absolute-amount range parameters end-to-end without breaking existing consumers.

Contract changes:

- Web API abstraction signature MUST be extended:
  - File: `src/FamilyFinances.Web/Api/IAccountsApi.cs`
  - Method:
    - `Task<AccountMovementsDto> GetMovementsAsync(Guid accountId, DateOnly? fromInclusive = null, DateOnly? toExclusive = null, string? searchQuery = null, decimal? minAmount = null, decimal? maxAmount = null, int page = 1, int pageSize = 50, CancellationToken ct = default);`
- Web API implementation signature MUST match interface:
  - File: `src/FamilyFinances.Web/Api/AccountsApi.cs`
- HTTP query parameter mapping MUST be:
  - `minAmount=<decimal>`
  - `maxAmount=<decimal>`
- Decimal query serialization MUST use invariant culture formatting.
- Existing query parameters (`from`, `to`, `q`, `page`, `pageSize`) MUST remain backward compatible.

#### Scenario: Client sends minAmount and maxAmount when both are provided
- **WHEN** `AccountMovementsPage` requests movements with both amount bounds
- **THEN** the generated URL MUST contain `minAmount` and `maxAmount` query parameters
- **AND** existing query parameters MUST still be included when set

#### Scenario: Client omits amount parameters when bounds are empty
- **WHEN** no amount range values are provided
- **THEN** the generated request MUST omit `minAmount` and `maxAmount`
- **AND** endpoint behavior MUST remain equivalent to pre-change behavior

### Requirement: Account Movements Endpoint SHALL Expose Optional Amount Range Query Inputs
The existing endpoint `GET /api/v1/accounts/{id}/movements` MUST accept optional amount range query parameters and pass them to the repository layer.

Controller contract:

- File: `src/FamilyFinances.Api/Controllers/V1/AccountsController.cs`
- Action signature MUST include:
  - `[FromQuery] decimal? minAmount = null`
  - `[FromQuery] decimal? maxAmount = null`
- Repository invocation MUST pass `minAmount` and `maxAmount` to `GetAccountMovementsAsync(...)`.

Repository abstraction contract:

- File: `src/FamilyFinances.Application/Reporting/Abstractions/IReportingReadRepository.cs`
- Method signature MUST be:
  - `Task<AccountMovementsDto> GetAccountMovementsAsync(Guid accountId, DateOnly fromInclusive, DateOnly toExclusive, string? searchQuery = null, decimal? minAmount = null, decimal? maxAmount = null, int skip = 0, int take = 50, CancellationToken ct = default);`

#### Scenario: Endpoint accepts amount range inputs with pagination
- **WHEN** caller requests `/api/v1/accounts/{id}/movements?from=...&to=...&minAmount=10&maxAmount=50&page=2&pageSize=50`
- **THEN** endpoint MUST parse all query values successfully
- **AND** endpoint MUST call repository with parsed amount range and paging values

#### Scenario: Existing calls without amount range stay valid
- **WHEN** caller requests `/api/v1/accounts/{id}/movements` using only existing parameters
- **THEN** endpoint MUST return successful response using prior filtering behavior
- **AND** no new required query parameter MUST be introduced

### Requirement: Account Movements Repository SHALL Filter Using Absolute Signed Amount Cents
Account movements filtering MUST operate on `TransactionSplits.AmountCents` absolute value with inclusive bounds.

Data model constraints:

- No schema changes are allowed.
- Existing column used for predicate MUST be `TransactionSplits.AmountCents` (mapped from `TransactionSplit.Amount`).
- Absolute-value semantics MUST include both debit and credit signed rows for the same magnitude.

Implementation contract:

- File: `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs`
- Updated method signature MUST match repository interface.
- Query pipeline MUST apply amount predicates before `CountAsync`, `Skip`, and `Take`.
- Bound conversion MUST use deterministic cents precision:
  - `var minCents = Money.FromEuros(minAmount.Value).Cents;`
  - `var maxCents = Money.FromEuros(maxAmount.Value).Cents;`
- Predicate semantics MUST be inclusive:
  - `Math.Abs(SignedAmountCents) >= minCents`
  - `Math.Abs(SignedAmountCents) <= maxCents`

#### Scenario: Absolute filter includes opposite signed amounts with same magnitude
- **WHEN** account movements include `SignedAmount = -30.00` and `SignedAmount = +30.00`
- **AND** caller sets `minAmount=10.00&maxAmount=50.00`
- **THEN** both rows MUST satisfy amount filtering
- **AND** both rows MUST be eligible for returned page results

#### Scenario: Inclusive bounds include exact threshold values
- **WHEN** a movement absolute signed amount equals `minAmount` or `maxAmount`
- **THEN** that movement MUST be included in filtered results

#### Scenario: Min-only and max-only filters are independently supported
- **WHEN** only `minAmount` is supplied
- **THEN** repository MUST apply only lower-bound absolute predicate
- **AND** no upper-bound predicate MUST be applied
- **AND** when only `maxAmount` is supplied, the inverse rule MUST apply

### Requirement: Account Movements UI SHALL Validate Invalid Amount Ranges Before Request
The account movements page MUST validate user-entered amount ranges before calling the API.

UI validation contract:

- File: `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor`
- Backing state MUST include:
  - `private decimal? _amountFrom;`
  - `private decimal? _amountTo;`
- Apply action (`Search` button -> `ApplyFiltersAsync`) MUST reject ranges where `Amount From > Amount To`.
- Invalid ranges MUST surface localized error feedback and MUST NOT trigger `AccountsApi.GetMovementsAsync(...)`.

#### Scenario: Invalid amount range blocks API request
- **WHEN** user enters `Amount From = 100.00` and `Amount To = 50.00` and clicks `Search`
- **THEN** UI MUST show localized invalid-range error
- **AND** no account-movements API request MUST be sent

#### Scenario: Valid amount range sends API request with amount parameters
- **WHEN** user enters a valid amount range and clicks `Search`
- **THEN** UI MUST call `GetMovementsAsync(...)` with `minAmount`/`maxAmount`
- **AND** paging reset behavior (`_currentPage = 1`) MUST still apply

### Requirement: Running Balance and Pagination Semantics SHALL Remain Stable Under Amount Filtering
Adding amount range filtering MUST NOT change running-balance calculation ownership or page navigation semantics.

Behavioral invariants:

- `AccountMovementDto.RunningBalance` remains backend-provided source of truth.
- Page fallback behavior remains active:
  - if current page becomes empty while `TotalCount > 0` and page > 1, decrement and reload.
- Total count MUST represent the amount-filtered + date-filtered + text-filtered result set.

#### Scenario: Running balance values remain backend-sourced
- **WHEN** account movements are loaded with amount range filters
- **THEN** displayed running balance MUST come from API payload values
- **AND** frontend MUST NOT recompute running balances from visible rows

#### Scenario: Out-of-range page fallback still works with amount filters
- **WHEN** user is on a higher page and new amount filter reduces available pages
- **THEN** page fallback decrement-and-reload logic MUST execute
- **AND** user MUST land on the nearest valid page with rows or on page 1

### Requirement: Account Movements Search Query SHALL Ignore Diacritics and Case
The `q` search parameter in account movements MUST apply accent-insensitive and case-insensitive matching on movement text fields.

Implementation scope:

- Endpoint: `GET /api/v1/accounts/{id}/movements`
- API contract remains unchanged:
  - query parameter `q` stays optional
  - no new search-related query parameters are introduced
- Repository implementation path:
  - `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs`
  - method `GetAccountMovementsAsync(...)`
- Candidate fields:
  - `Transaction.Description`
  - `PayeeName`

#### Scenario: Plain query matches accented description
- **WHEN** caller requests `/api/v1/accounts/{id}/movements?...&q=jose`
- **THEN** rows with description containing `José` MUST match
- **AND** rows with description containing `Jose` MUST also match

#### Scenario: Plain query matches accented payee
- **WHEN** caller requests `/api/v1/accounts/{id}/movements?...&q=maria`
- **THEN** rows with payee `María` MUST match
- **AND** rows with payee `Maria` MUST match

#### Scenario: Search semantics remain backward compatible for empty query
- **WHEN** `q` is null, empty, or whitespace
- **THEN** no text-search predicate MUST be applied
- **AND** existing date/amount/pagination behavior MUST remain unchanged

### Requirement: Account Movements Search SHALL Preserve Pagination and Totals Semantics
Accent-insensitive matching MUST not break deterministic paging semantics.

#### Scenario: Total count reflects normalized query result set
- **WHEN** a normalized text query is applied
- **THEN** `TotalCount` MUST represent the full filtered set produced by normalized matching
- **AND** paged `Items` MUST be a slice of that same filtered set

#### Scenario: Running balance remains backend-sourced under normalized filtering
- **WHEN** account movements are returned for a normalized text query
- **THEN** `RunningBalance` values MUST remain sourced from backend movement calculation output
- **AND** frontend MUST NOT recompute running balances from displayed rows

