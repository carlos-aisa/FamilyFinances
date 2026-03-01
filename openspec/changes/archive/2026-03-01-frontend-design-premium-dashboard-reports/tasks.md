## 1. Premium token foundation and asset wiring

- [x] 1.1 Create `src/FamilyFinances.Web/wwwroot/css/premium-theme.css` with centralized tokens for color, typography, spacing, radius, elevation, and chart palette (dark-first baseline).
- [x] 1.2 Update `src/FamilyFinances.Web/Components/App.razor` (or current host stylesheet include point) to load `premium-theme.css` after the active shared stylesheet so tokens can override baseline presentation safely.
- [x] 1.3 Add shell activation marker (for example `ff-premium`) in `src/FamilyFinances.Web/Components/Layout/MainLayout.razor` and wire base shell classes without changing route/body rendering behavior.
- [x] 1.4 Normalize shared style ownership between `src/FamilyFinances.Web/wwwroot/app.css` and `src/FamilyFinances.Web/wwwroot/css/app.css` (choose one canonical file and remove duplicate premium rules).
- [x] 1.5 Refactor core primitives in the canonical shared stylesheet (`panel`, `metric-card`, `data-table`, `tab`, `alert/info`) to consume premium tokens instead of page-specific hardcoded values.

## 2. Layout and navigation premium chrome

- [x] 2.1 Update `src/FamilyFinances.Web/Components/Layout/MainLayout.razor.css` with premium shell spacing, background layering, and content container primitives.
- [x] 2.2 Update `src/FamilyFinances.Web/Components/Layout/NavMenu.razor.css` with premium navigation states (active/hover/focus), keeping existing navigation IA and routes unchanged.
- [x] 2.3 Verify `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` does not introduce language selector controls in navigation and keeps language management scoped to settings.
- [x] 2.4 Preserve existing authenticated navigation items and route behavior while applying premium visual classes (`Dashboard`, `Reports`, `Settings`, etc.).

## 3. Dashboard presentation refresh

- [x] 3.1 Update `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` to apply premium page header hierarchy classes (title/subtitle/actions) without changing component logic.
- [x] 3.2 Apply premium panel primitives to quick-entry cards/widgets and account nature cards in `DashboardPage.razor` while preserving current interaction semantics.
- [x] 3.3 Add/update Dashboard-specific style block (`src/FamilyFinances.Web/wwwroot/css/app.css` canonical section or new scoped CSS file) for balanced desktop/mobile spacing and visual rhythm.
- [x] 3.4 Validate selected account state, from/to badges, loading, empty, and unauthorized states keep current behavior with upgraded visual treatment.

## 4. Reports surface premium refresh

- [x] 4.1 Update `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` to use premium report-entry card patterns while preserving route targets.
- [x] 4.2 Update `src/FamilyFinances.Web/Components/Pages/Reports/MonthlySummaryPage.razor` to apply shared premium filter panel, KPI cards, chart panel, and insights panel classes.
- [x] 4.3 Update `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` with premium tab strip, KPI rows, info block, and chart-container hierarchy (no metric semantics change).
- [x] 4.4 Apply premium table density and readability classes to report tables used in monthly insights/evolution panels (`AccountTotals`, `AccountGroupTotals`, and related report components as needed).
- [x] 4.5 Ensure report empty/loading/error states remain consistent and use shared premium state primitives across report pages.

## 5. Chart theming standardization

- [x] 5.1 Refactor `src/FamilyFinances.Web/wwwroot/js/reportCharts.js` to resolve axis/grid/tooltip/legend colors from shared CSS tokens (`getComputedStyle`) instead of fixed hex literals.
- [x] 5.2 Update `src/FamilyFinances.Web/Components/Pages/Reports/Charts/AnnualLineChart.razor` and `MonthlyLineChart.razor` chrome classes so chart cards and controls align with premium chart panel standards.
- [x] 5.3 Align chart export button visual behavior with premium button states while preserving export functionality and existing test IDs.
- [x] 5.4 Validate income/expense semantic color mapping remains consistent after token-driven chart styling changes.

## 6. Settings alignment and documentation

- [x] 6.1 Update `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor` and optional scoped styling so Appearance/Language/Backup cards match premium visual language.
- [x] 6.2 Confirm language switch remains available only in settings and still performs current live-switch behavior.
- [x] 6.3 Update frontend documentation (`docs/*` and/or `README.md`) with premium token governance, dark-first default rule, and explicit navigation policy for language control.
- [x] 6.4 Add concise rollback notes to documentation describing how to disable premium shell marker and restore baseline visuals quickly.

## 7. Test updates and validation

- [x] 7.1 Update Web layout/navigation tests (including `tests/FamilyFinances.Web.Tests/Features/Layout/NavMenuEconomicStateTests.cs`) to assert premium class markers and absence of language selector in nav.
- [x] 7.2 Update report UI tests (`tests/FamilyFinances.Web.Tests/Features/Reports/ReportsIndexPageTests.cs`, `MonthlySummaryPageTests.cs`, `EconomicStatePageTests.cs`, `ReportResponsiveLayoutTests.cs`) for premium markup/class changes without weakening behavioral assertions.
- [x] 7.3 Update chart tests (`tests/FamilyFinances.Web.Tests/Features/Reports/Charts/AnnualLineChartTests.cs`) for tokenized chart config expectations and preserved export hooks.
- [x] 7.4 Update settings tests (`tests/FamilyFinances.Web.Tests/Features/Settings/SettingsPageLanguageTests.cs`) to confirm language controls remain on `/settings` and continue to function after styling changes.
- [x] 7.5 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release` and fix all regressions.
- [x] 7.6 Run `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~Reporting"` to confirm reporting contracts remain unchanged.
- [x] 7.7 Run `dotnet build FamilyFinances.sln -c Release` and address warnings introduced by this change.
- [x] 7.8 Perform manual smoke checks in desktop and mobile breakpoints for Dashboard, Monthly Summary, Economic State, Reports Index, and Settings to verify premium dark-first presentation and unchanged workflows.
