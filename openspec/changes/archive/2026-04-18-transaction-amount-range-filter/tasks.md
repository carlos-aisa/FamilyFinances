## 1. Backend and API contract propagation

- [x] 1.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Application/Reporting/Abstractions/IReportingReadRepository.cs` so `GetAccountMovementsAsync(...)` includes optional `decimal? minAmount = null` and `decimal? maxAmount = null` before `skip/take`; done when all compile-time callers use the same parameter order.
- [x] 1.2 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` method signature and apply absolute cents predicates before `CountAsync`: `Math.Abs(SignedAmountCents) >= minCents` and `Math.Abs(SignedAmountCents) <= maxCents`; done when pagination and ordering code paths remain unchanged.
- [x] 1.3 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Api/Controllers/V1/AccountsController.cs` to accept `[FromQuery] decimal? minAmount = null` and `[FromQuery] decimal? maxAmount = null`, and pass them to repository; done when endpoint keeps existing `from/to/q/page/pageSize` behavior.
- [x] 1.4 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Application/Ledger/FiscalYears/Handlers/GetHistoricalAccountMovementsHandler.cs` repository call to new signature while preserving current behavior (`minAmount/maxAmount` omitted); done when handler compiles without semantic regressions.
- [x] 1.5 Verify no persistence schema artifacts are created (`d:/Programacion/FamilyFinances/src/FamilyFinances.Infrastructure/Persistence/Migrations/**` unchanged); done when `git status` shows no new migration files.

## 2. Web API client contract and query serialization

- [x] 2.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Api/IAccountsApi.cs` `GetMovementsAsync(...)` to include optional `decimal? minAmount = null` and `decimal? maxAmount = null`; done when all consumers compile against the updated interface.
- [x] 2.2 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Api/AccountsApi.cs` method signature and query builder to append `minAmount`/`maxAmount` only when provided; done when URL generation remains backward compatible for requests without amount filters.
- [x] 2.3 Serialize decimal query params with invariant culture in `AccountsApi` using pattern `value.ToString(CultureInfo.InvariantCulture)`; done when tests assert query contains dot-decimal formatting independent of current UI culture.

## 3. Transactions page client-side amount range filtering

- [x] 3.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor` filter UI to include `Amount From` and `Amount To` numeric fields (`step="0.01"`, optional values) in the existing filters card; done when page renders both controls without removing current date/search filters.
- [x] 3.2 Add backing state in `TransactionsListPage.razor` (`private decimal? _amountFrom; private decimal? _amountTo;`) and clear them in `ResetFilters()`; done when reset restores both inputs to empty.
- [x] 3.3 Extend `FilterTransactions(IReadOnlyList<TransactionListItemDto> items)` in `TransactionsListPage.razor` with inclusive amount predicates (`t.Amount >= _amountFrom`, `t.Amount <= _amountTo`) while keeping date/search filters; done when all predicates compose correctly.
- [x] 3.4 Add deterministic invalid-range guard in `ApplyFilters()` for `Amount From > Amount To` and surface localized error key `Filter_AmountRangeInvalid`; done when invalid range does not mutate current result set.
- [x] 3.5 Ensure `LoadMoreAsync()` keeps using filtered dataset whenever amount filters are active; done when appended rows still obey amount bounds.

## 4. Account movements page amount range filtering

- [x] 4.1 Update `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` filter card to include `Amount From` and `Amount To` numeric controls without breaking existing layout/search button.
- [x] 4.2 Add backing state in `AccountMovementsPage.razor` (`private decimal? _amountFrom; private decimal? _amountTo;`) and apply deterministic validation before API call (`_amountFrom > _amountTo` is invalid).
- [x] 4.3 Update the `AccountsApi.GetMovementsAsync(...)` invocation in `LoadMovementsAsync()` to pass `minAmount`/`maxAmount` derived from `_amountFrom/_amountTo`; done when requests include query params only for provided values.
- [x] 4.4 Preserve existing pagination semantics (`_currentPage = 1` on filter apply and out-of-range fallback decrement/reload) after introducing amount filters; done when same logic path works with and without amount values.
- [x] 4.5 Keep running-balance rendering unchanged (`AccountMovementDto.RunningBalance` payload value only) while amount filtering is active; done when no frontend recomputation is introduced.

## 5. Localization resources

- [x] 5.1 Add new keys to `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Resources/SharedResource.resx`: `Transactions_AmountFrom`, `Transactions_AmountTo`, `AccountMovements_AmountFrom`, `AccountMovements_AmountTo`, `Filter_AmountRangeInvalid`.
- [x] 5.2 Add matching values to `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Resources/SharedResource.en-US.resx`; done when both pages render English labels and invalid-range message via localization.
- [x] 5.3 Add matching values to `d:/Programacion/FamilyFinances/src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx`; done when Spanish UI renders all new keys without fallback key text.

## 6. Automated test coverage updates

- [x] 6.1 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Api/AccountsApiAdditionalTests.cs` to assert URL includes `minAmount`/`maxAmount` when provided and omits them otherwise; include invariant decimal serialization assertion.
- [x] 6.2 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/Transactions/TransactionsListPageTests.cs` with amount-range scenarios: min-only, max-only, both-bounds inclusive, invalid-range error behavior.
- [x] 6.3 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/Accounts/AccountMovementsPageTests.cs` to verify valid amount filters are passed to `IAccountsApi.GetMovementsAsync(...)` and invalid range blocks request dispatch.
- [x] 6.4 Extend `d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/Ledger/Accounts/AccountMovementsApiTests.cs` with API scenarios for amount filtering: min-only, max-only, bounded inclusive edges, and absolute matching of both signed directions.
- [x] 6.5 Update any impacted mocks/setups for changed method signatures in Web/Application tests; done when all projects compile and test discovery succeeds.

## 7. Validation and readiness checks

- [x] 7.1 Run focused Web tests: `dotnet test d:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release --filter \"FullyQualifiedName~TransactionsListPage|FullyQualifiedName~AccountMovementsPage|FullyQualifiedName~AccountsApiAdditionalTests\"`.
- [x] 7.2 Run focused API integration tests: `dotnet test d:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter \"FullyQualifiedName~AccountMovementsApiTests\"`.
- [x] 7.3 Run full regression suite: `dotnet test d:/Programacion/FamilyFinances/FamilyFinances.sln -c Release`.
- [x] 7.4 Validate OpenSpec artifacts strictly: `openspec validate transaction-amount-range-filter --strict`; done when validation returns success with no schema/style errors.

