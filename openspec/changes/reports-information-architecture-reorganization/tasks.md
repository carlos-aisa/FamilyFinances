## 1. Reports Index Information Architecture

- [ ] 1.1 Refactor `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor` to render explicit analytical family sections (Financial Snapshot, Period Flow Analysis, Account Structure Analysis) while preserving existing premium card interaction pattern.
- [ ] 1.2 Reorder report cards into deterministic family/group order and keep existing route targets unchanged for `economic-state`, `monthly-summary`, `category-totals`, `account-totals`, and `account-group-totals`.
- [ ] 1.3 Add missing discoverability card for `/reports/asset-total-balance` in the Financial Snapshot family with the same click-navigation behavior used by other report cards.

## 2. Naming And Microcopy Consistency

- [ ] 2.1 Review reports index localization keys and text usage for title/description/badge consistency; resolve ambiguous wording for monthly summary entry intent.
- [ ] 2.2 Add or update required localization resource entries used by the new grouped family headers and revised report card copy in all supported UI languages.
- [ ] 2.3 Verify fallback rendering behavior when localization keys are missing to avoid runtime UX regressions.

## 3. UI Behavior And Accessibility Safeguards

- [ ] 3.1 Ensure grouped layout preserves keyboard and pointer interaction semantics for report cards and does not introduce hidden navigation layers.
- [ ] 3.2 Ensure visual hierarchy improvements do not alter authorization gating behavior on `/reports`.
- [ ] 3.3 Validate deterministic render ordering for sections/cards to avoid non-deterministic UI behavior across refreshes.

## 4. Automated Tests

- [ ] 4.1 Update or add reports index UI tests under `tests/FamilyFinances.Web.Tests/Features/Reports/` to assert grouped family rendering and section ordering.
- [ ] 4.2 Update or add tests to assert direct discoverability and navigation behavior for `/reports/asset-total-balance` from the reports index.
- [ ] 4.3 Update or add tests to verify existing report card destinations remain unchanged after reorganization.

## 5. Documentation And Change Notes

- [ ] 5.1 Add release/implementation note documenting reports IA reorganization rationale, new grouped model, and discoverability improvements.
- [ ] 5.2 Confirm OpenSpec change artifacts remain synchronized with final implemented behavior if additional UX adjustments appear during implementation.

## 6. Validation

- [ ] 6.1 Run focused web UI tests covering reports index and report navigation behavior.
- [ ] 6.2 Run broader solution validation (`dotnet test`) if focused tests expose potential cross-feature regressions.
- [ ] 6.3 Run `openspec validate reports-information-architecture-reorganization --strict` and confirm change is apply-ready.
