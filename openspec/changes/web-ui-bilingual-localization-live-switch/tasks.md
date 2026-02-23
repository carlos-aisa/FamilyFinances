## 1. Web Localization Infrastructure

- [x] 1.1 Configure localization services and middleware in `src/FamilyFinances.Web/Program.cs` with supported cultures `es-ES` and `en-US`, default culture `es-ES`, and a provider chain compatible with `.AspNetCore.Culture` cookie resolution.
- [x] 1.2 Add a client culture helper script at `src/FamilyFinances.Web/wwwroot/js/culture.js` exposing `getCulture(): string` and `setCulture(culture: string): string`, persisting both localStorage key and `.AspNetCore.Culture` cookie values.
- [x] 1.3 Register the culture helper script in `src/FamilyFinances.Web/Components/App.razor` alongside existing scripts and ensure script load order allows `NavMenu` JS interop usage on first interactive render.

## 2. Global Language Selector and Immediate Switch

- [x] 2.1 Extend `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` top controls to include a language selector with exactly two options (`es-ES`, `en-US`) while preserving existing theme toggle behavior.
- [x] 2.2 Implement selector change handling in `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` using JS interop call pattern `cultureHelper.setCulture(selected)` and immediate route reload pattern `NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true)`.
- [x] 2.3 Ensure selector initialization on first render reads persisted culture via `cultureHelper.getCulture()` and updates UI selected value deterministically.
- [x] 2.4 Replace static HTML language metadata behavior in `src/FamilyFinances.Web/Components/App.razor` so rendered language metadata aligns with active culture strategy rather than fixed `lang="en"`.

## 3. Resource Files and Localized Text Coverage

- [x] 3.1 Create bilingual resource files for shared shell/common strings (including `NavMenu`, app loading labels, auth-required messages) under a consistent folder strategy in `src/FamilyFinances.Web/Resources/**`.
- [x] 3.2 Localize shared date preset labels in `src/FamilyFinances.Web/Components/Shared/DateRangePresets.razor` by replacing hardcoded visible labels (`This Month`, `Last Month`, `This Quarter`, `This Year`, `Last Year`, `All Time`) with resource keys.
- [x] 3.3 Localize transaction pages in `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`, `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionDetailPage.razor`, and `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionEditPage.razor` for titles, actions, filter labels, states, and notices.
- [x] 3.4 Localize account movement UI strings in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` including table headers, filter sections, modal labels, alerts, and empty/loading states.
- [x] 3.5 Localize report index card labels and descriptions in `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` and any touched dashboard quick-entry labels in `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor` and child quick-entry components.

## 4. Culture-Driven Formatting Standardization

- [x] 4.1 Replace hardcoded `new CultureInfo("es-ES")` date rendering in `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`, `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionDetailPage.razor`, and `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` with active-culture formatting.
- [x] 4.2 Update `src/FamilyFinances.Web/Features/Reports/MoneyFormatter.cs` to stop using `CultureInfo.InvariantCulture` for user-facing outputs and ensure culture-aware formatting for both supported languages.
- [x] 4.3 Update `src/FamilyFinances.Web/Features/Reports/DateHelper.cs` month-name paths so UI month names are culture-aware instead of invariant English-only output.
- [x] 4.4 Validate that `src/FamilyFinances.Web/Components/Pages/OpeningBalance/OpeningBalancePage.razor` and other touched pages do not retain conflicting mixed-culture display formatting.

## 5. Automated Tests for Localization Behavior

- [x] 5.1 Add/adjust unit tests in `tests/FamilyFinances.Web.Tests/Features/Reports/DateHelperTests.cs` and `tests/FamilyFinances.Web.Tests/Features/Reports/MoneyFormatterTests.cs` to assert deterministic formatting behavior under explicit `es-ES` and `en-US` cultures.
- [x] 5.2 Add tests in `tests/FamilyFinances.Web.Tests` for localization helper/service logic introduced by this change (for example, supported-culture normalization and fallback-to-default behavior).
- [x] 5.3 Update any existing web tests that assume previous hardcoded formatting text so expectations remain aligned with culture-driven output.

## 6. Validation, Build, and Documentation Checks

- [x] 6.1 Run `dotnet build src/FamilyFinances.Web/FamilyFinances.Web.csproj` and confirm clean compilation after localization and resource integration.
- [x] 6.2 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj` and confirm all updated localization/formatting tests pass.
- [ ] 6.3 Execute manual acceptance checks for both languages on `/`, `/transactions`, `/transactions/{id}`, `/accounts/{id}/movements`, and `/reports` verifying immediate switch, persistence across reload, and consistent date/currency formatting.
- [x] 6.4 Confirm no API/OpenAPI, database schema, or migration files were changed by this Web-only localization capability.
