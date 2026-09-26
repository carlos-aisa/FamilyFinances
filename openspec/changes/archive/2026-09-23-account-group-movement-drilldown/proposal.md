## Why

The Dashboard identifies account groups worth monitoring, but users cannot reach the corresponding report from a group row. The group totals report also explains aggregate amounts without showing the transactions that produced them, making expense investigation slow and disconnected from transaction editing.

## What Changes

- Make each Dashboard pinned account-group name a link to its account-group totals report for the Dashboard selected month.
- Add a group-movement drill-down that returns one row per transaction touching a selected group's member accounts within the active date and nature filters.
- Display the drill-down in the account-group totals report with date, description, source accounts, destination accounts, group net amount, and running accumulation.
- Allow users to open a movement's transaction and return to the same account-group report context.
- Add the authenticated reporting endpoint and its OpenAPI contract.

## Capabilities

### New Capabilities

- `account-group-movement-drilldown`: Retrieve, display, and navigate from filtered account-group transaction movements.

### Modified Capabilities

- `dashboard-household-financial-overview`: Pinned account-group rows provide a semantic link to the selected-period group report.

## Impact

- Affects Reporting Application and Infrastructure read paths, the versioned Reports API, and the Web reports client.
- Affects Dashboard, account-group totals, transaction-detail return navigation, and localized Web resources.
- Adds an OpenAPI operation without breaking existing reporting contracts or changing the database schema.
- Adds API integration tests and Web component/navigation tests.

## Non-Goals

- Do not change account-group membership, account balances, or transaction posting semantics.
- Do not add pagination, a separate movements page, or exports for the new drill-down.
- Do not make annual-chart bars or non-pinned Dashboard groups clickable.

## Rollback Plan

- Remove the Dashboard group link and the movements section if the interaction needs to be disabled.
- Remove the additive endpoint and client call without changing existing report endpoints or persisted data.
- Re-run Reporting API and Web report tests to confirm the existing account-group totals flow remains available.
