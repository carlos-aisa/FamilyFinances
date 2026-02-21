## 1. Backend Monthly Evolution Contract

- [x] 1.1 Add graph-ready DTOs in `src/FamilyFinances.Application/Reporting/Dtos/` (`MonthlyEvolutionScope`, `MonthlyEvolutionReportDto`, `MonthlyEvolutionSeriesDto`, `MonthlyEvolutionPointDto`) using integer cents fields only.
- [x] 1.2 Add `GetMonthlyEvolutionQuery` in `src/FamilyFinances.Application/Reporting/Queries/` with `Year` and `Scope` parameters.
- [x] 1.3 Add `GetMonthlyEvolutionHandler` in `src/FamilyFinances.Application/Reporting/Handlers/` to validate year bounds and delegate to repository.
- [x] 1.4 Register `GetMonthlyEvolutionHandler` in `src/FamilyFinances.Infrastructure/DependencyInjection.cs`.
- [x] 1.5 Extend `IReportingReadRepository` in `src/FamilyFinances.Application/Reporting/Abstractions/IReportingReadRepository.cs` with `GetMonthlyEvolutionAsync(int year, MonthlyEvolutionScope scope, CancellationToken ct)`.
- [x] 1.6 Add endpoint `GET /api/v1/reports/monthly-evolution` in `src/FamilyFinances.Api/Controllers/V1/ReportsController.cs` with explicit `year` and `scope` query parameters and `400` for invalid input.

## 2. Repository Aggregation And Delta Semantics

- [x] 2.1 Implement monthly evolution aggregation in `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` with deterministic month ordering and continuous buckets.
- [x] 2.2 Implement account-level canonical monthly balances first, then derive `asset-total` and `account-groups` scopes from that canonical base.
- [x] 2.3 Implement delta formulas exactly: `DeltaVsPreviousMonthCents = End(M) - End(M-1)` and `DeltaVsYearStartCents = End(M) - YearStartBaseline`.
- [x] 2.4 Implement year window logic: `1..currentMonth` for current year, `1..12` for past years.
- [x] 2.5 Ensure months with no activity are still returned with carry-forward balances.
- [x] 2.6 Use fiscal-year snapshots as baseline when available, with per-series fallback to historical sums when snapshot baseline is missing.

## 3. Web API Client And Report Page

- [x] 3.1 Extend `src/FamilyFinances.Web/Api/ReportsApi.cs` with `GetMonthlyEvolutionAsync(int year, MonthlyEvolutionScope scope, CancellationToken ct)` including token/unauthorized handling consistent with existing methods.
- [x] 3.2 Create `src/FamilyFinances.Web/Components/Pages/Reports/MonthlyEvolutionPage.razor` with route `/reports/monthly-evolution`.
- [x] 3.3 Implement page controls: year selector and scope tabs (`Accounts`, `Asset Total`, `Account Groups`) that trigger reload on change.
- [x] 3.4 Implement page states: loading, error, empty, and success states using existing Reports page visual patterns.
- [x] 3.5 Render graph-ready data in table-first form for all scopes (including monthly rows with end balance and both deltas).
- [x] 3.6 Include explicit chart placeholder panel in the page without introducing chart libraries.
- [x] 3.7 Add `Monthly Evolution` navigation card in `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` linking to `/reports/monthly-evolution`.

## 4. Tests

- [x] 4.1 Add application tests in `tests/FamilyFinances.Application.Tests/Reporting/` for handler validation/delegation behavior.
- [x] 4.2 Add API integration tests in `tests/FamilyFinances.Api.IntegrationTests/Reporting/` for:
- [x] 4.3 `scope=accounts` correctness (series per account and delta semantics).
- [x] 4.4 `scope=asset-total` correctness (single series and asset-only aggregation).
- [x] 4.5 `scope=account-groups` correctness (group aggregation and deterministic points).
- [x] 4.6 Month window behavior (`current year` vs `historical year`) and no-activity month carry-forward behavior.
- [x] 4.7 Bad request behavior for missing/invalid `year` or `scope` query parameters.
- [x] 4.8 Add web API client tests in `tests/FamilyFinances.Web.Tests/Api/` for URL/query construction, deserialization, and auth/error behavior.
- [x] 4.9 Add web page tests in `tests/FamilyFinances.Web.Tests/Features/Reports/` for render, year/scope switching, and load/error states.

## 5. Documentation And Validation

- [x] 5.1 Update `openspec/api-spec.yaml` with `GET /api/v1/reports/monthly-evolution` and all new monthly evolution schemas.
- [x] 5.2 Ensure OpenSpec artifact coherence (`proposal.md`, `design.md`, `specs/**/*.md`, `tasks.md`) after implementation choices are finalized.
- [x] 5.3 Run `dotnet build FamilyFinances.sln` and ensure zero build warnings.
- [x] 5.4 Run `dotnet test FamilyFinances.sln` and ensure all tests pass without runtime warning noise.
