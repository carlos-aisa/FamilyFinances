## 1. Localized Reporting Context And Semantics

- [x] 1.1 Add additive shared-resource entries in `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Resources/SharedResource.resx`, `SharedResource.en-US.resx`, and `SharedResource.es-ES.resx` for a selected reporting period, the Asset Evolution `Asset movement` column, and the explanation that asset movement is a stock delta distinct from Snapshot `Income - Expense`; preserve existing resource-key naming and all supported-culture parity.
- [x] 1.2 Verify the new default, English, and Spanish strings do not describe a historical selected period as the current month and do not imply that asset movement and period net result must be equal.

## 2. Focused-Month Monthly Overview Behavior

- [x] 2.1 Update `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Reports/AssetTotalEvolutionPanel.razor` so its Monthly Overview derives rendered months, selected-row marker, header context, and `ExportMonthlyOverviewCsvAsync` rows from the global `_focusedMonth` whenever `UseExternalFilters` is true; retain existing standalone behavior when it is false.
- [x] 2.2 In the same Asset panel, replace the generic first-column `Reports_Balance` header with the new localized asset-movement label and render the localized stock-versus-flow clarification alongside the overview without changing `DeltaVsPreviousMonthCents`, color selection, or API calls.
- [x] 2.3 Update `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Reports/IncomeEvolutionPanel.razor` so its Monthly Overview table, selected-period marker/context, and `ExportMonthlyOverviewCsvAsync` use the same externally focused month cutoff; do not change its income chart query, annual series, sign handling, or standalone mode.
- [x] 2.4 Update `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Reports/ExpenseEvolutionPanel.razor` with the equivalent focused-month table, selected-period marker/context, and export cutoff; preserve existing expense colors, chart query, annual series, and standalone mode.
- [x] 2.5 Review `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` only to confirm the existing `SelectedYear` and `FocusedMonth` parameter wiring remains the single source of truth; do not add endpoint parameters, duplicate filter state, or a new navigation flow.

## 3. Focused Web UI Regression Coverage

- [x] 3.1 Extend `D:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/Reports/EconomicStatePageTests.cs` with deterministic Asset Evolution coverage that selects a month before the current month and asserts: months after the selection are absent from the overview, the final row/context identifies the selected month, and the daily chart still requests that month.
- [x] 3.2 Add equivalent parameterized or separate Income and Expense Evolution assertions in `EconomicStatePageTests.cs` for the same selected-month cutoff, ensuring all three tabs stay behaviorally aligned and neither displays a `Current month` label for the earlier selected period.
- [x] 3.3 Add focused CSV-export coverage using the existing report export test conventions (`D:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests/Features/Reports/Export/ReportCsvBuilderTests.cs` and/or the Economic State page JS-export seam) to verify that an overview export contains exactly the visible months through the selected month and includes selected-period context.
- [x] 3.4 Add Asset Evolution markup assertions that the first overview column uses the localized asset-movement wording and the clarification distinguishes Asset-account stock delta from Snapshot income-and-expense flow, without asserting numeric equality between those values.

## 4. Validation And Change Documentation

- [x] 4.1 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj --filter FullyQualifiedName~EconomicStatePageTests` and resolve any focused-report UI failures. Passed 7 tests on 2026-08-02 using an isolated output directory because the local web app and Visual Studio lock the default build output.
- [x] 4.2 Not applicable: focused export coverage is implemented through the Economic State page JS-export seam rather than `ReportCsvBuilderTests`.
- [x] 4.3 Run `dotnet test FamilyFinances.sln` to confirm the localization and Web UI changes do not regress the solution. Passed 751 tests on 2026-08-02.
- [x] 4.4 Run `openspec validate economic-state-focused-overview-clarity --strict` and confirm the proposal, design, delta specs, and completed task checklist continue to describe the implemented behavior; update these artifacts if implementation reveals a material scope or UX change. Passed on 2026-08-02 after documenting the report-wide clarity extension.

## 5. Report-Wide Clarity Consistency

- [x] 5.1 Label existing table exports as `Export CSV` and reusable chart exports as `Export PNG`, preserving their CSV and PNG implementations.
- [x] 5.2 Normalize monthly and annual period badges, shorten the requested annual-report copy, and correct affected Spanish `año` strings in all supported resource files.
- [x] 5.3 Keep percentages in composition pies while rendering EUR values in their legends; make Income and Expense composition use only the selected month's movement and cover the adapter behavior with a unit test.

## 6. Test Stability

- [x] 6.1 Remove the unnecessary scheduler-delay assumptions from `ApiTokenStoreTests` so token signaling is verified deterministically.
