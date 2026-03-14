## 1. Token governance foundation and asset wiring

- [x] 1.1 Create `src/FamilyFinances.Web/wwwroot/css/ui-tokens.css` as the canonical token registry for typography, spacing, radius, control sizing, chart panel sizing, and semantic chart color aliases.
- [x] 1.2 Add ownership headers/comments in `src/FamilyFinances.Web/wwwroot/css/ui-tokens.css`, `src/FamilyFinances.Web/wwwroot/css/app.css`, and `src/FamilyFinances.Web/wwwroot/css/premium-theme.css` documenting where global primitives are allowed and where only token consumption is allowed.
- [x] 1.3 Update stylesheet load order in `src/FamilyFinances.Web/Components/App.razor` so `ui-tokens.css` is loaded before `app.css` and `premium-theme.css`.
- [x] 1.4 Update `src/FamilyFinances.Web/wwwroot/app.css` import chain to include `css/ui-tokens.css` before `css/app.css` and keep one deterministic entrypoint for shared styling.
- [x] 1.5 Add deterministic fallback token values (for missing premium overrides) in `ui-tokens.css` so login and non-premium pages render without style regressions.

## 2. Shared CSS normalization and ownership boundary cleanup

- [x] 2.1 Refactor hardcoded dark/light surface and text literals in `src/FamilyFinances.Web/wwwroot/css/app.css` to consume canonical tokens.
- [x] 2.2 Refactor chart panel, legend, and canvas sizing literals in `src/FamilyFinances.Web/wwwroot/css/app.css` to token-based values.
- [x] 2.3 Refactor `src/FamilyFinances.Web/wwwroot/css/premium-theme.css` to consume canonical primitives from `ui-tokens.css` and remove duplicated primitive declarations where not theme-specific.
- [x] 2.4 Normalize button geometry rules so export and non-export actions consume the same shared size/radius tokens in `src/FamilyFinances.Web/wwwroot/css/app.css` and `src/FamilyFinances.Web/wwwroot/css/premium-theme.css`.
- [x] 2.5 Sweep component-scoped CSS (`src/FamilyFinances.Web/Components/**/*.razor.css`) and replace repeated primitive literals (font-size, radius, spacing, fixed heights) with shared token references where technically feasible.

## 3. Chart semantic palette contract in C#

- [x] 3.1 Introduce a shared semantic palette helper in `src/FamilyFinances.Web/Features/Reports/Charts/` (new file) that resolves semantic keys (`income`, `expense`, `balance`, `neutral`, indexed-series fallback) to colors.
- [x] 3.2 Update `src/FamilyFinances.Web/Features/Reports/Charts/AnnualChartDatasetAdapter.cs` to resolve series colors through the shared semantic palette helper.
- [x] 3.3 Update `src/FamilyFinances.Web/Features/Reports/Charts/MonthlyChartDatasetAdapter.cs` and related monthly chart model projection paths to resolve colors through the same helper.
- [x] 3.4 Replace hardcoded `ColorHex` assignments in `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` with semantic palette resolution calls.
- [x] 3.5 Replace hardcoded `ColorHex` assignments in `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` and `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupTotalsPage.razor` with semantic palette resolution calls.
- [x] 3.6 Verify existing top-10-plus-others composition behavior in chart builders remains unchanged after palette refactor (no semantic drift in slice grouping).

## 4. Chart runtime token consumption in JavaScript

- [x] 4.1 Refactor `src/FamilyFinances.Web/wwwroot/js/reportCharts.js` to read chart fallback colors from canonical CSS custom properties instead of local hardcoded hex literals.
- [x] 4.2 Replace repeated `pick("--token", "fallback")` literal fallback patterns in `reportCharts.js` with a centralized token resolver map and semantic defaults.
- [x] 4.3 Tokenize composition/pie chart legend sizing and spacing behavior in `reportCharts.js` so legend layout contract is shared across all composition charts.
- [x] 4.4 Keep current marker/cutoff plugin behavior functionally identical while moving visual constants (line color, shade opacity, marker styling) to token-driven values.
- [x] 4.5 Validate chart destruction/recreate flow in `reportCharts.js` still prevents stale instances and memory leaks after token resolver refactor.

## 5. Razor presentation literal cleanup across full app surfaces

- [x] 5.1 Inventory inline style and hardcoded presentation literals in `src/FamilyFinances.Web/Components/**/*.razor` and classify each finding as `tokenize`, `keep-as-dynamic`, or `delete`.
- [x] 5.2 Replace static presentation literals in report chart components (`src/FamilyFinances.Web/Components/Pages/Reports/Charts/*.razor`) with token-backed classes.
- [x] 5.3 Replace static presentation literals in dashboard/report pages (`src/FamilyFinances.Web/Components/Pages/Dashboard/*.razor`, `src/FamilyFinances.Web/Components/Pages/Reports/*.razor`) with token-backed classes.
- [x] 5.4 Replace static presentation literals in operational views (`src/FamilyFinances.Web/Components/Pages/Accounts/*.razor`, `Transactions/*.razor`, `History/*.razor`, `QuickEntry/*.razor`, `Payees/*.razor`, `Login/*.razor`, `Settings/*.razor`) with shared token-backed classes.
- [x] 5.5 For required data-driven color indicators (for example composition legend chips), constrain any remaining inline style usage to explicit CSS custom-property assignment patterns only.
- [x] 5.6 Remove newly-obsolete one-off utility classes created only to preserve previous hardcoded values after token migration.

## 6. Governance documentation updates

- [x] 6.1 Create `docs/frontend-token-governance.md` defining canonical token ownership, allowed override points, semantic chart palette rules, and anti-hardcode do/don't examples.
- [x] 6.2 Update `openspec/FRONTEND_STANDARDS.md` and `openspec/FRONTEND_STANDARDS_AI.md` with explicit enforcement rule: no new hardcoded presentation literals in protected frontend paths.
- [x] 6.3 Add a short release-note entry in `docs/` describing that this change is behavior-neutral and focused on tokenization/governance hardening.

## 7. Automated guardrails and regression tests

- [x] 7.1 Add `tests/FamilyFinances.Web.Tests/Features/Layout/HardcodedStyleGuardTests.cs` to fail on disallowed hardcoded color literals and disallowed inline style usage in protected frontend files.
- [x] 7.2 Add explicit scoped allowlist in `HardcodedStyleGuardTests.cs` for approved data-driven inline style patterns and assert each allowlist entry is path-limited.
- [x] 7.3 Extend `tests/FamilyFinances.Web.Tests/Features/Layout/PremiumThemeCssTests.cs` to verify canonical token files are loaded and consumed in deterministic order.
- [x] 7.4 Extend chart component tests in `tests/FamilyFinances.Web.Tests/Features/Reports/Charts/MonthlyLineChartTests.cs`, `AnnualLineChartTests.cs`, and `AnnualCompositionChartTests.cs` to validate token/semantic palette usage and unchanged data semantics.
- [x] 7.5 Extend page-level tests in `tests/FamilyFinances.Web.Tests/Features/Dashboard/DashboardPageTests.cs` and `tests/FamilyFinances.Web.Tests/Features/Reports/EconomicStatePageTests.cs` to verify chart payload color semantics are resolved through shared palette paths.

## 8. Final validation and apply readiness

- [x] 8.1 Run `dotnet build src/FamilyFinances.Web/FamilyFinances.Web.csproj -c Release` and resolve compile warnings/errors introduced by tokenization refactors.
- [x] 8.2 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release` and fix any test regressions from style/palette normalization.
- [x] 8.3 Run `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release` and ensure chart adapter/application tests remain green after palette updates.
- [x] 8.4 Run repository grep checks (`rg`) to confirm no new disallowed hardcoded literals were introduced in protected Web UI paths.
- [x] 8.5 Execute manual smoke checks on Dashboard, Economic State (all tabs), Accounts, Quick Entry, Payees, Transactions, History, Login, and Settings in desktop/mobile breakpoints to validate behavior parity and visual uniformity.
- [x] 8.6 Align month-focused daily evolution contract so charts normalize against month opening balance (zero baseline) without suppressing day-1 movement.
- [x] 8.7 Add additive `OpeningBalanceCents` metadata plumbing (`MonthlyBalanceChartDto`, `MonthlyChartSeriesDto`, repository mapping, frontend chart adapters) to keep normalization deterministic and backward-compatible.
