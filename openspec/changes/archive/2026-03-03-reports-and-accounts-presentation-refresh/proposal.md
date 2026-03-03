## Why

Current reporting and accounts presentation does not provide enough decision-ready context in a single screen. Users need clearer period-vs-accumulated balances, expense evolution parity in Economic State, and denser layouts that keep key lists and charts visible together without scrolling in standard desktop usage.

## What Changes

- Update the Accounts page to show two balance perspectives per account:
  - accumulated balance (current behavior),
  - selected-period balance for this iteration fixed to the current month.
- Update Economic State with a new **Expenses** evolution tab, symmetric to existing Asset/Income evolution tabs.
- Update Economic State summary content to include:
  - monthly net list computed as **Income - Expense** (positive when income exceeds expense),
  - existing month-focused Income vs Expense chart,
  - new annual bar chart with month-by-month Income vs Expense comparison.
- Refine summary layout so monthly net list and both charts fit on screen in a 1920x1080 desktop viewport without requiring vertical scroll.
- Update Account Totals (period totals tab):
  - default ordering inside each nature group by net change,
  - user-driven sorting by clicking table headers.
- Update Account Group State Evolution:
  - list must show balance for the exact selected month,
  - annual evolution chart must use bars and month result values (non-cumulative),
  - rebalance table/chart width to give charts more visual space,
  - remove or relocate comparability info card to avoid reducing usable analysis area.
- Add/update localization strings and automated tests to cover new tab, chart semantics, sorting behavior, and revised layouts.

## Capabilities

### New Capabilities
- `accounts-balance-presentation`: Account list presents both accumulated balance and current-month period balance in a single view.

### Modified Capabilities
- `economic-state-reporting`: Add expense evolution tab and expanded summary section requirements (monthly net list, annual Income vs Expense bars, no-scroll desktop fit target).
- `monthly-balance-evolution-reporting`: Extend supported scope set to include expense total evolution and enforce selected-month exact balance semantics in group evolution list context.
- `annual-reporting-charts`: Update annual group evolution visualization to bar-based monthly results and add annual Income vs Expense bar comparison in Economic State summary.

## Impact

- Affected frontend pages/components:
  - `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/EconomicStatePage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/AccountTotalsPage.razor`
  - `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor`
  - chart components and shared reporting CSS/JS.
- Affected backend contracts and query mapping:
  - monthly evolution scope parsing/mapping for expense-total,
  - reporting repository aggregation for expense-total evolution series.
- Affected tests:
  - Web report component tests (tabs, sorting, layout markers, chart rendering paths),
  - Reports API tests and monthly evolution integration tests for new scope behavior.
- No breaking API removal is planned; existing endpoints and aliases remain.

## Non-Goals

- Adding a custom date-range selector to the Accounts page in this change (period is fixed to current month).
- Changing core accounting formulas or ledger posting semantics.
- Redesigning report information architecture beyond requested presentation changes.
- Guaranteeing no-scroll behavior for every viewport size; the explicit target is 1920x1080 desktop.

## Rollback Plan

- Revert the new Expense evolution tab wiring and restore previous Economic State tab set.
- Restore prior annual chart rendering mode (line/cumulative where applicable) and previous account-group chart panel layout.
- Revert account-table sortable headers and default order changes to previous static ordering.
- Keep localization additions harmless if code is rolled back, and rerun reporting web/API test suites to validate baseline behavior restoration.
