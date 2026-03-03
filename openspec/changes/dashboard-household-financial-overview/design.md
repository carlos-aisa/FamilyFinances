## Context

Dashboard and reporting improvements were previously split across two draft changes:
- `dashboard-reports-hub-quick-entry-separation` (Dashboard vs Quick Entry IA)
- `reports-and-accounts-presentation-refresh` (Accounts + Reports analytical improvements)

This change unifies both into one coherent design:
- Dashboard becomes a household financial cockpit (at-a-glance analytics)
- Quick transaction capture moves to a dedicated `Quick Entry` workspace
- Reports remain the deep-dive path through navigation, not through dashboard shortcut cards
- Accounts and report pages receive the presentation/behavior updates already requested

Current implementation baseline:
- Dashboard page: `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor`
- Reporting pages: `src/FamilyFinances.Web/Components/Pages/Reports/*`
- Accounts page: `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`
- Navigation: `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`
- Shared chart JS/theme: `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`, premium CSS token layer

Constraints and stakeholder expectations:
- Financial semantics remain unchanged (`Net = Income - Expense` positive when income > expense)
- Dashboard must avoid tabs and avoid report shortcut cards
- Dashboard should minimize vertical scroll on 2560x1440 desktop (current baseline target)
- Language selector remains in Settings only
- Current data may be sparse (for example only 2 months), but UI should be ready for year-over-year comparisons once data exists

Primary users:
- Household operator using Dashboard daily for quick status checks
- User needing quick capture flow without dashboard clutter
- User opening report pages only for deep detail

## Goals / Non-Goals

**Goals:**
- Deliver a no-tabs dashboard financial overview focused on current month vs previous month status.
- Separate quick transaction capture into `/quick-entry` while preserving existing behavior semantics.
- Add dual balance display in Accounts list: accumulated + selected-period (current month for this iteration).
- Add/adjust report surfaces:
  - Economic State: new Expense evolution tab
  - Economic State summary: monthly net list + month-focused chart + annual Income vs Expense bars
  - Account Totals: default net-change order + header-based sorting
  - Account Group State Evolution: selected-month exact balance in list, annual non-cumulative bars, improved chart/list width allocation
- Ensure clear insufficient-history states and future-ready year-over-year comparison hooks.

**Non-Goals:**
- No change to accounting formulas, posting logic, or reporting metric semantics.
- No removal/rename of existing `/reports/*` detail routes.
- No report shortcut cards on Dashboard.
- No tabbed containers on Dashboard.
- No professional forecasting/ML models.
- No server-side user preference persistence changes in this iteration.

## IMPLEMENTATION RULES - DO NOT DEVIATE

### MUST
- Keep layered architecture boundaries (Presentation -> Application -> Domain; Infrastructure only where required).
- Keep Dashboard analytics-first and no-tabs.
- Move quick-entry interaction workload off dashboard into dedicated route/component host.
- Keep Reports menu as the deep-dive access path.
- Use shared styling primitives/tokens for any new dashboard/report blocks.
- Keep selected month semantics deterministic across dashboard and report panels.
- Keep test IDs stable where already present.
- Add tests for any changed interaction or rendering contract.
- Preserve localization behavior and update resources for any new labels.

### MUST NOT
- Must not add language selector controls to nav.
- Must not introduce dashboard report-link cards.
- Must not duplicate business calculations between UI and API if reporting services already provide contractable values.
- Must not degrade existing report endpoint compatibility.
- Must not hardcode one-off styles bypassing shared premium tokens.

## DETAILED UI FLOWS

### Flow 1: Dashboard initial load (analytics-first)
1. User opens `/`.
2. App resolves current comparison context: selected/current month plus previous month.
3. Dashboard loads compact KPI strip (Income, Expense, Net Result, Net Worth) with delta vs previous month.
4. Dashboard loads analytical blocks:
   - month-focused Income vs Expense line chart,
   - annual Income vs Expense month-result bar chart (absolute values for comparability),
   - monthly net-balance line chart (`Income - Expense`),
   - account-group current-state chart,
   - account-group composition chart,
   - expense composition pie chart (Top-N + Others, where N=8..10).
5. If history is insufficient, dashboard displays explicit state text and still preserves block layout footprint.

Expected:
- No tabs.
- No report shortcut cards.
- No vertical scroll in baseline 2560x1440 desktop viewport.

### Flow 2: Quick transaction capture
1. User navigates to `/quick-entry` from main nav.
2. Existing quick-entry flows (expense/income/transfer/refund + widgets) are available there.
3. On submit, behavior remains same as current capture flow semantics.
4. User can return to dashboard for high-level analysis.

Expected:
- Capture logic unchanged.
- Dashboard remains uncluttered.

### Flow 3: Accounts dual-balance reading
1. User opens Accounts list.
2. Each row shows accumulated balance and selected-period balance (current month in this phase).
3. Balance badges/styles remain semantically clear (positive/negative/neutral).

Expected:
- Both balance perspectives visible without entering detail reports.

### Flow 4: Economic State with expense parity
1. User opens `/reports/economic-state`.
2. Tabs now include Snapshot, Asset Evolution, Income Evolution, Expense Evolution.
3. Snapshot summary includes monthly net list, month-focused Income vs Expense chart, and annual Income vs Expense bars.
4. Any missing history shows clear informative empty/partial state.

Expected:
- Semantics unchanged, visualization improved.

### Flow 5: Account Totals sorting
1. User opens Account Totals period tab.
2. Rows are initially ordered by net change within each nature group.
3. User clicks table headers to change sort column/direction.

Expected:
- Sorting is deterministic and keyboard/click accessible.

### Flow 6: Account Group State Evolution
1. User opens Account Group State Evolution.
2. List shows exact selected-month balance column.
3. Annual evolution chart uses month-result bars (not cumulative lines).
4. Layout allocates more horizontal space to chart while preserving list readability.
5. Comparability info card is removed or relocated out of critical analysis area.

Expected:
- Better readability and stronger month-result interpretation.

### Flow 7: Year-over-year readiness with sparse data
1. Dashboard/report component asks for same-month-last-year values.
2. If unavailable, UI marks state as `insufficient_history`.
3. Once data exists, the same block automatically starts showing YoY delta.

Expected:
- No layout jumps.
- No misleading zero defaults.

## DETAILED PAGE WIREFRAMES

### Dashboard (desktop target 2560x1440, no tabs)

```text
+--------------------------------------------------------------------------------------------------------------+
| KPI Strip: [Income] [Expense] [Net Result] [Net Worth] (each with delta vs previous month)                 |
+--------------------------------------------------------------------------------------------------------------+
| Left (50%)                                           | Right (50%)                                    |
|------------------------------------------------------+-----------------------------------------------|
| Month-focused Income vs Expense (line)               | Annual Income vs Expense (month-result bars)   |
| fixed-height chart                                    | fixed-height chart (absolute values)          |
+--------------------------------------------------------------------------------------------------------------+
| Left (50%)                                           | Right (50%)                                    |
|------------------------------------------------------+-----------------------------------------------|
| Monthly Net Balance Trend (Income - Expense, line)   | Account Group Current State (bars)             |
| fixed-height chart                                    | fixed-height chart                            |
+--------------------------------------------------------------------------------------------------------------+
| Left (50%)                                           | Right (50%)                                    |
|------------------------------------------------------+-----------------------------------------------|
| Expense composition (pie: Top-N + Others)            | Account Group Composition (pie/donut)          |
| fixed-height chart                                    | fixed-height chart                            |
+--------------------------------------------------------------------------------------------------------------+
```

### Quick Entry workspace

```text
+--------------------------------------------------------------------------------------------------+
| Quick Entry Header                                                                               |
+--------------------------------------------------------------------------------------------------+
| Capture cards/widgets currently hosted on old dashboard (expense/income/transfer/refund, etc.) |
| Interaction semantics unchanged                                                                   |
+--------------------------------------------------------------------------------------------------+
```

### Accounts dual-balance row

```text
| Account Name | Nature | Accumulated Balance | Current-Month Balance | Status |
```

### Economic State summary area

```text
+----------------------------------------------------------------------------------------------+
| Monthly Net List (Income - Expense) | Month-focused Income vs Expense | Annual I/E bars    |
+----------------------------------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Quick-entry interaction components | Existing quick-entry cards/drawers/widgets | Rehost route and container placement | Optional `QuickEntryPage.razor` route host |
| Dashboard layout shell | Existing dashboard page route | Replace composition with analytics-first chart grid blocks | Optional dashboard section components |
| Reporting data endpoints | Existing reporting APIs and DTO contracts | Add/extend mappings for month-focused, annual bars, net trend, and Top-N+Others datasets | Optional aggregate DTO for dashboard snapshot |
| Accounts list page | Existing Accounts page and row rendering | Add period-balance column and formatting | None expected |
| Economic State page | Existing tabs and panels | Add Expense tab and summary content blocks | Optional panel component for monthly net list |
| Annual/monthly chart components | Existing chart wrappers and JS | Add bar variants/non-cumulative semantics where required | Optional reusable annual bar component |
| Account totals/group totals tables | Existing table markup and APIs | Add default sorting and click-header sorting behavior | Optional shared sortable-header helper |
| Navigation | Existing nav menu | Add `Quick Entry` destination, keep Reports and Settings policies | None expected |
| Localization | Existing resource system | Add labels/messages for new blocks/states | None |

## Decisions

### Decision 1: Dashboard is analytics-first, not report-links-first
- **Choice:** Dashboard shows only analytical status blocks; no report shortcut cards.
- **Rationale:** Avoid duplicate navigation concepts and reduce cognitive noise.
- **Alternative:** Use dashboard as report launcher.
- **Rejected because:** Reports are already reachable from main menu and user explicitly requested otherwise.

### Decision 2: No tabs on dashboard
- **Choice:** Fixed multi-row layout with distinct blocks.
- **Rationale:** Better scanability and less interaction overhead for at-a-glance checks.
- **Alternative:** Tabs to save space.
- **Rejected because:** User preference and extra interaction cost.

### Decision 3: Quick Entry split into dedicated route
- **Choice:** Move quick capture workflows to `/quick-entry`.
- **Rationale:** Separation of concerns (capture vs analysis).
- **Alternative:** Keep quick-entry on dashboard.
- **Rejected because:** Competes with analytical overview and increases clutter.

### Decision 4: Accounts must show two balance lenses
- **Choice:** Show accumulated and current-month balance in same list row.
- **Rationale:** Supports operational and period analysis without report navigation.
- **Alternative:** Keep only accumulated and require report deep dive.
- **Rejected because:** Adds unnecessary clicks for common check.

### Decision 5: Economic State expense parity
- **Choice:** Add Expense evolution tab, symmetric to asset/income evolution structure.
- **Rationale:** Completes core flow visibility.
- **Alternative:** Keep expense only in snapshot values.
- **Rejected because:** Incomplete trend perspective.

### Decision 6: Annual evolution must be bar-based month results where requested
- **Choice:** Replace cumulative line behavior with month-result bars in specified contexts.
- **Rationale:** Better interpretation for period-over-period operational comparison.
- **Alternative:** Keep cumulative line charts.
- **Rejected because:** Obscures month-level signal in requested panels.

### Decision 7: Sorting must be explicit and user-driven in totals
- **Choice:** Default by net change + click-header sorting.
- **Rationale:** Users can pivot analysis quickly.
- **Alternative:** Static ordering.
- **Rejected because:** Lower analytical utility.

### Decision 8: Data-sufficiency states are first-class
- **Choice:** Show explicit `insufficient history` and `partial history` messages.
- **Rationale:** Prevent false interpretation with sparse datasets.
- **Alternative:** Show zeros/silent empty chart.
- **Rejected because:** Misleading and poor UX.

### Decision 9: Keep endpoint compatibility
- **Choice:** Reuse/extend existing reporting contracts cautiously.
- **Rationale:** Minimize breaking risk for current pages/tests.
- **Alternative:** Introduce entirely new endpoint family.
- **Rejected because:** High integration churn for limited value.

### Decision 10: Desktop no-scroll target is constrained, not universal
- **Choice:** Target 2560x1440 for no-scroll baseline dashboard glanceability.
- **Rationale:** Realistic and testable acceptance criteria.
- **Alternative:** strict no-scroll for all breakpoints.
- **Rejected because:** infeasible and harmful on small screens.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Dashboard structure contract (no tabs)

```razor
<div class="ff-dashboard-overview">
  <DashboardKpiStrip Metrics="_kpis" />

  <div class="row g-3 ff-dashboard-row-2">
    <div class="col-12 col-xxl-6">
      <MonthlyIncomeExpenseChart ... />
    </div>
    <div class="col-12 col-xxl-6">
      <AnnualIncomeExpenseBarsChart ... />
    </div>
  </div>

  <div class="row g-3 ff-dashboard-row-3">
    <div class="col-12 col-xxl-6">
      <MonthlyNetBalanceLineChart ... />
    </div>
    <div class="col-12 col-xxl-6">
      <AccountGroupStateChart ... />
    </div>
  </div>

  <div class="row g-3 ff-dashboard-row-4">
    <div class="col-12 col-xxl-6">
      <ExpenseCompositionChartTopNWithOthers ... />
    </div>
    <div class="col-12 col-xxl-6">
      <AccountGroupCompositionChart ... />
    </div>
  </div>
</div>
```

### Example 2: Dashboard comparison DTO shape

```csharp
public sealed record DashboardOverviewDto(
    DateOnly SelectedMonth,
    DateOnly PreviousMonth,
    long IncomeCents,
    long ExpenseCents,
    long NetResultCents,
    long NetWorthCents,
    long IncomeDeltaVsPreviousCents,
    long ExpenseDeltaVsPreviousCents,
    long NetResultDeltaVsPreviousCents,
    long NetWorthDeltaVsPreviousCents,
    IReadOnlyList<GroupStatePointDto> GroupStates,
    IReadOnlyList<GroupCompositionPointDto> GroupComposition,
    IReadOnlyList<MonthlyIncomeExpensePointDto> MonthlyIncomeExpense,
    IReadOnlyList<MonthlyIncomeExpensePointDto> AnnualIncomeExpenseByMonth,
    IReadOnlyList<MonthlyNetPointDto> MonthlyNetBalanceTrend,
    IReadOnlyList<ExpenseCompositionPointDto> ExpenseTopNWithOthers,
    DataSufficiencyState DataState);
```

### Example 3: Accounts dual-balance row rendering

```razor
<td class="text-end">@MoneyFormatter.FormatCents(account.AccumulatedBalanceCents)</td>
<td class="text-end">@MoneyFormatter.FormatCents(account.SelectedPeriodBalanceCents)</td>
```

### Example 4: Sortable header pattern for totals table

```razor
<th role="button" @onclick="() => ToggleSort(SortColumn.NetChange)">
  @L["AccountTotals_NetChange"]
  <SortGlyph Column="SortColumn.NetChange" ActiveColumn="_sort.Column" Direction="_sort.Direction" />
</th>
```

### Example 5: Insufficient-history state contract

```csharp
public enum DataSufficiencyState
{
    Complete,
    Partial,
    InsufficientHistory
}
```

### Example 6: Year-over-year guard in view model assembly

```csharp
var yoy = hasSameMonthLastYear
    ? currentValue - lastYearValue
    : (long?)null;

var state = hasSameMonthLastYear
    ? DataSufficiencyState.Complete
    : DataSufficiencyState.InsufficientHistory;
```

## Risks / Trade-offs

- [Risk] Dashboard becomes too dense and noisy.
  -> Mitigation: fixed block hierarchy, strict row caps, priority ordering, semantic typography.

- [Risk] Quick-entry relocation may initially confuse habitual users.
  -> Mitigation: stable nav label, migration hint text on dashboard during transition window.

- [Risk] Added sorting and chart changes create test fragility.
  -> Mitigation: preserve data-test IDs and add deterministic interaction tests.

- [Risk] Sparse data can make charts look "empty" or broken.
  -> Mitigation: explicit data-sufficiency states and placeholder guidance.

- [Risk] Backend query extension for expense evolution introduces performance regressions.
  -> Mitigation: reuse existing aggregations where possible; profile monthly range queries.

- [Risk] Non-cumulative bars may conflict with previous user mental model.
  -> Mitigation: clear labels/subtitles indicating month result semantics.

- [Risk] 2560x1440 no-scroll target may be brittle under localization expansion.
  -> Mitigation: concise labels and controlled card heights with overflow-safe rules.

- [Trade-off] Reusing report endpoints reduces backend churn but can produce slightly coupled dashboard contracts.
  -> Mitigation: isolate transformation in Application layer adapters.

- [Trade-off] Strict no-tabs improves glanceability but limits optional drill controls inline.
  -> Mitigation: keep reports for deep dive, dashboard for summary.

## Migration Plan

### Phase 1: Routing and IA boundary
1. Add `/quick-entry` route host and move existing quick-entry composition there.
2. Update nav to include Quick Entry and keep Reports/Settings policies.
3. Remove quick-entry workload from dashboard composition.

### Phase 2: Dashboard analytics composition
4. Implement dashboard KPI strip and fixed block layout (no tabs, no shortcut cards).
5. Integrate Option 1 blocks: month-focused line, annual bars, monthly net trend, group state, group composition, expense Top-N+Others.
6. Implement data-sufficiency UI states.

### Phase 3: Accounts + Reports presentation deltas
7. Add dual-balance columns in Accounts list.
8. Add Expense evolution tab in Economic State.
9. Implement Economic State summary additions (monthly net list + annual I/E bars).
10. Update Account Totals sorting behavior.
11. Update Account Group State Evolution list/chart/info-card behavior.

### Phase 4: Validation and docs
12. Update localization resources.
13. Update/add Web and API tests for new contracts and interactions.
14. Update docs with dashboard intent and quick-entry separation.

### Rollback Strategy
- Revert by boundary:
  - first dashboard composition,
  - then quick-entry route move,
  - then accounts/reports presentation deltas.
- Keep `/reports/*` endpoints/routes stable during rollback.
- Re-run affected suites after each rollback boundary:
  - dashboard/layout tests,
  - reports tests,
  - reporting API integration tests.

## Open Questions

- Should expense composition Top-N default to 8 or 10 when the user has no explicit preference configured?
- Should selected month in dashboard be always current month for now, or user-selectable from dashboard header in this iteration?
- For Accounts period balance, should period label be displayed inline (`Current month`) to avoid ambiguity?
- For Economic State annual bars, should bars be grouped (Income/Expense side-by-side) or stacked with net overlay?
- Should migrated quick-entry page keep exactly the previous visual order of cards/widgets, or can order be optimized?
- Is a temporary one-release onboarding hint needed after moving quick-entry away from dashboard?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Architecture and scope
- [ ] Dashboard contains no report shortcut cards.
- [ ] Dashboard contains no tab controls.
- [ ] Quick-entry workflows are hosted outside `/`.
- [ ] `/reports/*` routes remain unchanged.
- [ ] Layer boundaries are respected.
- [ ] No accounting semantics changed.

### Navigation and IA
- [ ] Main nav exposes `Quick Entry`.
- [ ] Main nav keeps `Reports` as deep-dive entry.
- [ ] Main nav does not expose language selector.
- [ ] Settings still hosts language controls.
- [ ] Dashboard route remains `/`.
- [ ] Quick Entry route resolves without auth regressions.

### Dashboard KPI strip
- [ ] KPI cards render Income.
- [ ] KPI cards render Expense.
- [ ] KPI cards render Net Result (`Income - Expense`).
- [ ] KPI cards render Net Worth.
- [ ] Each KPI shows delta vs previous month.
- [ ] Delta sign rendering is correct for positive/negative.

### Dashboard chart blocks
- [ ] Monthly Income vs Expense chart renders.
- [ ] Annual Income vs Expense bars render with month buckets Jan-Dec.
- [ ] Monthly net-balance trend chart renders as `Income - Expense`.
- [ ] Account-group state chart renders.
- [ ] Account-group composition chart renders.
- [ ] Expense composition chart renders Top-N + Others.
- [ ] Block titles and subtitles are localized.

### Dashboard layout contract
- [ ] Desktop (2560x1440) shows KPI + primary analytical rows without vertical scroll.
- [ ] Block heights are consistent with design contract.
- [ ] No tabs are used to hide core dashboard blocks.
- [ ] No report shortcut cards are introduced.
- [ ] Mobile/tablet breakpoints remain usable with expected vertical stacking.
- [ ] Focus order remains keyboard-friendly.

### Data sufficiency and YoY readiness
- [ ] `insufficient_history` state appears when data is missing.
- [ ] `partial_history` state appears when only partial comparison exists.
- [ ] Complete state appears when same-month-last-year data exists.
- [ ] No zero-value fallback is used to fake missing YoY data.
- [ ] Placeholder messages are localized.
- [ ] Layout remains stable across state transitions.

### Quick Entry workspace
- [ ] Expense capture works from new route.
- [ ] Income capture works from new route.
- [ ] Transfer capture works from new route.
- [ ] Refund capture works from new route.
- [ ] Existing widget behavior remains unchanged.
- [ ] Post-submit behavior matches previous baseline.

### Accounts dual-balance view
- [ ] Accumulated balance column remains present.
- [ ] Current-month period balance column is present.
- [ ] Period balance semantics match selected/current month contract.
- [ ] Positive/negative formatting remains consistent.
- [ ] Table remains readable in dark mode.
- [ ] Column headers are localized.

### Economic State changes
- [ ] Expense evolution tab exists and is selectable.
- [ ] Expense tab data loads deterministically.
- [ ] Monthly net list is displayed as `Income - Expense`.
- [ ] Month-focused Income vs Expense chart remains present.
- [ ] Annual Income vs Expense bar chart renders.
- [ ] Snapshot semantics remain unchanged.

### Account Totals sorting
- [ ] Default sort is net change inside each nature group.
- [ ] Header click toggles sort direction.
- [ ] Sort indicators update correctly.
- [ ] Sorting remains deterministic with equal values.
- [ ] Keyboard accessibility for sortable headers is verified.
- [ ] Export behavior still works with sorted view.

### Account Group State Evolution
- [ ] Selected-month exact balance appears in list.
- [ ] Annual chart uses month-result bars.
- [ ] Annual chart is non-cumulative in this context.
- [ ] List/chart width rebalance is applied.
- [ ] Comparability info card is removed or relocated.
- [ ] Labels explain bar semantics clearly.

### Testing and quality
- [ ] Dashboard UI tests updated for new composition.
- [ ] Navigation tests updated for quick-entry route.
- [ ] Accounts tests updated for dual-balance columns.
- [ ] Economic State tests updated for expense tab and summary blocks.
- [ ] Totals/evolution tests updated for sorting and bar semantics.
- [ ] Reporting API integration tests remain green.

### Documentation and operations
- [ ] Dashboard intent is documented as analytics-first.
- [ ] Quick-entry separation is documented.
- [ ] Data-sufficiency behavior is documented.
- [ ] Rollback boundaries are documented.
- [ ] Localization resource updates are documented.
- [ ] Release notes mention IA and report/account presentation changes.
