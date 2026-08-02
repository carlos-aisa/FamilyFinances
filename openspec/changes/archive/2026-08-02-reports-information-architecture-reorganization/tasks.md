## 1. Reports Index Information Architecture

- [x] 1.1 Refactor `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` to render explicit analytical family sections (Financial Snapshot, Period Flow Analysis, Account Structure Analysis) while preserving existing premium card interaction pattern.
- [x] 1.2 Reorder report cards into deterministic family/group order and keep existing route targets unchanged for `economic-state`, `monthly-summary`, `category-totals`, `account-totals`, and `account-group-totals`.
- [x] 1.3 Keep `/reports/asset-total-balance` available as an existing deep link without duplicating its summary as a Financial Snapshot card; size report groups for two-card desktop presentation.

## 2. Naming And Microcopy Consistency

- [x] 2.1 Review reports index localization keys and text usage for title/description/badge consistency; resolve ambiguous wording for monthly summary entry intent.
- [x] 2.2 Add or update required localization resource entries used by the new grouped family headers and revised report card copy in all supported UI languages.
- [x] 2.3 Verify fallback rendering behavior when localization keys are missing to avoid runtime UX regressions.

## 3. UI Behavior And Accessibility Safeguards

- [x] 3.1 Ensure grouped layout preserves keyboard and pointer interaction semantics for report cards and does not introduce hidden navigation layers.
- [x] 3.2 Ensure visual hierarchy improvements do not alter authorization gating behavior on `/reports`.
- [x] 3.3 Validate deterministic render ordering for sections/cards to avoid non-deterministic UI behavior across refreshes.

## 4. Automated Tests

- [x] 4.1 Update or add reports index UI tests under `tests/FamilyFinances.Web.Tests/Features/Reports/` to assert grouped family rendering and section ordering.
- [x] 4.2 Update or add tests to assert that `/reports/asset-total-balance` is intentionally absent from the reports index while the five primary report cards retain their navigation behavior.
- [x] 4.3 Update or add tests to verify existing report card destinations remain unchanged after reorganization.

## 5. Documentation And Change Notes

- [x] 5.1 Add release/implementation note documenting reports IA reorganization rationale, new grouped model, and discoverability improvements.
- [x] 5.2 Confirm OpenSpec change artifacts remain synchronized with final implemented behavior if additional UX adjustments appear during implementation.

## 6. Validation

- [x] 6.1 Run focused web UI tests covering reports index and report navigation behavior. Passed `ReportsIndexPageTests` (15 tests) on 2026-08-02, including intentional absence of asset total balance and two-column desktop sizing.
- [x] 6.2 Run broader solution validation (`dotnet test`) if focused tests expose potential cross-feature regressions. An isolated-output run on 2026-08-02 passed 738 tests; 8 repository-location-dependent tests failed only because the isolated output path was required to avoid DLL locks held by the local web app and Visual Studio. The default-output suite had passed 746 tests before this UI revision.
- [x] 6.3 Run `openspec validate reports-information-architecture-reorganization --strict` and confirm change is apply-ready. Passed on 2026-08-02.
