## Context

`AccountGroupTotalsPage` already requests movement rows for the selected account group and the active report filters. Those rows are rendered below the totals report, ordered newest first, and contain the data needed by users to audit group spending. Other reporting areas provide a card header, a period badge, and a CSV download action, while this drill-down currently does not.

## Goals

- Make the movement drill-down visually consistent with the reporting cards around it.
- Make the active period explicit where the movement rows are read.
- Let users download exactly the movement result that is currently visible.

## Non-Goals

- Alter movement retrieval, filtering, ordering, or navigation to transaction detail.
- Add a new export API or download data not already loaded in the browser.
- Change the existing account-group totals CSV export.

## Decisions

### Render movements in an autonomous report card

The movement table, loading state, error feedback, and empty state will live inside a report card. Its header will contain the localized title, the standard `Exportar CSV` button when rows are available, and a period badge derived from the active movement filter range.

This keeps the action close to the data it affects and prevents users from confusing it with the separate totals export. The card will retain the existing filtered-state feedback when no rows match, but will not expose an export action for an empty result.

### Scope the account-breakdown export to its own card

The existing CSV action exports account-breakdown rows, not the selected group's summary. It will therefore move from the group-summary card header into a dedicated account-breakdown card, alongside the table and its active period badge. The group-summary header will retain only contextual badges.

This preserves the CSV format and filter semantics while making the action's target obvious. The movement card remains a separate card with its own export action because it represents a different dataset.

### Navigate from account breakdown to filtered account movements

Each account name in the account-breakdown card will link to that account's movement page. The link will carry the active `from` and `to` values and record the account-group report as the parent context with its group and nature filter.

The account movement page will initialize its date filters from that context. Its Back action will return to the group report when a group parent is present; otherwise it will keep its existing return-to-accounts behavior. Transactions opened from the account movement page will use the account-movements origin so their own Back action returns to the immediately preceding account movement view, while retaining the group parent context for the next Back action.

### Export the already loaded view client-side

The export will reuse `ReportCsvBuilder`, the existing report filename convention, and the JavaScript CSV download interop. The movement request already returns the complete filtered result used by the table, so a client-side export preserves the exact visible order and does not introduce a redundant endpoint.

The generated CSV will contain the report context (account group, period, and nature where applicable) and the table's movement data: date, description, payee, source account, destination account, amount, and accumulated amount. Values will use the established report formatting conventions.

### Keep feedback local to the movement card

If browser download fails, the error will be shown in the movement card rather than replacing the report-level export feedback. This lets a user distinguish an export failure from a report-loading failure and continue using the totals report.

## Alternatives Considered

### Reuse the totals card export button

Rejected because it would make one action export two conceptually distinct datasets or leave its scope unclear. The movement table needs its own explicit export action.

### Add a dedicated backend CSV endpoint

Rejected because all filtered rows are already available in the page and existing reports use client-side CSV generation. A new endpoint would add contract and authorization surface without adding value for this result size and interaction.

## Testing Strategy

- Add bUnit coverage that verifies the movement card header and period badge for the active filters.
- Verify the export control is available only when movement rows are present.
- Mock the existing download interop and assert the exported filename and CSV include the loaded rows, report context, and current visible order.
- Retain existing tests for empty, error, and transaction-navigation states.

## Risks and Mitigations

- **Export differs from the table:** Build the CSV directly from the same loaded DTO collection and preserve its order.
- **Ambiguous export scope:** Use a separate card-local action and include group/period/nature context inside the CSV.
- **Visual inconsistency:** Reuse the existing report card, badge, and export-button patterns instead of introducing a new component style.
