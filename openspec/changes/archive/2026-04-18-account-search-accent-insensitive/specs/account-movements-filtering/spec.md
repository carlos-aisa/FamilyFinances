# account-movements-filtering Delta Specification

## ADDED Requirements

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

