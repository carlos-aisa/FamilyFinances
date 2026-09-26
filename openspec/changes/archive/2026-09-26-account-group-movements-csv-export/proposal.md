## Why

The account-group movement drill-down lets users inspect the transactions behind a group total, but those filtered rows cannot be saved or shared. Its unframed presentation also makes the currently applied reporting period less visible than it is on the surrounding report cards.

## What Changes

- Present the account-group movement drill-down in a report card that follows the shared card-header layout.
- Display the active reporting period in the movement card header.
- Add a local `Exportar CSV` action that exports the currently displayed, filtered movement rows together with their report context.
- Move the existing account-breakdown CSV action from the group-summary header into an account-breakdown card with its own period context.
- Allow users to open an account's movements from its account-breakdown row while preserving the account-group report's period and return-navigation context.
- Reuse the existing client-side CSV generation and browser download pattern; no API, persistence, or report-query change is required.

## Capabilities

### New Capabilities

- `account-group-totals-export-organization`: Scope the existing account-breakdown export to a dedicated account-breakdown card.
- `account-group-account-movement-navigation`: Navigate from an account-breakdown row to that account's filtered movements and back to the originating group report.

### Modified Capabilities

- `account-group-movement-drilldown`: Display the selected group's filtered movement rows in a contextual report card and allow their CSV export.

## Impact

- Affected frontend: the account-group totals report, account movements page, existing report-card styles, localization resources, and the existing CSV export helper/interoperability.
- Affected tests: account-group totals and account movements bUnit coverage for cards, period context, CSV download content, and contextual navigation.
- No backend endpoint, API contract, data model, or database migration change.

## Non-Goals

- Adding a server-side export endpoint.
- Exporting movement rows that are not loaded by the active report filters.
- Changing the movement filter semantics or the account-group totals export.

## Rollback Plan

Remove the movement-card export action and retain the existing on-screen drill-down; no persisted data or contract requires rollback.
