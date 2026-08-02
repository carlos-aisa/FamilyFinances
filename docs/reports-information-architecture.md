# Reports Index Information Architecture

## Purpose

The `/reports` landing page groups report entry cards by the financial question they answer. The grouping improves report discovery without changing report routes, calculations, filters, or data retrieval.

## Analytical Families

The page renders these families in a fixed order:

1. **Financial Snapshot**: `Economic State` provides the point-in-time view of the financial position. `Asset Total Balance` remains available through its existing deep link but is intentionally not duplicated in the index because its summary is already represented by Economic State.
2. **Period Flow Analysis**: `Monthly Summary` and `Category Totals` explain income, expenses, and net results over a selected period.
3. **Account Structure Analysis**: `Account Totals` and `Account Group Totals` support analysis by individual account and account group.

Cards remain directly actionable and use their existing `/reports/*` routes. The index remains neutral: it does not promote a recommended first report.

## Copy And Accessibility

Family headings are semantic rather than workflow-oriented. Each card uses a concise title and a one-line description of its purpose. Families with two cards use a two-column desktop layout to reduce description wrapping. Existing premium card classes, authorization gating, and pointer interaction are preserved. The grouped sections use semantic `section` elements with labelled headings.

## Verification

`ReportsIndexPageTests` covers the family order, card order, the five primary report destinations, intentional absence of the redundant asset total balance card, default-resource fallback, the existing stock-versus-flow explanation, and unauthenticated rendering.
