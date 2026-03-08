## 1. Shared UI primitives and navigation contracts

- [x] 1.1 Create `src/FamilyFinances.Web/Components/Shared/SingleOpenAccordion.razor` with parameters `Id`, `IReadOnlyList<Section> Sections`, `RenderFragment<Section> SectionTemplate`, and single-open behavior using `data-bs-parent`.
- [x] 1.2 Add `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionOriginContext.cs` with whitelisted origin tokens (`transactions`, `accounts-movements`, `history-transactions`, `history-movements`, `report-category-totals`, `report-account-totals`) and typed optional values (`accountId`, `from`, `to`, `year`).
- [x] 1.3 Add helper methods in `TransactionOriginContext` to parse query values safely (`Guid`, `DateOnly`, `int`) and fallback to `transactions` when token is unknown.
- [x] 1.4 Update shared style primitives in `src/FamilyFinances.Web/wwwroot/css/app.css` and `src/FamilyFinances.Web/wwwroot/css/premium-theme.css` for consistent rectangular export buttons, accordion spacing, and reusable report-panel alignment classes.

## 2. Copy and localization alignment

- [x] 2.1 Update dashboard month-context wording keys in `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx` and `src/FamilyFinances.Web/Resources/SharedResource.en-US.resx` so UI text uses `Current month` semantics instead of `Selected month` where this change requires it.
- [x] 2.2 Add localization keys for: accounts page `updated as of` sentence, Quick Entry guidance labels, transactions `Payee` column header, report naming `Account Analysis`, and Economic State evolution `Balance` column label.
- [x] 2.3 Replace direct/hardcoded labels in affected Razor pages with localized key usage (`IStringLocalizer<SharedResource>`) for every new/changed visible string.
- [x] 2.4 Update `tests/FamilyFinances.Web.Tests/Features/Localization/SharedResourceLocalizationTests.cs` to assert all newly introduced keys exist in both locales.

## 3. Dashboard and report-entry presentation updates

- [x] 3.1 Update `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` subtitle/description text to current-month terminology and keep dashboard month handling fixed to current month in this change.
- [x] 3.2 Update `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` labels so the previous `Monthly Summary` entry is presented as localized `Account Analysis` while keeping route target `/reports/monthly-summary` unchanged.
- [x] 3.3 Review report bottom tag/badge rendering in `MonthlySummaryPage.razor`, `EconomicStatePage.razor`, `CategoryTotalsPage.razor`, `AccountTotalsPage.razor`, and `AccountGroupTotalsPage.razor`; remove or replace tags whose semantics do not match page content.
- [x] 3.4 Ensure any retained footer tags use explicit localized semantics and are consistent across report pages.

## 4. Accounts page accordion and update timestamp

- [x] 4.1 Refactor `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor` to render nature groups through `SingleOpenAccordion` and keep exactly one open section at a time.
- [x] 4.2 Keep all existing account row actions (rename, view movements, close/reopen, delete where applicable) functional after accordion refactor.
- [x] 4.3 Remove per-group footer copy based on `Accounts_CurrentMonth_Basis` from account group blocks.
- [x] 4.4 Add one page-level description message in `AccountsListPage.razor` with format `Accounts updated as of {currentDate}` using localized resource and deterministic date format.

## 5. Quick Entry discoverability, guidance, and date persistence

- [x] 5.1 Update `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor` to add one global accounts search input above grouped accounts and filter by account name plus account nature/type label.
- [x] 5.2 Replace per-group expanded logic in `QuickEntryPage.razor` with `SingleOpenAccordion`; auto-expand matching group when global search has results.
- [x] 5.3 Extend `src/FamilyFinances.Web/Components/Dashboard/QuickEntrySpec.cs` with configurable guidance/description property and populate guidance for Expense, Income, Transfer, and Refund entries.
- [x] 5.4 Move guidance rendering to the top description/header area of active quick-entry controls in `src/FamilyFinances.Web/Components/QuickEntry/QuickEntryDrawer.razor` (not footer-only).
- [x] 5.5 Implement shared selected-date state in `QuickEntryPage.razor` and `QuickEntryDrawer.razor` so selected date persists when switching quick-entry mode.

## 6. Origin-preserving navigation for transactions and history

- [x] 6.1 Update `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` to navigate to transaction detail with query parameters `origin=accounts-movements&accountId={id}` and optional period context.
- [x] 6.2 Update `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionDetailPage.razor` to parse `TransactionOriginContext`, resolve back target by origin, and pass full context to edit route.
- [x] 6.3 Update `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionEditPage.razor` so cancel/back/save paths preserve origin context and return to deterministic previous surface.
- [x] 6.4 Update `src/FamilyFinances.Web/Components/Pages/History/HistoryTransactionsPage.razor` and `src/FamilyFinances.Web/Components/Pages/History/HistoryMovementsPage.razor` to emit history origin tokens and keep read-only behavior.
- [x] 6.5 Add defensive fallback behavior: invalid/unknown origin values must never redirect to arbitrary URLs and must default to `/transactions`.

## 7. Payees and transactions presentation refresh

- [x] 7.1 Refactor `src/FamilyFinances.Web/Components/Pages/Payees/PayeesPage.razor` from long list/table presentation to responsive card grid (`name + edit + delete`) while retaining search behavior.
- [x] 7.2 Add card-grid styles for payees in `src/FamilyFinances.Web/wwwroot/css/app.css` (row wrapping, spacing, compact action alignment, mobile stacking behavior).
- [x] 7.3 Add dedicated `Payee` column to `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor` and remove payee text duplication from description/subheadline rendering.
- [x] 7.4 Extend `src/FamilyFinances.Application/Ledger/Transactions/Dtos/TransactionListItemDto.cs` with additive `string? PayeeName` field and update mappings in `ListTransactionsHandler.cs` and `ListHistoricalTransactionsHandler.cs`.
- [x] 7.5 Keep transaction search behavior matching both description and payee fields after DTO/view changes.

## 8. Report table interactions (sorting + drilldown)

- [x] 8.1 Add sortable header controls to `src/FamilyFinances.Web/Components/Pages/Reports/CategoryTotalsPage.razor` for account, total, and transactions columns with deterministic asc/desc toggling.
- [x] 8.2 Add row click drilldown in `CategoryTotalsPage.razor` to `/accounts/{accountId}/movements?origin=report-category-totals&from={yyyy-MM-dd}&to={yyyy-MM-dd}`.
- [x] 8.3 Add row click drilldown in `src/FamilyFinances.Web/Components/Pages/Reports/AccountTotalsPage.razor` to `/accounts/{accountId}/movements?origin=report-account-totals&from={yyyy-MM-dd}&to={yyyy-MM-dd}`.
- [x] 8.4 Ensure sort controls and row-click interactions do not conflict (header buttons sort, body rows drill down).

## 9. Economic State evolution layout and monthly table rules

- [x] 9.1 Remove `Load report` button dependency from `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` and trigger reload automatically on committed year/focused-month changes.
- [x] 9.2 Refactor `src/FamilyFinances.Web/Components/Pages/Reports/AssetTotalEvolutionPanel.razor`, `IncomeEvolutionPanel.razor`, and `ExpenseEvolutionPanel.razor` to 3-column desktop layout (`table 33% + composition pie 33% + charts 33%`).
- [x] 9.3 Update evolution monthly tables in those panels to show months up to current month for current year and full 12 months for past years.
- [x] 9.4 Rename `Variation vs previous month` column to localized `Balance` and place it before final balance column in all three evolution panels.
- [x] 9.5 Build month-focused composition pie in each evolution panel using top-10 contributors plus aggregated `Others` slice for remaining contributors.

## 10. Monthly/annual chart consistency and account-group visibility

- [x] 10.1 Update `src/FamilyFinances.Web/Components/Pages/Reports/Charts/MonthlyLineChart.razor` so month-focused charts consistently use full-month axis and pass cutoff metadata to chart JS.
- [x] 10.2 Update `src/FamilyFinances.Web/Components/Pages/Reports/Charts/AnnualLineChart.razor` and `AnnualBarChart.razor` to provide current-year `markerMonth`/cutoff metadata and de-emphasize future months.
- [x] 10.3 Update `src/FamilyFinances.Web/wwwroot/js/reportCharts.js` plugin logic to render cutoff marker and disabled future area for both monthly and annual contexts while preserving existing datasets when marker is null.
- [x] 10.4 Align export button appearance in `MonthlyLineChart.razor`, `AnnualLineChart.razor`, `AnnualBarChart.razor`, and `AnnualCompositionChart.razor` with the shared app button shape.
- [x] 10.5 Fix `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor` layout so the monthly evolution chart is visible without being pushed below hidden overflow regions.

## 11. Login remembered-username UX

- [x] 11.1 Extend login JS helper (`src/FamilyFinances.Web/wwwroot/js/auth.js` or equivalent login helper location) with `getLastUsername()` and `setLastUsername(value)` using local storage key `ff_last_username`.
- [x] 11.2 Update `src/FamilyFinances.Web/Components/Pages/Login/LoginPage.razor` first-render flow to prefill login identifier input from `ff_last_username` when available.
- [x] 11.3 Update successful login path in `LoginPage.razor` to store the submitted identifier in `ff_last_username` only after successful authentication.
- [x] 11.4 Ensure password values are never persisted in local storage, session storage, query parameters, or logs.

## 12. Automated tests and verification

- [x] 12.1 Update `tests/FamilyFinances.Application.Tests/Ledger/Transactions/ListTransactionsHandlerTests.cs` and `ListTransactionsHandlerRefundTests.cs` for `TransactionListItemDto.PayeeName` and cleaned description/subheadline behavior.
- [x] 12.2 Add/extend web tests for accounts accordion and page-level update message in `tests/FamilyFinances.Web.Tests/Features/Accounts/AccountsListPageTests.cs`.
- [x] 12.3 Add new Quick Entry tests (new file `tests/FamilyFinances.Web.Tests/Features/QuickEntry/QuickEntryPageTests.cs`) for global search, single-open accordion, per-mode guidance, and shared date persistence.
- [x] 12.4 Add new transaction navigation tests (new file `tests/FamilyFinances.Web.Tests/Features/Transactions/TransactionNavigationContextTests.cs`) for account/history/report origin back paths.
- [x] 12.5 Add new payees UI tests (new file `tests/FamilyFinances.Web.Tests/Features/Payees/PayeesPageTests.cs`) for card rendering, search filtering, rename, and delete.
- [x] 12.6 Add/extend transactions list tests (new file `tests/FamilyFinances.Web.Tests/Features/Transactions/TransactionsListPageTests.cs`) for payee column visibility and search by payee.
- [x] 12.7 Add new report interaction tests (new file `tests/FamilyFinances.Web.Tests/Features/Reports/CategoryTotalsPageTests.cs`) for sorting and row drilldown; extend `AccountTotalsPageTests.cs` for drilldown behavior.
- [x] 12.8 Extend `tests/FamilyFinances.Web.Tests/Features/Reports/EconomicStatePageTests.cs` and `ReportResponsiveLayoutTests.cs` for 3-column layout, month visibility rules, renamed balance column, and top-10-plus-others composition.
- [x] 12.9 Extend `tests/FamilyFinances.Web.Tests/Features/Reports/Charts/MonthlyLineChartTests.cs` and `AnnualLineChartTests.cs` for cutoff marker payload and future-area de-emphasis contracts.
- [x] 12.10 Add login remembered-username tests (new file `tests/FamilyFinances.Web.Tests/Features/Login/LoginPageTests.cs`) to validate prefill and no password persistence.
- [x] 12.11 Run `dotnet build src/FamilyFinances.Web/FamilyFinances.Web.csproj -c Release` and fix compile warnings/errors introduced by the change.
- [x] 12.12 Run `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release`, `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release`, and targeted reporting integration tests in `tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj`.
- [x] 12.13 Create `docs/v0.9.8-ui-full-views-review-notes.md` documenting changed UX flows, navigation-context tokens, chart cutoff semantics, and known deferred items (`global-filter-behavior-semantics`, `ui-hardcode-normalization`).
- [x] 12.14 Execute manual smoke checks on desktop and mobile breakpoints for: accounts accordion, quick-entry search/date persistence, report drilldowns, economic-state 3-column layout, chart cutoff rendering, and login username prefill.

