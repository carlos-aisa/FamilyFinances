# historical-ledger-views Delta Specification

## ADDED Requirements

### Requirement: Historical Movements Search SHALL Ignore Diacritics and Case
Historical account movements search MUST use accent-insensitive and case-insensitive text matching while keeping year/account scoping intact.

Implementation scope:

- Endpoint: `GET /api/v1/history/movements`
- Query parameter: optional `q`
- Web caller: `src/FamilyFinances.Web/Components/Pages/History/HistoryMovementsPage.razor`
- API client: `src/FamilyFinances.Web/Api/HistoryApi.cs`
- Backend flow:
  - `src/FamilyFinances.Api/Controllers/V1/HistoryController.cs`
  - `src/FamilyFinances.Application/Ledger/FiscalYears/Handlers/GetHistoricalAccountMovementsHandler.cs`
  - `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs`

#### Scenario: Historical query matches accented values
- **WHEN** user requests historical movements with `q=jose`, `accountId=A`, and `year=Y`
- **THEN** rows for account `A` in year `Y` with `José` in description or payee MUST match
- **AND** account/year scoping MUST still be enforced

#### Scenario: Historical query remains optional
- **WHEN** `q` is omitted or whitespace in historical movements requests
- **THEN** historical movement retrieval MUST behave as before without text filtering
- **AND** read-only behavior of historical view MUST remain unchanged

### Requirement: Historical Search Semantics SHALL Match Operational Account Movements Semantics
Historical movement search behavior MUST be consistent with operational account-movements search semantics for diacritic handling.

#### Scenario: Same normalized query yields consistent inclusion behavior
- **WHEN** equivalent movement data exists in operational and historical views for comparable periods
- **THEN** normalized query matching rules for description/payee text MUST be equivalent in both views
- **AND** neither view MUST require users to type exact accent marks to find matching rows

