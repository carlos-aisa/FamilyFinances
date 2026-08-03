## 1. Annual evolution lists

- [x] 1.1 Add a reusable annual evolution list component with account-group and account layouts, localized period context, and CSV export.
- [x] 1.2 Replace the Dashboard annual group chart with a complete group list ordered by selected-month magnitude.
- [x] 1.3 Preserve semantic green income, red expense, and neutral mixed or unknown group colours.
- [x] 1.4 Simplify Account Totals State Evolution to the existing expense and income composition analysis.

## 2. Drill-down navigation

- [x] 2.1 Link group rows to Account Group Totals with group, year, and month query context.
- [x] 2.2 Validate the Dashboard group query context and load the matching calendar month at its destination.

## 3. Scope cleanup and coverage

- [x] 3.1 Remove the rejected heatmap, trajectory-strip, SVG export, styling, resources, and their tests.
- [x] 3.2 Keep focused-month daily comparisons and income-versus-expense controls unchanged.
- [x] 3.3 Add component and host coverage for list rendering, semantic presentation, drill-down URLs, and destination query handling.

## 4. Account-group historical account detail

- [x] 4.1 Keep the group summary expand/collapse action separate from group-month selection.
- [x] 4.2 Show only the selected group's member accounts with exact monthly and year-to-date values in the right Evolution panel.
- [x] 4.3 Remove navigation from the account detail list and add focused panel coverage.
- [x] 4.4 Use the existing localized CSV export label in the reusable list and cover its rendered text.

## 5. Account state composition-only panel

- [x] 5.1 Remove the annual account list and the Evolution/Composition mode selector.
- [x] 5.2 Retain direct expense and income composition selection with the focused-month filter.
- [x] 5.3 Update focused Web coverage for the composition-only behavior.
- [x] 5.4 Remove the unused account direct-navigation path; account navigation remains deferred.

## 6. Validation

- [x] 6.1 Run the focused Web test subset (6 tests passed on 2026-08-02).
- [x] 6.2 Run the complete solution test suite (754 tests passed on 2026-08-02).
- [x] 6.3 Run strict OpenSpec validation.
