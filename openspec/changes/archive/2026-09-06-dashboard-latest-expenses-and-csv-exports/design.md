## Context

The Dashboard already loaded a reporting overview for its analytical cards. The requested latest-expenses block needs different data: recent ledger transactions whose splits include Expense-nature accounts. The same Dashboard row also contains two table/list panels that need actionable CSV export controls rather than decorative badges.

## Goals / Non-Goals

**Goals:**

- Retrieve six Expense-related transactions deterministically by booked date and transaction identifier.
- Keep movement rendering reusable without coupling it to a data-source abstraction.
- Present dates, descriptions, and non-negative expense magnitudes without unfavorable-result coloring.
- Export the visible data of each third-row Dashboard card as CSV, including selected-period context.
- Keep highlighted-group Expense metrics neutral and order those rows by monthly result.

**Non-Goals:**

- Forecasting, recurring expenses, provider-based feeds, or configurable Dashboard widgets.
- New persistence tables, schema migrations, or changes to transaction balance rules.
- A reusable generic export subsystem beyond existing CSV utilities.

## Decisions

### Decision 1: Provide a focused latest-expenses read endpoint

- **Choice:** Add a dedicated authorized read endpoint backed by the transaction repository.
- **Rationale:** The Dashboard requires a small, bounded, deterministic data set that does not belong in the reporting-overview contract.
- **Alternative considered:** Extend the Dashboard overview response.
- **Rejected because:** It would couple a ledger movement feed to unrelated analytical aggregates and make future movement-list reuse harder.

### Decision 2: Reuse a visual movement list, not a provider abstraction

- **Choice:** Create a movement-list component that accepts display items while the Dashboard obtains data through its concrete API client call.
- **Rationale:** This separates rendering from retrieval while preserving a small, explicit scope for a future planned-expenses view.
- **Alternative considered:** Add a generic movement-feed provider.
- **Rejected because:** It would add premature forecasting/feed architecture without a second active source.

### Decision 3: Export per card through existing CSV primitives

- **Choice:** Each Dashboard card builds its visible rows and delegates CSV construction/download to the existing report CSV builder and JS interop helper.
- **Rationale:** It provides a functional button without duplicating escaping or browser-download behavior.
- **Alternative considered:** Add a server-side export endpoint or generic Dashboard export service.
- **Rejected because:** The displayed data is already client-side, and those abstractions exceed the requested scope.

### Decision 4: Treat Expense values as magnitudes in visual and export output

- **Choice:** Apply absolute-value formatting for latest expenses and Expense-kind highlighted groups, without red unfavorable-result semantics.
- **Rationale:** These panels communicate spending volume rather than financial performance.
- **Alternative considered:** Preserve signed result formatting everywhere.
- **Rejected because:** Negative signs and unfavorable colors make expense lists harder to scan and conflict with their intended meaning.

## Risks / Trade-offs

- [Risk] A transaction can contain multiple Expense splits. -> Mitigation: the handler sums their absolute values into one movement amount.
- [Risk] CSV localization can make manual comparisons harder. -> Mitigation: use stable ISO dates and the existing localized monetary formatter; include period context.
- [Risk] The component may be reused with a future source having different fields. -> Mitigation: keep its input limited to display-ready date, description, and amount.

## Migration Plan

1. Deploy the read-only API endpoint and the OpenAPI contract with the Web consumer.
2. Replace the Dashboard summary with the latest-expenses card and retain existing Dashboard data loading/error patterns.
3. Add the CSV buttons and period badges to the third-row card headers.
4. Validate application, API integration, Web component, full solution, and OpenSpec checks.
5. Roll back by removing the new endpoint/card/controls if necessary; no data migration is required.

## Open Questions

- A future planned-expenses feature can reuse the component after its concrete data contract is defined.
