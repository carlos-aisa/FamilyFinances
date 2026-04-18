## 1. Account movements pagination UX

- [x] 1.1 Update `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` to track `_currentPage` and `_pageSize`, and pass them to `AccountsApi.GetMovementsAsync(...)` instead of hardcoded `page=1`.
- [x] 1.2 Add deterministic pagination controls (previous/next) in the account movements footer/header and disable controls at boundaries.
- [x] 1.3 Show visible range metadata (`X-Y of Z`) using API `TotalCount` and current page state.
- [x] 1.4 Reset `_currentPage` to `1` whenever filters are reapplied (date preset, manual date change + search action).
- [x] 1.5 Handle out-of-range page results: if a requested page is empty while `TotalCount > 0` and `_currentPage > 1`, decrement page and reload.

## 2. Running balance correctness safeguards

- [x] 2.1 Extend `tests/FamilyFinances.Api.IntegrationTests/Ledger/Accounts/AccountMovementsApiTests.cs` DTO/test helpers to assert `RunningBalance` values explicitly.
- [x] 2.2 Add an integration test with more than 50 movements in one filtered range and assert running balances are correct on page 1 and page 2.
- [x] 2.3 Add an integration test that combines pagination with filtering (`from/to` and/or `q`) and verifies running-balance correctness is independent from page size limits.

## 3. Localization and Web UI tests

- [x] 3.1 Add pagination-related localization keys to:
  - `src/FamilyFinances.Web/Resources/SharedResource.resx`
  - `src/FamilyFinances.Web/Resources/SharedResource.en-US.resx`
  - `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx`
- [x] 3.2 Add bUnit coverage in `tests/FamilyFinances.Web.Tests/Features/Accounts/AccountMovementsPageTests.cs` for pagination controls visibility, next/previous transitions, and range text updates.
- [x] 3.3 Add/adjust navigation-context tests under `tests/FamilyFinances.Web.Tests/Features/Transactions/` if needed to ensure movement-detail navigation still works after pagination state changes.

## 4. Validation and documentation

- [x] 4.1 Run `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~AccountMovements"`.
- [x] 4.2 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release --filter "FullyQualifiedName~AccountMovementsPage"`.
- [x] 4.3 Run `dotnet test FamilyFinances.sln -c Release` (or equivalent affected-project matrix) to confirm no regressions.
- [x] 4.4 Validate OpenSpec artifacts with `openspec validate account-movements-running-balance-pagination --strict`.
