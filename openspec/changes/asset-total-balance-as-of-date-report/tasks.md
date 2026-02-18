## 1. Backend Reporting Contract

- [ ] 1.1 Add `AssetTotalBalanceDto` in `src/FamilyFinances.Application/Reporting/Dtos/` with fields `AsOf`, `TotalCents`, and `AssetAccountsCount`.
- [ ] 1.2 Add `GetAssetTotalBalanceQuery` in `src/FamilyFinances.Application/Reporting/Queries/` with required `AsOf` date parameter.
- [ ] 1.3 Add `GetAssetTotalBalanceHandler` in `src/FamilyFinances.Application/Reporting/Handlers/` and wire it in `src/FamilyFinances.Infrastructure/DependencyInjection.cs`.
- [ ] 1.4 Extend `IReportingReadRepository` with `GetAssetTotalBalanceAsync(DateOnly asOf, CancellationToken ct)`.
- [ ] 1.5 Implement repository aggregation in `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` using `AccountNature.Asset` and `BookedOn <= asOf`.
- [ ] 1.6 Add endpoint `GET /api/v1/reports/asset-total-balance` in `src/FamilyFinances.Api/Controllers/V1/ReportsController.cs`.

## 2. Web Report UI

- [ ] 2.1 Extend `src/FamilyFinances.Web/Api/ReportsApi.cs` with `GetAssetTotalBalanceAsync(DateOnly asOf, CancellationToken ct)`.
- [ ] 2.2 Create `src/FamilyFinances.Web/Components/Pages/Reports/AssetTotalBalancePage.razor` with date picker, load action, loading/error states, and total result card.
- [ ] 2.3 Add navigation card in `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` for `Asset Total Balance`.
- [ ] 2.4 Ensure report route is `/reports/asset-total-balance` and accessible from Reports index only for authenticated users.

## 3. Tests

- [ ] 3.1 Add/extend integration tests in `tests/FamilyFinances.Api.IntegrationTests/Reporting/` to verify correct totals, inclusive `asOf`, and zero-result behavior.
- [ ] 3.2 Add/extend application/repository-level reporting tests if present for query/handler validation.
- [ ] 3.3 Add/extend web API client tests in `tests/FamilyFinances.Web.Tests/Api/` for `ReportsApi.GetAssetTotalBalanceAsync`.
- [ ] 3.4 Add/extend web page tests (if applicable in current test stack) for basic render + load behavior.

## 4. Documentation And Validation

- [ ] 4.1 Update `openspec/api-spec.yaml` with the new reports endpoint and new DTO schema.
- [ ] 4.2 Verify OpenSpec artifacts coherence: proposal/design/specs/tasks for this change.
- [ ] 4.3 Run `dotnet build` and ensure zero build warnings.
- [ ] 4.4 Run `dotnet test` and ensure all tests pass without runtime warning noise.
