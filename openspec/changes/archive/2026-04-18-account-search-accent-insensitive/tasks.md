## 1. Shared Search Normalization Primitive

- [x] 1.1 Create `d:/Programacion/FamilyFinances/src/FamilyFinances.Application/Common/SearchTextNormalizer.cs` with `public static string NormalizeForSearch(string? value)` using Unicode decomposition (`NormalizationForm.FormD`), combining-mark removal (`CharUnicodeInfo.GetUnicodeCategory`), trim, and lowercase normalization; done when method returns stable normalized text for null/empty/accented inputs.
- [x] 1.2 Add unit tests at `d:/Programacion/FamilyFinances/tests/FamilyFinances.Application.Tests/Common/SearchTextNormalizerTests.cs` covering `maria <-> maría`, `jose <-> josé`, uppercase/lowercase symmetry, and null/whitespace behavior; done when all new tests pass deterministically.
- [x] 1.3 Ensure normalization helper has no layer violation (Application-only implementation, no Web/Infrastructure dependency leakage); done when `dotnet build d:/Programacion/FamilyFinances/FamilyFinances.sln -c Release` succeeds with no architecture-breaking reference changes.

## 2. Quick Entry and Shared Account Selector Search Behavior

- [x] 2.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor` so `GetFilteredAccounts(...)` normalizes `_globalAccountSearchQuery`, account names, nature labels, and kind labels with `SearchTextNormalizer.NormalizeForSearch(...)`; done when plain query matches accented account names and vice versa.
- [x] 2.2 Preserve existing Quick Entry behavior (section grouping, accordion open logic, empty-query behavior) after introducing normalization in `QuickEntryPage.razor`; done when no non-search UX regressions are observed in existing Quick Entry tests.
- [x] 2.3 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Shared/AccountSelector.razor` search predicate to use `SearchTextNormalizer.NormalizeForSearch(...)` for query and candidate fields (`a.Name`, `a.Nature.ToString()`); done when selector remains accent-insensitive while `FilterByNature` and `AllowedNatures` behavior is unchanged.

## 3. Transactions List Accent-Insensitive Text Filtering

- [x] 3.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor` text-filter block in `FilterTransactions(...)` to use normalized comparisons for `Headline`, `Subheadline`, and `PayeeName`; done when search results match irrespective of accents and case.
- [x] 3.2 Keep existing filter composition order in `TransactionsListPage.razor` (date -> text -> amount range) and do not modify amount-range validation logic introduced in `transaction-amount-range-filter`; done when prior amount-range tests keep passing without behavior drift.
- [x] 3.3 Ensure normalized text filtering does not alter reset/load-more semantics in `TransactionsListPage.razor`; done when `Reset` and `Load More` continue to operate on the expected filtered dataset.

## 4. Backend Accent-Insensitive Search for Movements and Expense Lookup

- [x] 4.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` account-movements search branch (`searchQuery`) to apply accent-insensitive matching for description/payee using `SearchTextNormalizer.NormalizeForSearch(...)`; done when query `q` matches accented and non-accented variants.
- [x] 4.2 Preserve deterministic account-movements behavior in `ReportingReadRepository.GetAccountMovementsAsync(...)` under normalized filtering: total count calculation, sorting, pagination slicing, and running-balance output semantics remain unchanged; done when existing pagination/running-balance tests still pass.
- [x] 4.3 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Infrastructure/Persistence/Repositories/TransactionRepository.cs` in `SearchExpensesAsync(...)` to perform accent-insensitive matching for description, payee name, and expense account name; done when refund expense search no longer requires exact accent typing.
- [x] 4.4 Keep API contract unchanged for search routes (`q` remains optional parameter only) across:
  - `d:/Programacion/FamilyFinances/src/FamilyFinances.Api/Controllers/V1/AccountsController.cs`
  - `d:/Programacion/FamilyFinances/src/FamilyFinances.Api/Controllers/V1/HistoryController.cs`
  - `d:/Programacion/FamilyFinances/src/FamilyFinances.Api/Controllers/V1/TransactionsController.cs`
  done when no new query parameters or endpoint shape changes are introduced.

## 5. Web Test Coverage Updates

- [x] 5.1 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/QuickEntry/QuickEntryPageTests.cs` with at least one scenario proving unaccented query matches accented account names in Quick Entry global search.
- [x] 5.2 Add/extend tests for `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Components/Shared/AccountSelector` (create file if missing) to validate accent-insensitive matching while preserving existing nature filters.
- [x] 5.3 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/Transactions/TransactionsListPageTests.cs` with accent-insensitive scenarios for headline/subheadline/payee matching.
- [x] 5.4 Ensure existing amount-range tests in `TransactionsListPageTests.cs` remain green after text-filter normalization changes.
- [x] 5.5 Keep API client tests (`AccountsApiAdditionalTests.cs`, `HistoryApiAdditionalTests.cs`, `TransactionsApiAdditionalTests.cs`) unchanged unless required by refactors; done when no contract regressions are introduced.

## 6. API Integration and Application Test Coverage

- [x] 6.1 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/Ledger/Accounts/AccountMovementsApiTests.cs` with accent-insensitive `q` scenarios for description and payee matching.
- [x] 6.2 Add a historical movements integration scenario in `d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/Ledger/FiscalYears/FiscalYearGovernanceApiTests.cs` (or a dedicated history integration test file) proving `GET /api/v1/history/movements` supports accent-insensitive search semantics.
- [x] 6.3 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/Ledger/Transactions/RefundsApiTests.cs` to validate accent-insensitive behavior for `GET /api/v1/transactions/search-expenses`.
- [x] 6.4 Ensure all added integration tests use relational provider-backed test setup only (no EF Core InMemory); done when they run in current CI-safe test harness.

## 7. Validation, Regression Checks, and Artifact Readiness

- [x] 7.1 Run focused Application tests: `dotnet test d:/Programacion/FamilyFinances/tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release --filter "FullyQualifiedName~SearchTextNormalizerTests"`.
- [x] 7.2 Run focused Web tests: `dotnet test d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release --filter "FullyQualifiedName~QuickEntryPageTests|FullyQualifiedName~TransactionsListPageTests|FullyQualifiedName~AccountSelector"`.
- [x] 7.3 Run focused API integration tests: `dotnet test d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~AccountMovementsApiTests|FullyQualifiedName~FiscalYearGovernanceApiTests|FullyQualifiedName~RefundsApiTests"`.
- [x] 7.4 Run full regression suite: `dotnet test d:/Programacion/FamilyFinances/FamilyFinances.sln -c Release`.
- [x] 7.5 Verify no schema/migration artifacts were created under `d:/Programacion/FamilyFinances/src/FamilyFinances.Infrastructure/Persistence/Migrations/**`; done when git status shows no new migration files for this change.
- [x] 7.6 Validate OpenSpec artifacts: `openspec validate account-search-accent-insensitive --strict`; done when validation passes with no schema/style errors.

