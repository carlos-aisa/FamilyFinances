## Context

Account-group totals already provide a filtered aggregate for a selected group. The implementation needs to expose the underlying ledger transactions without duplicating reporting calculations in the browser, while preserving the report filters when a user temporarily opens a transaction.

## Goals / Non-Goals

**Goals:**

- Provide a direct Dashboard entry to a pinned group's selected-period report.
- Return a deterministic, transaction-level group drill-down for a date range and optional account nature.
- Preserve the selected report context through transaction detail and edit navigation.
- Keep the implementation within the existing Application, Infrastructure, API, and Blazor layer boundaries.

**Non-Goals:**

- Change ledger data, group membership, or account balances.
- Introduce client-side aggregation, pagination, exports, or a standalone movement-report route.
- Make every Dashboard visualization a drill-down control.

## Decisions

### Dedicated reporting endpoint

`GET /api/v1/reports/account-groups/{groupId}/movements` accepts the group identifier, inclusive/exclusive period bounds, and optional nature. A dedicated endpoint keeps report data shaping in the Reporting stack and avoids coupling the Web page to multiple ledger endpoints.

Alternative considered: derive rows from the existing account-group totals response. Rejected because totals do not contain transaction identifiers, counterparty account names, or enough data to safely reconstruct a group net amount.

### One row per transaction with all split context

The repository finds matching member-account splits, groups them by transaction, and queries all splits of those transactions to produce source and destination account names. The group's net amount is the negated sum of matching split amounts, so positive values represent money entering the group and negative values represent money leaving it.

Alternative considered: one row per split. Rejected because multi-split transactions would be fragmented and would not answer what happened in a single business event.

### Running accumulation is computed in chronological order

The repository orders movement rows ascending by booking date, creation date, and transaction identifier, computes the running net from zero at the start of the selected range, then returns the completed rows in descending display order. This keeps the visible list newest-first without making the accumulation ambiguous.

### Typed report-origin context

Transaction navigation adds a report-account-group origin plus group, period, and nature query values. Transaction detail uses that context for both Back and Edit links, returning users to the same group report state instead of a generic transactions list.

## Risks / Trade-offs

- [Risk] A group may have many matching transactions, so an unpaginated response can grow large. → Mitigation: keep the new response scoped to the selected report filters; pagination is explicitly deferred and can be added through a separate contract change.
- [Risk] Internal transfers can be difficult to interpret. → Mitigation: include every transaction that touches a group account and show both source and destination account names.
- [Risk] The optional nature filter can hide member-account splits. → Mitigation: preserve the active filter in the response and navigation context so the displayed result remains explainable.

## Migration Plan

1. Deploy the additive endpoint, DTOs, and OpenAPI operation.
2. Deploy the Dashboard link and account-group totals movements section.
3. Verify API integration, Dashboard, report, and transaction-navigation tests.
4. To roll back, remove the additive Web section and endpoint; no migration or data rollback is required.

## Open Questions

- Whether a future change should add paging, CSV export, or a dedicated movements route for very large filtered ranges.
