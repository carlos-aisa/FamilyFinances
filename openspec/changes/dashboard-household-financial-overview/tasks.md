## 1. Routing And Information Architecture Boundary

- [x] 1.1 Create a dedicated quick-entry route host at `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor` with `@page "/quick-entry"` and move the capture workload out of dashboard route composition.
- [x] 1.2 Refactor `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` so `/` renders analytics blocks only (no quick-entry card/workspace sections and no tab containers).
- [x] 1.3 Update `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` to expose `Quick Entry` plus existing `Dashboard`/`Reports` links while preserving `/reports/*` route reachability semantics.
- [x] 1.4 Update `src/FamilyFinances.Web/Components/Layout/NavMenu.razor.css` to style the new `Quick Entry` item consistently with existing premium nav states.
- [x] 1.5 Verify no language selector is introduced in navigation and keep language controls scoped to Settings (`src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor`).

## 2. Dashboard Overview Read Model And API Contract

- [x] 2.1 Add dashboard overview DTOs in `src/FamilyFinances.Application/Reporting/Dtos/` (for KPI values, deltas, group chart points, compact list rows, and data-sufficiency state enum).
- [x] 2.2 Add `GetDashboardOverviewQuery` in `src/FamilyFinances.Application/Reporting/Queries/` with selected/current month context inputs.
- [x] 2.3 Implement `GetDashboardOverviewHandler` in `src/FamilyFinances.Application/Reporting/Handlers/` reusing reporting semantics (`Net = Income - Expense`) and previous-month comparison rules.
- [x] 2.4 Extend `src/FamilyFinances.Application/Reporting/Abstractions/IReportingReadRepository.cs` with dashboard overview read methods required by the new handler.
- [x] 2.5 Implement repository methods in `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` for current-month/previous-month KPIs, YTD aggregate, group-state dataset, and compact list source data.
- [x] 2.6 Add/extend API endpoint in `src/FamilyFinances.Api/Controllers/V1/ReportsController.cs` (for example `GET /api/v1/reports/dashboard-overview`) with strict input validation.
- [x] 2.7 Update Web API client in `src/FamilyFinances.Web/Api/ReportsApi.cs` with strongly typed `GetDashboardOverviewAsync` method mapping to the new endpoint.

## 3. Dashboard Analytical Composition (No Tabs)

- [x] 3.1 Implement dashboard KPI strip block in `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` showing Income, Expense, Net Result, and Net Worth with previous-month deltas.
- [x] 3.2 Implement the primary monthly `Income vs Expense` chart block in Dashboard by reusing existing chart contracts/components (`src/FamilyFinances.Web/Components/Pages/Reports/Charts/*`).
- [x] 3.3 Implement account-group current-state chart block in Dashboard (horizontal-bars style contract) with deterministic ordering.
- [x] 3.4 Implement annual YTD accumulation card/block in Dashboard with compact trend rendering.
- [x] 3.5 Implement one compact analytical list block in Dashboard (Top expenses and/or anomalies) with strict row cap (`max 5-8`).
- [x] 3.6 Add explicit dashboard data-sufficiency UI states (`Complete`, `Partial`, `InsufficientHistory`) and localized fallback messages.
- [x] 3.7 Update shared styles in `src/FamilyFinances.Web/wwwroot/css/app.css` and/or `src/FamilyFinances.Web/wwwroot/css/premium-theme.css` to enforce fixed-height desktop analytical rows and minimized-scroll behavior.

## 4. Quick Entry Workspace Migration

- [x] 4.1 Move existing quick-entry capture sections from `DashboardPage.razor` into `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor` while preserving expense/income/transfer/refund semantics.
- [x] 4.2 Reuse existing quick-entry components (`src/FamilyFinances.Web/Components/QuickEntry/QuickEntryCard.razor`, `QuickEntryDrawer.razor`) without changing business behavior.
- [x] 4.3 Rehost `MortgagePaymentWidget` and `MultiSplitWidget` in the quick-entry route preserving expand/collapse and submit behavior.
- [x] 4.4 Add/adjust quick-entry page styles in canonical stylesheet (`src/FamilyFinances.Web/wwwroot/css/app.css`) for premium consistency and mobile stacking.
- [x] 4.5 Ensure dashboard route `/` contains no residual capture actions or widget toggles after migration.

## 5. Accounts Dual-Balance Presentation

- [x] 5.1 Extend Accounts list view model usage in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor` to include accumulated balance and current-month period balance per account row.
- [x] 5.2 If missing in current contracts, add required read-model fields in application DTOs/repository queries (`src/FamilyFinances.Application/Reporting/Dtos/AccountBalanceDto.cs` and repository implementations).
- [x] 5.3 Update Accounts list table headers/columns and formatting in `AccountsListPage.razor` to render both balance lenses with existing monetary conventions.
- [x] 5.4 Add explicit period-basis label text (`Current month`) in Accounts UI to avoid ambiguity.

## 6. Economic State Report Enhancements

- [x] 6.1 Add Expense evolution tab selection in `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` symmetric to existing Asset/Income evolution tab logic.
- [x] 6.2 Create `src/FamilyFinances.Web/Components/Pages/Reports/ExpenseEvolutionPanel.razor` (or equivalent) reusing existing evolution panel patterns and shared chart components.
- [x] 6.3 Extend Economic State summary area in `EconomicStatePage.razor` to render monthly net list (`Income - Expense`).
- [x] 6.4 Add annual month-by-month Income vs Expense bar comparison block in `EconomicStatePage.razor` using shared annual chart contracts.
- [x] 6.5 Keep existing stock/flow KPI semantics unchanged and verify labels stay explicit.

## 7. Monthly Evolution Scope And Group Evolution Semantics

- [x] 7.1 Extend `MonthlyEvolutionScope` enum in `src/FamilyFinances.Application/Reporting/Dtos/MonthlyEvolutionReportDto.cs` with `ExpenseTotal`.
- [x] 7.2 Update scope parsing in `src/FamilyFinances.Api/Controllers/V1/ReportsController.cs` to accept `expense-total` and reject invalid values deterministically.
- [x] 7.3 Update scope-to-query mapping in `src/FamilyFinances.Web/Api/ReportsApi.cs` to include `MonthlyEvolutionScope.ExpenseTotal`.
- [x] 7.4 Update `src/FamilyFinances.Application/Reporting/Handlers/GetMonthlyEvolutionHandler.cs` and `IReportingReadRepository` / `ReportingReadRepository` to compute expense-total series.
- [x] 7.5 Update `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor` to display exact selected-month balance in the list context.
- [x] 7.6 Update Account Group annual evolution rendering in `AccountGroupStateEvolutionPanel.razor` to month-result bars (non-cumulative semantics) with explicit labeling.
- [x] 7.7 Rebalance list/chart width allocation in `AccountGroupStateEvolutionPanel.razor` + shared CSS and remove/relocate low-value comparability info card.

## 8. Account Totals Sorting Contract

- [x] 8.1 Update `src/FamilyFinances.Web/Components/Pages/Reports/AccountTotalsPage.razor` default ordering to net change within each nature group.
- [x] 8.2 Implement sortable header click behavior in `AccountTotalsPage.razor` (column + direction state, deterministic toggling).
- [x] 8.3 Add visual sort indicators and keyboard-accessible sortable header markup.
- [x] 8.4 Ensure existing export behavior still works with active sorting.

## 9. Dashboard Compact Insights Integration

- [x] 9.1 Define deterministic compact-list prioritization rules (top contributors vs anomalies) in application-level dashboard overview assembly.
- [x] 9.2 Implement dashboard compact list mapping in the new overview handler to reuse existing reporting insight semantics from `src/FamilyFinances.Application/Reporting/Internal/ReportingInsightsCalculator.cs` where possible.
- [x] 9.3 Enforce row cap in dashboard rendering and keep key numeric columns readable under constrained layout.

## 10. Localization And Documentation

- [x] 10.1 Add new localization keys for dashboard overview blocks, quick-entry navigation label, data-sufficiency states, accounts dual-balance headers, and new report texts in `src/FamilyFinances.Web/Resources/SharedResource.resx`, `SharedResource.es-ES.resx`, and `SharedResource.en-US.resx`.
- [x] 10.2 Update `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx` with Spanish strings for all new keys.
- [x] 10.3 Update `src/FamilyFinances.Web/Resources/SharedResource.en-US.resx` with English strings for all new keys.
- [x] 10.4 Keep `src/FamilyFinances.Web/Resources/SharedResource.resx` synchronized as baseline resource.
- [x] 10.5 Update user/developer documentation (`README.md` and relevant docs) to describe dashboard analytics-first intent and quick-entry route separation.

## 11. Automated Test Updates

- [x] 11.1 Add dashboard page behavior tests under `tests/FamilyFinances.Web.Tests/Features/Dashboard/` covering: no tabs, no report shortcut cards, KPI strip presence, and block rendering.
- [x] 11.2 Add/extend navigation tests in `tests/FamilyFinances.Web.Tests/Features/Layout/` to verify `Quick Entry` link presence and language selector absence in nav.
- [x] 11.3 Add/extend accounts page tests for dual-balance columns in `tests/FamilyFinances.Web.Tests/Features/` (Accounts area).
- [x] 11.4 Update `tests/FamilyFinances.Web.Tests/Features/Reports/EconomicStatePageTests.cs` for expense tab, monthly net list, and annual Income vs Expense bar block expectations.
- [x] 11.5 Update report evolution tests (including `AccountGroupTotalsPageTests.cs` / `ReportResponsiveLayoutTests.cs`) for selected-month balance semantics and bar-based annual evolution.
- [x] 11.6 Update `tests/FamilyFinances.Web.Tests/Features/Reports/AccountTotalsPageTests.cs` for default net-change ordering and header-driven sorting behavior.
- [x] 11.7 Add/extend application tests in `tests/FamilyFinances.Application.Tests/Reporting/` for expense-total scope, dashboard overview assembly, and data-sufficiency states.
- [x] 11.8 Add/extend API integration tests in `tests/FamilyFinances.Api.IntegrationTests/Reporting/` for dashboard-overview contract and `expense-total` monthly-evolution scope.
- [x] 11.9 Update Web API client tests in `tests/FamilyFinances.Web.Tests/Api/ReportsApiTests.cs` for new dashboard endpoint and scope mapping.

## 12. Validation And Release Readiness

- [x] 12.1 Run `dotnet build FamilyFinances.sln -c Release` and fix any warnings/errors introduced by this change.
- [x] 12.2 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release` and resolve regressions.
- [x] 12.3 Run `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release --filter "FullyQualifiedName~Reporting"` and resolve regressions.
- [x] 12.4 Run `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~Reporting"` and resolve regressions.
- [x] 12.5 Run `openspec validate dashboard-household-financial-overview` and confirm no artifact/schema validation errors.
- [ ] 12.6 Execute manual smoke checks (desktop + mobile): Dashboard at-a-glance layout, `/quick-entry` capture flows, Accounts dual-balance view, Economic State expense tab, and Account Totals sorting interactions.
