## Context

`EconomicStatePage.razor` owns a global selected year and focused month, then passes both values to `AssetTotalEvolutionPanel`, `IncomeEvolutionPanel`, and `ExpenseEvolutionPanel` through `UseExternalFilters`, `SelectedYear`, and `FocusedMonth`. The panels already use the received focused month for their daily chart request and selected-month composition.

Their Monthly Overview table, highlighted row, current-period badge, and CSV use `GetDataUntilMonthForSelectedYear()`, which returns the system current month for the current year. Consequently, a user can select July while the table and export remain bounded by August. The Asset panel also uses the generic header `Balance` for a monthly asset delta. Snapshot instead displays `PeriodNetResultCents`, calculated from income and expense accounts. These measures intentionally differ when a transaction also affects Asset, Liability, or Equity accounts.

This is a Presentation-layer correction. Existing API endpoints, DTOs, account-nature aggregations, and the twelve-month evolution response remain valid and unchanged.

## Goals / Non-Goals

**Goals:**

- Give every visible element and CSV row in the three integrated Evolution overviews one shared selected-year/selected-month context.
- Preserve the full annual data returned by the evolution endpoint while rendering only months `1..FocusedMonth` in the integrated Economic State page.
- Make Asset Evolution's first monthly column explicitly describe asset movement and state that it is not the Snapshot period net result.
- Keep the interaction, dark-mode card/table styling, error handling, and external-filter parameter model already used by the page.

**Non-Goals:**

- No calculation, API, query, DTO, database, migration, or OpenAPI change.
- No financial reconciliation feature and no requirement that asset movement equal income plus expense.
- No changes to standalone panel calculations, report routes, or Reports index navigation. Existing controls on other report routes may receive the same wording-only export and period-clarity treatment.
- No broad visual redesign, chart rewrite, or introduction of a new shared component.

## Implementation Rules - Do Not Deviate

- Modify only the existing four Economic State components, shared resource files, focused Web UI tests, and this OpenSpec change.
- Continue passing `FocusedMonth` from `EconomicStatePage.razor`; do not create duplicate parent state or an additional API parameter.
- Use the existing `DateHelper.GetMonthName`, `ReportCsvBuilder`, `ReportExportInterop`, Bootstrap badges, and panel styling conventions.
- Do not rename or reinterpret `MonthlyEvolutionPointDto.DeltaVsPreviousMonthCents`, `EconomicStateDto.PeriodNetResultCents`, or `ReportingMetricSemantics` definitions.
- Retain the current-year focused-month clamp. A future month cannot be selected for the current year; a past year retains months 1 through 12.
- Add equivalent changes to Asset, Income, and Expense panels rather than allowing their table/export behavior to drift.

## Decisions

### Decision 1: The focused month is the integrated Overview cutoff

When `UseExternalFilters` is true, the Monthly Overview visible month sequence will be `Enumerable.Range(1, _focusedMonth)`. Its table rows, selected/highlighted row, header badge, and exported rows will derive from this same sequence. The change occurs after data retrieval: the panels continue requesting the annual evolution series, so no endpoint contract or year-to-date point semantics changes.

Alternative considered: add a focused-month parameter to the state-evolution API and truncate backend data. Rejected because it changes a stable twelve-bucket contract solely to control client presentation and would affect charts and consumers that still need annual data.

Alternative considered: retain all rows and only highlight the focused one. Rejected because the current defect is the overview claiming a period through the current system month after the user selected an earlier one.

### Decision 2: Context text reflects the selected period, not wall-clock current month

For externally filtered panels, the existing `Reports_CurrentMonth` badge and `Reports_Current` marker will be replaced or conditionally rendered with localized selected-period wording that identifies `YYYY-MM` / month name from `_selectedYear` and `_focusedMonth`. The badge must not say “Current month” for a non-current selection.

Alternative considered: reuse the current-month resource with a substituted month name. Rejected because the wording itself asserts a false temporal meaning for historical selections.

### Decision 3: Asset movement is clarified, not reconciled

The Asset Evolution first monthly column will use a specific localized label such as `Asset movement`, rather than the ambiguous generic `Balance`. A localized information note adjacent to the Asset overview will state that the value is the month-over-month change in Asset-account balances and can differ from Snapshot `Income - Expense` when transactions affect Liability or Equity accounts.

The existing `AssetEvolution_StockMetricsHint` remains useful for the KPI/card family. The new note targets the exact comparison the table invites and must use the same stock-versus-flow vocabulary as `reporting-metric-semantics`.

Alternative considered: replace the asset movement with `PeriodNetResultCents`. Rejected because it would make an Asset Evolution report display a flow value and violate the separately tested stock/flow contract.

### Decision 4: Test behavior at the Web boundary

Focused bUnit tests will use a deterministic selected month earlier than the current month and assert rendered table month rows, selected-period wording, and the CSV's serialized rows/context. Tests will also assert the asset explanatory copy without asserting accidental equality between stock and flow values. Existing API/integration coverage already protects formulas and explicitly protects their non-equivalence; it will not be changed merely to duplicate UI coverage.

Alternative considered: only snapshot markup tests. Rejected because a stale CSV would leave the user-facing inconsistency unresolved.

### Decision 5: Reuse chart and export primitives for report-wide clarity

The existing table export operations remain CSV downloads and the existing chart export operations remain PNG downloads. Their visible labels state that file type consistently, while their existing detailed accessible names remain intact. This is copy-only and does not introduce a new export path or alter serialization.

Period badges use `MM-YYYY` for a selected month and `YYYY` for a selected year. Composition charts keep percentage values in the pie payload, but their side legend renders the corresponding EUR amount. Income and Expense composition derives each slice from `DeltaVsPreviousMonthCents` at the selected month; Asset composition keeps its existing balance-based semantics.

Alternative considered: leave each report's legacy export, badge, and legend wording untouched. Rejected because identical controls otherwise communicate different output and period meanings across the reporting surface.

## Detailed UI Flows And Component Reuse

1. User opens `/reports/economic-state`; `EconomicStatePage` initializes year and focused month, loads the snapshot, and gives panels those values when a tab is activated.
2. User changes the global focused month. The parent updates `_filterFocusedMonth`, applies it, reloads Snapshot at the selected month's valid as-of date, and renders the active evolution child with the new `FocusedMonth` parameter.
3. The active child runs `OnParametersSetAsync`, assigns/clamps `FocusedMonth` into `_focusedMonth`, reloads its annual evolution series and daily focused-month chart, and renders monthly rows only through `_focusedMonth`.
4. The overview badge and selected-row marker identify the same focused month. Export serializes the same `visibleMonths` collection, so no row beyond the visible UI period appears in the CSV.
5. In Asset Evolution, the user sees `Asset movement` and the explanatory note before comparing it with Snapshot. The note states that period net result uses income/expense flows, while asset movement uses asset-account balances; no extra action or navigation is required.

## Detailed Page Wireframe

```text
/reports/economic-state
┌──────────────── Year ────────────┬──────── Focused month ────────┐
│ 2026                             │ July                          │
└──────────────────────────────────┴───────────────────────────────┘
 [Snapshot] [Asset Evolution] [Income Evolution] [Expense Evolution]

Asset Evolution
┌ Monthly Overview ──────────────── Selected period: July 2026 ┐
│ Month     Asset movement     End balance     Delta vs year start│
│ January       …                  …                  …          │
│ …                                                        │
│ July          …                  …                  …          │
└────────────────────────────────────────────────────────────────┘
 Asset movement is a stock delta. It can differ from Snapshot
 Income - Expense when a transaction affects Liability or Equity.
```

## Component Reuse Matrix

| Element | Reuse / modification | Responsibility |
|---|---|---|
| `EconomicStatePage.razor` | Reuse unchanged parameter wiring | Own global year/focused-month state and passes applied filters to active panels. |
| `AssetTotalEvolutionPanel.razor` | Modify | Uses focused month for overview/export context; clarifies asset movement. |
| `IncomeEvolutionPanel.razor` | Modify | Uses focused month for overview/export context. |
| `ExpenseEvolutionPanel.razor` | Modify | Uses focused month for overview/export context. |
| `ReportsApi` / API controller | Reuse unchanged | Continue providing annual series and selected-month daily charts. |
| `DateHelper`, CSV builder, export interop | Reuse unchanged | Supply existing formatting and download behavior. |
| Shared `.resx` files | Modify additively | Provide localized selected-period, asset-movement, and explanation text. |
| Reusable annual chart components and report table controls | Modify wording only | State PNG/CSV output type consistently and keep period/legend presentation aligned. |
| `AnnualChartDatasetAdapter` | Modify | Produce selected-month Income and Expense movement composition without changing API data. |

## Critical Implementation Pattern

The three panels must centralize their displayed period before both table rendering and export. The exact helper name may follow local style, but its behavior is fixed:

```csharp
private IReadOnlyList<int> GetOverviewMonths()
{
    if (UseExternalFilters)
        return Enumerable.Range(1, _focusedMonth).ToList();

    var maxMonth = GetDataUntilMonthForSelectedYear() ?? 12;
    return Enumerable.Range(1, maxMonth).ToList();
}
```

The table, current/selected marker, header badge, and `ExportMonthlyOverviewCsvAsync` must all consume `GetOverviewMonths()`. This preserves standalone-panel behavior while correcting the integrated page.

## Critical UX Behaviors

- Selecting July in the current year renders January through July, even if the system date is August or later.
- Selected month is always the final visible row in externally filtered overviews and is the only row marked as the selected period.
- The daily chart remains month-focused and preserves its existing current-month day cutoff only when the selected year/month is truly the current period.
- For a historical year, selecting November renders January through November; the API still provides December but the overview and CSV do not display or export it.
- Empty or zero-value months remain visible within the selected range because the annual evolution contract carries points forward.
- Asset clarification is informational only: it does not hide values, alter colors, change signs, or claim a reconciliation.

## Risks / Trade-offs

- [Risk] Repeated panel code may be updated inconsistently. → Mitigation: use the same helper/behavioral pattern and parameterized regression coverage for all three panels.
- [Risk] A resource key could be added in the default resource but omitted from a supported culture. → Mitigation: add the keys in default, `en-US`, and `es-ES` resource files and assert visible localized text in Web tests.
- [Risk] A CSV could retain the old current-month row set while the table is corrected. → Mitigation: make export consume the same overview-month collection and assert its rows/context.
- [Risk] Users might infer that the new note proves their individual discrepancy. → Mitigation: describe the general accounting condition without diagnosing transactions not shown in the report.

## Migration Plan

1. Add localized selected-period, asset-movement, and semantic-explanation strings in all existing shared resources.
2. Update the three panels to derive overview rows, marker, badge, and export rows from the focused month when externally filtered.
3. Update the Asset panel's first-column header and render its clarification note.
4. Add/update focused bUnit tests, then run the affected Web test suite and `openspec validate economic-state-focused-overview-clarity --strict`.
5. Deploy as a patch release; no data migration, cache invalidation, or API rollout coordination is needed.

The report-wide clarity extension also verifies the chart-adapter selected-month delta behavior and removes an unnecessary timing delay from token-store tests that was causing an intermittent test failure.

Rollback is a source-only revert of the component/resource/test changes. The annual API response and historical data remain untouched.

## Open Questions

None. The selected-month cutoff, retained financial formulas, and patch release impact were confirmed during exploration.
