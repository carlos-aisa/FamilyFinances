## Context

This change applies a complete UX and presentation review across the Blazor Web application while preserving current business calculations and architecture boundaries. The scope is intentionally broad at the UI layer and includes:

- Dashboard copy and presentation consistency.
- Accounts and Quick Entry interaction model alignment (accordion behavior, discoverability, contextual guidance).
- Navigation continuity for transaction detail/edit flows.
- Payees, transactions, and report table presentation updates.
- Economic State evolution layout and chart consistency updates.
- Login UX enhancement to remember the last successful username.

The codebase already contains reusable chart components (`MonthlyLineChart`, `AnnualLineChart`, `AnnualBarChart`, `AnnualCompositionChart`), a shared date preset component (`DateRangePresets`), and a dark-theme aware style layer (`app.css` + `premium-theme.css`). The design below reuses these building blocks and introduces only minimal new shared UI contracts where reuse is currently missing.

This design explicitly excludes the two parallel changes already split out:

- `global-filter-behavior-semantics` for end-date inclusivity and global filter auto-apply/reset semantics.
- `ui-hardcode-normalization` for app-wide tokenization and hardcoded value removal.

## Goals / Non-Goals

**Goals:**

- Standardize visual behavior and interaction patterns across all main views without breaking current report calculations.
- Make monthly evolution charts and annual evolution charts visually consistent with a clearly marked current-period cutoff.
- Ensure users always return to the origin context when navigating transaction detail/edit flows.
- Improve dense list usability (accounts and payees) with clearer hierarchy and faster selection/search.
- Add requested report drilldowns and sorting where user workflows currently stop at summary rows.
- Keep localization, tests, and UX copy aligned in `es-ES` and `en-US`.

**Non-Goals:**

- No global migration from exclusive to inclusive end date semantics.
- No app-wide removal of Apply/Reset filters behavior across all pages.
- No token/governance cleanup of all hardcoded UI values (reserved for change 3).
- No change to financial formulas or report metric definitions.
- No authentication architecture changes beyond last-username memory in login UX.

## IMPLEMENTATION RULES - DO NOT DEVIATE

### ? Forbidden

- Do not implement global date semantic migration (`to` inclusive) in this change.
- Do not remove all apply/reset actions globally; only apply page-scoped behavior documented here.
- Do not introduce open redirects by accepting arbitrary return URLs in query string.
- Do not bypass current layered architecture or duplicate chart rendering logic outside existing chart components/JS.
- Do not hardcode user-facing Spanish/English strings in Razor files; use resource keys.
- Do not alter backend report formulas or DTO meaning.
- Do not break existing API contracts; additive DTO fields are allowed only when required.
- Do not introduce new UI framework dependencies.

### ? Required

- Keep all new user-facing copy in resource files (`SharedResource*.resx`).
- Keep dashboard/report visual conventions aligned (`ff-panel`, `ff-data-table`, `ff-chart-panel`, consistent export button shape).
- Implement origin-aware navigation as a whitelisted context contract.
- Keep accordion behavior single-open where explicitly requested.
- Keep all chart cutoffs visually explicit for current-month-to-future sections.
- Ensure all evolution-tab monthly tables show only visible months up to the current month when year is current year.
- Aggregate pie/composition slices beyond top 10 into an `Others` slice.
- Update and add web tests for every behavior changed in this design.

## Decisions

### Decision 1: Use a whitelisted transaction-origin context instead of free-form return URLs

- Chosen approach: introduce a small navigation context contract with known origin values (for example `transactions`, `accounts-movements`, `history-transactions`, `history-movements`, `report-category-totals`, `report-account-totals`) and optional typed parameters (`accountId`, `from`, `to`, `year`).
- Why: fixes "Back goes to `/transactions`" regressions without security risk from arbitrary URL redirects.
- Alternatives considered:
  - Free-form `returnUrl` string: rejected due to open-redirect and validation complexity.
  - Browser `history.back()`: rejected because it is unreliable after reload/deep-link.

### Decision 2: Reuse Bootstrap accordion primitives through a shared single-open wrapper

- Chosen approach: add one reusable shared accordion wrapper for "single open section" and reuse it in Accounts and Quick Entry account side panel.
- Why: avoids repeating ad-hoc collapse ids and `data-bs-parent` wiring in multiple pages.
- Alternatives considered:
  - Keep page-local accordion code in each page: rejected due to duplicated behavior and harder maintenance.

### Decision 3: Keep report drilldowns anchored on account movement route

- Chosen approach: on Category Totals and Account Totals row click, navigate to `/accounts/{accountId}/movements` with optional filter query context.
- Why: route already exists, aligns with user mental model ("open account movements"), and requires no new report-detail page.
- Alternatives considered:
  - Open `/transactions` with search prefilled: rejected because it loses account-specific running balance context.

### Decision 4: Standardize chart cutoff UX in chart components, not per-page

- Chosen approach: extend chart payloads/components so both monthly and annual charts support a visual current-period cutoff marker + future shaded area.
- Why: one implementation path in `reportCharts.js` ensures consistency across dashboard and reports.
- Alternatives considered:
  - Per-page overlays with CSS: rejected due to duplication and mismatch across chart types.

### Decision 5: Build Economic State evolution composition from account evolution dataset

- Chosen approach: for Asset/Income/Expense evolution tabs, fetch account evolution series for the selected year and derive month-focused composition by nature with top-10 + `Others`.
- Why: avoids new backend endpoint and reuses existing `AnnualChartDatasetAdapter` composition helpers.
- Alternatives considered:
  - New backend endpoint per nature/month: rejected for this change due to broader API surface and no functional need.

### Decision 6: Preserve current global filter semantics while removing only page-specific load buttons where requested

- Chosen approach: remove explicit "Load report" buttons from Economic State global filters and trigger reload on year/month change there; keep other pages under current semantics unless explicitly changed in this design.
- Why: honors user request while respecting split with `global-filter-behavior-semantics`.
- Alternatives considered:
  - Remove all load buttons globally now: rejected (belongs to change 2).

### Decision 7: Keep login username memory in browser local storage

- Chosen approach: store last successful username in local storage and prefill login input on first render.
- Why: simple UX gain with minimal risk and no backend change.
- Alternatives considered:
  - Server-side persistence: rejected as unnecessary for this requirement.
## DETAILED UI FLOWS

### Flow 1: Dashboard subtitle wording and month context

1. User opens `/`.
2. Subtitle shows "current month" wording (localized), replacing "selected month".
3. No month selector is shown in dashboard in this change.
4. KPI/cards/charts remain unchanged functionally.

### Flow 2: Accounts page single-open accordion and page-level "updated as of"

1. User opens `/accounts`.
2. Nature groups render as accordion sections.
3. First section is open by default; opening another closes previous one.
4. Per-section footer text "current month basis" is removed.
5. Page-level description displays "Accounts updated as of {currentDate}".

### Flow 3: Account movements -> transaction detail -> edit -> back to origin

1. User opens `/accounts/{accountId}/movements`.
2. User clicks a movement row.
3. App navigates to `/transactions/{transactionId}?origin=accounts-movements&accountId={accountId}`.
4. User clicks Edit.
5. App navigates to `/transactions/{transactionId}/edit?origin=accounts-movements&accountId={accountId}`.
6. User clicks Back in edit page.
7. App returns to `/transactions/{transactionId}?origin=accounts-movements&accountId={accountId}`.
8. User clicks Back in detail page.
9. App returns to `/accounts/{accountId}/movements`.

### Flow 4: History transaction inspection continuity

1. User opens `/history/transactions` or `/history/movements`.
2. User opens transaction detail via existing read-only links.
3. Back action resolves to originating history tab (`history-transactions` or `history-movements`) consistently.
4. No edit/delete actions are shown in read-only mode.

### Flow 5: Quick Entry account panel with global search + accordion sections

1. User opens `/quick-entry`.
2. Right-side accounts panel shows one global search field above all account sections.
3. Account sections render as single-open accordion grouped by nature.
4. Search filters accounts by name and nature label/type.
5. Matching entries remain visible even if they belong to non-open sections; matching section auto-expands.

### Flow 6: Quick Entry contextual guidance per mode

1. User toggles between Expense/Income/Transfer/Refund quick entry cards.
2. Each card header/intro area shows mode-specific guidance text from configuration (`QuickEntrySpec`-based).
3. Expense guidance is no longer only at drawer footer.

### Flow 7: Quick Entry shared date persistence

1. User sets date in one quick entry mode drawer.
2. User switches to another quick entry mode.
3. Date field keeps the previously selected date.
4. Date remains while creating transactions unless user changes it manually.

### Flow 8: Payees responsive card grid

1. User opens `/payees`.
2. Search bar remains at top.
3. Results render as responsive card grid (name + edit + delete actions).
4. Edit mode appears inline within the card.
5. Grid wraps to next row automatically.

### Flow 9: Transactions list payee column

1. User opens `/transactions`.
2. Table now has a dedicated Payee column.
3. Description column no longer embeds payee in subheadline.
4. Search still matches description and payee text.

### Flow 10: Category Totals sorting + row drilldown

1. User opens `/reports/category-totals` and loads report.
2. User can sort by account name, total amount, or transaction count.
3. User clicks any account row.
4. App opens `/accounts/{accountId}/movements` with contextual period query (`from`,`to`) and report origin metadata.

### Flow 11: Account Totals row drilldown

1. User opens `/reports/account-totals` and loads period totals tab.
2. User clicks any account row.
3. App opens `/accounts/{accountId}/movements` with contextual period query and report origin metadata.

### Flow 12: Economic State tab-level global filters without load button

1. User opens `/reports/economic-state`.
2. User changes year or focused month in global filters.
3. Report reloads automatically (debounced single call per change).
4. No explicit "Load report" button is shown.

### Flow 13: Economic State Asset/Income/Expense evolution tab layout

1. User switches to one of the three evolution tabs.
2. Main content renders three columns (33/33/33):
3. Left column: monthly table (months up to current month only for current year).
4. Middle column: composition pie (top 10 + Others for selected month).
5. Right column: stacked monthly and annual evolution charts.
6. Monthly table column previously called delta-vs-prev is shown as `Balance` and placed before end balance.

### Flow 14: Evolution monthly chart style unification

1. Any monthly evolution chart (dashboard/report tabs) uses full-month axis rendering.
2. Current day/month cutoff vertical marker is visible.
3. Future area after cutoff is shaded/disabled.
4. Line style after cutoff is dashed.

### Flow 15: Annual evolution chart style unification

1. Any annual evolution chart receives `DataUntilMonth` when year equals current year.
2. Chart shows explicit cutoff marker at current month.
3. Future area/months are visually de-emphasized.

### Flow 16: Account Group State Evolution chart visibility fix

1. User opens `/reports/account-group-totals` -> `State Evolution` tab.
2. Monthly chart is visible in viewport together with list and right chart without hidden-overflow issues.
3. On desktop, layout prioritizes visibility with no hidden chart below fold-only placement.

### Flow 17: Login remembers last successful username

1. User logs in successfully with username/email.
2. App stores it in local storage key (`ff_last_username`).
3. Next time user visits `/login`, username field is prefilled from local storage.
4. Password field remains empty.

## DETAILED PAGE WIREFRAMES

### 1) Accounts (`/accounts`)

```text
+--------------------------------------------------------------+
| Accounts                                   [Show closed] [+] |
| Accounts updated as of 2026-03-08                             |
+--------------------------------------------------------------+
| > Assets (12)                                                |
|   table...                                                   |
+--------------------------------------------------------------+
| > Liabilities (3)                                            |
+--------------------------------------------------------------+
| > Expense (22)                                               |
+--------------------------------------------------------------+
```

### 2) Quick Entry (`/quick-entry`)

```text
+-------------------------------+------------------------------+
| [Expense card v]              | Search accounts [_________]  |
| Guidance (mode specific)      | > Assets (open)              |
|  Date  Desc  Amount  Payee    |   account rows               |
|  ...                          | > Liabilities (closed)       |
| [Create]                      | > Expense (closed)           |
+-------------------------------+------------------------------+
| [Income card >] [Transfer >] [Refund >] [Widgets...]        |
+--------------------------------------------------------------+
```

### 3) Payees (`/payees`)

```text
+--------------------------------------------------------------+
| Search payees [_____________________]                        |
+--------------------------------------------------------------+
| [Card: Alice] [Edit] [Delete]   [Card: Bob] [Edit] [Delete] |
| [Card: Car]   [Edit] [Delete]   [Card: Gym] [Edit] [Delete] |
| [Card: ... wraps responsive to next row...]                 |
+--------------------------------------------------------------+
```

### 4) Transactions (`/transactions`)

```text
+-------------------------------------------------------------------+
| Date | Type | Description | Payee | Amount                        |
|---------------------------------------------------------------    |
| 08 Mar | Expense | Groceries | Store X | -120.00                  |
| 07 Mar | Income  | Salary    | Company | +3000.00                 |
+-------------------------------------------------------------------+
```

### 5) Economic State Evolution tabs (`/reports/economic-state`)

```text
+-------------------------------------------------------------------+
| Year [2026] Focused month [Mar]                                   |
+-------------------------------------------------------------------+
| [Snapshot] [Asset Evolution] [Income Evolution] [Expense Evolution]|
+-------------------------------------------------------------------+
| Monthly Table (33%) | Composition Pie (33%) | Charts Stack (33%)  |
| Month | Balance | End | ... |  Top10+Others | Monthly + Annual    |
+-------------------------------------------------------------------+
```

### 6) Account Group State Evolution (`/reports/account-group-totals`, tab 2)

```text
+-------------------------------------------------------------------+
| Summary table (left wide) | Annual chart / Composition (right)    |
+-------------------------------------------------------------------+
| Monthly evolution chart (full width under top row, always visible)|
+-------------------------------------------------------------------+
```

### 7) Transaction detail/edit with origin continuity

```text
Account movements -> /transactions/{id}?origin=accounts-movements&accountId=...
                     [Edit]
                     -> /transactions/{id}/edit?origin=accounts-movements&accountId=...
                     [Back]
                     -> /transactions/{id}?origin=accounts-movements&accountId=...
                     [Back]
                     -> /accounts/{accountId}/movements
```
## COMPONENT REUSE MATRIX

| Area | File / Component | Reuse Action | Implementation Notes |
| --- | --- | --- | --- |
| Accounts grouping | `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor` | Modify | Replace card blocks with single-open accordion sections; remove per-block current-month footer. |
| Shared accordion behavior | `src/FamilyFinances.Web/Components/Shared/SingleOpenAccordion.razor` | New | Reusable wrapper for one-open-at-a-time sections (Accounts + Quick Entry account panel). |
| Quick Entry side panel | `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor` | Modify | Move to accordion sections + global account search above groups. |
| Quick Entry drawer | `src/FamilyFinances.Web/Components/QuickEntry/QuickEntryDrawer.razor` | Modify | Consume shared selected date state and contextual guidance placement. |
| Quick Entry spec | `src/FamilyFinances.Web/Components/Dashboard/QuickEntrySpec.cs` | Modify | Add optional `Guidance`/`Description` property for each mode. |
| Transaction detail | `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionDetailPage.razor` | Modify | Parse/resolve whitelisted origin context; back button routes by context. |
| Transaction edit | `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionEditPage.razor` | Modify | Preserve origin context in back/save navigation. |
| Account movements | `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` | Modify | Include origin/query continuity and pass origin when opening transaction detail. |
| History pages | `src/FamilyFinances.Web/Components/Pages/History/HistoryTransactionsPage.razor`, `HistoryMovementsPage.razor` | Modify | Keep and align return context tokens with shared contract. |
| Category totals | `src/FamilyFinances.Web/Components/Pages/Reports/CategoryTotalsPage.razor` | Modify | Add sortable headers + row click drilldown to account movements. |
| Account totals | `src/FamilyFinances.Web/Components/Pages/Reports/AccountTotalsPage.razor` | Modify | Add row click drilldown preserving period context. |
| Economic state parent | `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor` | Modify | Remove global load button, auto-reload on filter change, keep current-year month limits. |
| Evolution panels | `AssetTotalEvolutionPanel.razor`, `IncomeEvolutionPanel.razor`, `ExpenseEvolutionPanel.razor` | Modify | 3-column layout + composition chart + table column reorder/rename + monthly cutoff alignment. |
| Account group evolution panel | `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor` | Modify | Ensure monthly evolution chart visibility and layout balance. |
| Chart components | `MonthlyLineChart.razor`, `AnnualLineChart.razor`, `AnnualBarChart.razor`, `AnnualCompositionChart.razor` | Modify | Unify export button shape and cutoff marker behavior where applicable. |
| Chart JS | `src/FamilyFinances.Web/wwwroot/js/reportCharts.js` | Modify | Add annual cutoff marker/shaded future plugin support (month-based). |
| Styling | `src/FamilyFinances.Web/wwwroot/css/app.css`, `premium-theme.css` | Modify | Harmonize export button shape, accordion and 3-column layouts, payee card grid styles. |
| Login | `src/FamilyFinances.Web/Components/Pages/Login/LoginPage.razor` | Modify | Read/write `ff_last_username` through JS interop/localStorage. |
| Localization | `src/FamilyFinances.Web/Resources/SharedResource*.resx` | Modify | Update wording (`selected month` -> `current month`, report naming, new column labels, new hints). |

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Whitelisted transaction origin context

```csharp
internal enum TransactionOrigin
{
    Transactions,
    AccountsMovements,
    HistoryTransactions,
    HistoryMovements,
    ReportCategoryTotals,
    ReportAccountTotals
}

internal sealed record TransactionOriginContext(
    TransactionOrigin Origin,
    Guid? AccountId = null,
    DateOnly? From = null,
    DateOnly? To = null)
{
    public static TransactionOriginContext FromQuery(IReadOnlyDictionary<string, string?> query)
    {
        var originRaw = query.GetValueOrDefault("origin");
        var origin = originRaw?.ToLowerInvariant() switch
        {
            "accounts-movements" => TransactionOrigin.AccountsMovements,
            "history-transactions" => TransactionOrigin.HistoryTransactions,
            "history-movements" => TransactionOrigin.HistoryMovements,
            "report-category-totals" => TransactionOrigin.ReportCategoryTotals,
            "report-account-totals" => TransactionOrigin.ReportAccountTotals,
            _ => TransactionOrigin.Transactions
        };

        return new TransactionOriginContext(
            origin,
            Guid.TryParse(query.GetValueOrDefault("accountId"), out var accountId) ? accountId : null,
            DateOnly.TryParse(query.GetValueOrDefault("from"), out var from) ? from : null,
            DateOnly.TryParse(query.GetValueOrDefault("to"), out var to) ? to : null);
    }
}
```

### Example 2: Preserve origin in TransactionDetail -> Edit navigation

```razor
@code {
    private string BuildEditUrl()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Origin))
            parts.Add($"origin={Uri.EscapeDataString(Origin)}");
        if (OriginAccountId is not null)
            parts.Add($"accountId={OriginAccountId}");
        if (OriginFrom is not null)
            parts.Add($"from={OriginFrom:yyyy-MM-dd}");
        if (OriginTo is not null)
            parts.Add($"to={OriginTo:yyyy-MM-dd}");

        var query = parts.Count == 0 ? string.Empty : $"?{string.Join("&", parts)}";
        return $"/transactions/{Id}/edit{query}";
    }
}
```

### Example 3: Shared single-open accordion usage (Accounts / Quick Entry)

```razor
<SingleOpenAccordion Id="accounts-by-nature" Sections="@_sections">
    <SectionTemplate Context="section">
        <table class="table table-sm table-hover align-middle mb-0 ff-data-table">
            ...
        </table>
    </SectionTemplate>
</SingleOpenAccordion>
```

### Example 4: Quick Entry shared date state across modes

```csharp
private DateOnly _sharedBookedOn = DateOnly.FromDateTime(DateTime.Today);

private void SetActive(QuickEntryType type)
{
    _activeEntry = type;
    _expandedWidget = null;
    SyncSelectionForActiveEntry();
}

private QuickEntryDrawerModel BuildDrawerModel() => new()
{
    BookedOn = _sharedBookedOn,
    OnBookedOnChanged = value => _sharedBookedOn = value
};
```

### Example 5: Category totals sorting + drilldown

```razor
@foreach (var item in SortItems(_report.Items))
{
    <tr class="clickable-row"
        @onclick="() => OpenAccountMovements(item.AccountId)">
        <td>@item.AccountName</td>
        <td class="text-end">@MoneyFormatter.FormatCentsWithSign(item.Total)</td>
        <td class="text-end">@item.TransactionsCount</td>
    </tr>
}
```

```csharp
private void OpenAccountMovements(Guid accountId)
{
    var from = _fromDate.ToString("yyyy-MM-dd");
    var to = _toDate.ToString("yyyy-MM-dd");
    Nav.NavigateTo($"/accounts/{accountId}/movements?origin=report-category-totals&from={from}&to={to}");
}
```

### Example 6: Annual cutoff marker support in chart payload

```javascript
const markerMonthRaw = Number(payload.markerMonth);
const markerMonth = Number.isFinite(markerMonthRaw) ? Math.trunc(markerMonthRaw) : null;
const markerIndex = markerMonth ? Math.max(0, markerMonth - 1) : null;

const datasets = (payload.datasets || []).map((dataset) =>
  toDataset(dataset, markerIndex)
);

const cutoffPlugin = markerIndex !== null ? buildCutoffPlugin(markerIndex) : null;
new Chart(canvas, {
  type: payload.type || "line",
  data: { labels: payload.labels, datasets },
  plugins: cutoffPlugin ? [cutoffPlugin] : [],
  options: { ... }
});
```

## Risks / Trade-offs

- [Navigation context complexity] More query parameters increase state handling branches. -> Mitigation: centralize parsing/serialization in one helper and whitelist origins only.
- [Extra API calls for composition in evolution tabs] Fetching account evolution in each panel can add latency. -> Mitigation: cache per selected year per panel and reuse for month changes.
- [Responsive layout regression in 3-column evolution view] Added column can collapse poorly at medium breakpoints. -> Mitigation: explicit breakpoints (`xl`/`xxl`) and test with `ReportResponsiveLayoutTests`.
- [Sort + row click discoverability conflict] Sort headers and row click can compete for pointer behavior. -> Mitigation: keep sort controls in headers only and row click on tbody cells with cursor affordance.
- [Localization drift] New labels might diverge between `es-ES` and `en-US`. -> Mitigation: update both locale files together and assert key presence in tests.
- [Transaction list DTO expansion] Adding payee field affects consumers. -> Mitigation: additive field only, keep old fields untouched.
- [Chart rendering complexity in JS] Adding monthly+annual cutoff logic can affect existing charts. -> Mitigation: preserve backward compatibility when marker is null and extend chart tests.

## Migration Plan

1. Implement shared primitives first:
1. Add origin context helper (web layer).
1. Add single-open accordion shared component.
1. Extend chart payload contracts (`markerMonth` for annual usage).
1. Implement navigation continuity changes:
1. Update AccountMovements, TransactionDetail, TransactionEdit, and History links.
1. Implement page-level presentation changes:
1. Accounts accordion and "updated as of" description.
1. Quick Entry global account search + accordion + shared date + per-mode guidance.
1. Payees card-grid layout.
1. Transactions table payee column integration.
1. Implement report interactions:
1. Category Totals sort + drilldown.
1. Account Totals drilldown.
1. Economic State tab filter behavior and three-column evolution layout.
1. Account Group State Evolution visibility fix.
1. Update localization resources.
1. Update/add tests and run full web test suite.

Rollback strategy:

1. Revert per-area commits in reverse order (reports -> transactions/navigation -> quick entry/accounts -> shared helpers).
1. If chart cutoff changes regress, keep previous payload behavior by removing marker fields while preserving existing `DataUntilMonth`.
1. If origin navigation fails, fallback to current `/transactions` back path until context helper is corrected.

## Open Questions

1. Dashboard month selector: should dashboard remain fixed to current month in this change, or should month selection be enabled on dashboard as an explicit control?
1. Report drilldown period propagation: when opening account movements from report rows, should report dates prefill movement filters exactly, or should movements keep their current-month defaults and only preserve origin?
1. Payees large dataset behavior: is pagination/virtualization required now, or is responsive card wrapping with search sufficient for current scale?
## IMPLEMENTATION VERIFICATION CHECKLIST

- ? Dashboard subtitle no longer says "selected month" in `es-ES` and `en-US`.
- ? Dashboard keeps existing KPI and chart data behavior.
- ? Accounts page groups render as accordion sections.
- ? Only one accounts section is expanded at a time.
- ? First accounts section opens by default.
- ? Accounts per-section "current month basis" footer is removed.
- ? Accounts page displays one "updated as of {date}" message.
- ? Accounts table still shows current-month and accumulated balances.
- ? Account actions (rename/view movements/close/reopen/delete) still work.
- ? Quick Entry right panel has one global account search box.
- ? Quick Entry account groups are shown as single-open accordion sections.
- ? Global search filters by account name.
- ? Global search also matches account nature/type label.
- ? Matching group auto-expands during search.
- ? Quick Entry mode-specific guidance text is visible for Expense.
- ? Quick Entry mode-specific guidance text is visible for Income.
- ? Quick Entry mode-specific guidance text is visible for Transfer.
- ? Quick Entry mode-specific guidance text is visible for Refund.
- ? Quick Entry selected date persists when switching modes.
- ? Quick Entry created transaction keeps expected account-selection behavior.
- ? Payees page keeps search capability.
- ? Payees page renders responsive card grid instead of one long table.
- ? Payee rename action still works in new card layout.
- ? Payee delete action still works in new card layout.
- ? Transactions table includes a dedicated Payee column.
- ? Transactions description column no longer prepends payee text.
- ? Transactions search still works for description text.
- ? Transactions search works for payee text.
- ? Account movements row navigation includes origin context.
- ? Transaction detail resolves back destination from origin context.
- ? Transaction edit back action preserves origin context.
- ? Transaction edit save redirects to detail while preserving origin context.
- ? History transactions links still include read-only behavior.
- ? History movements links still include read-only behavior.
- ? History detail back returns to the correct history tab.
- ? Category Totals table supports sortable columns.
- ? Category Totals sort toggles ascending/descending correctly.
- ? Category Totals row click opens account movements route.
- ? Category Totals drilldown preserves report origin metadata.
- ? Account Totals row click opens account movements route.
- ? Account Totals drilldown preserves report period context.
- ? Economic State global "Load report" button is removed.
- ? Economic State changes year/month trigger reload automatically.
- ? Economic State Asset evolution uses 3-column layout (table/pie/charts).
- ? Economic State Income evolution uses 3-column layout (table/pie/charts).
- ? Economic State Expense evolution uses 3-column layout (table/pie/charts).
- ? Evolution table hides months beyond current month for current year.
- ? Evolution table keeps full 12 months for past years.
- ? Evolution table column "Delta vs previous month" renamed to "Balance".
- ? Evolution table places "Balance" before "End balance".
- ? Evolution composition chart aggregates over top 10 into `Others`.
- ? Monthly evolution charts show full month range.
- ? Monthly evolution charts show cutoff marker and disabled future area.
- ? Annual line charts show current-month cutoff marker when applicable.
- ? Annual bar charts show current-month cutoff marker when applicable.
- ? Export buttons in chart headers match app button visual shape.
- ? Account Group State Evolution monthly chart is visible without hidden overflow issues.
- ? Login stores last successful username in local storage.
- ? Login prefills username field with last successful username.
- ? Login does not store password.
- ? Added/updated tests cover navigation continuity paths.
- ? Added/updated tests cover Quick Entry shared date behavior.
- ? Added/updated tests cover payee card rendering + actions.
- ? Added/updated tests cover report drilldown and sorting behavior.
- ? Added/updated tests cover Economic State 3-column layout and cutoff behavior.
- ? Added/updated tests cover login remembered-username behavior.
