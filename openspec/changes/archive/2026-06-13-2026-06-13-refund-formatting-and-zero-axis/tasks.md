## 1. Refund quick-entry formatting hardening

- [x] 1.1 Ensure original-expense rows in refund quick entry render booked date using `dd/MM/yyyy`.
- [x] 1.2 Ensure amount badge rendering in that list uses EUR suffix format (`XXX,XX €`).
- [x] 1.3 Verify selection behavior and transaction-link workflow remain unchanged.

## 2. Shared money/date formatter alignment

- [x] 2.1 Update affected formatter paths so touched surfaces render money as European numeric with trailing euro symbol.
- [x] 2.2 Preserve sign semantics and null-safe rendering behavior.
- [x] 2.3 Keep public helper APIs backward compatible unless explicitly unnecessary.

## 3. Chart zero-baseline visual emphasis

- [x] 3.1 Add shared Y-axis grid style helper in report chart JS with scriptable `color`, `lineWidth`, and `borderDash`.
- [x] 3.2 Emphasize tick value `0` relative to other Y-axis ticks.
- [x] 3.3 Apply the helper to annual line and annual bar chart Y axes (single and dual-axis contexts).

## 4. Test and regression updates

- [x] 4.1 Update/add tests for date formatting behavior in touched date helper paths.
- [x] 4.2 Update/add tests for money formatter expectations with trailing euro symbol.
- [x] 4.3 Run focused Web tests for quick-entry/reporting formatting regressions.

## 5. Validation and OpenSpec consistency

- [x] 5.1 Run `openspec validate 2026-06-13-refund-formatting-and-zero-axis --strict`.
- [x] 5.2 Confirm proposal/tasks/spec deltas match implemented scope and touched files.
