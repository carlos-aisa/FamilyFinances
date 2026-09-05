# Dashboard latest expenses design

## Goal

Replace the Dashboard's `Monthly Summary` card with `Latest expenses`. The card shows the six most recent movements posted to accounts whose nature is `Expense`.

## Data flow

The application exposes a focused read query and API endpoint for the six latest expense movements. It filters by account nature, orders by movement date descending and then transaction identifier descending, and projects only the data required by the card: date, optional description, optional payee, and amount.

The Dashboard loads this collection separately from its existing overview payload. No forecasting, recurrence, configurable-widget, or generic-feed abstraction is introduced.

## Presentation

A reusable Razor movement-list component receives presentation-ready list items and has no knowledge of the query or source of its data. Following the compact table layout of the Dashboard's highlighted-groups card, each row renders only the date, description, and absolute monetary amount using neutral styling. Payee is deliberately omitted from this compact Dashboard presentation. The component remains suitable for a future `Upcoming planned expenses` card without alteration to its data-access boundary.

When no expense movements exist, the card shows the existing localized no-data state.

## Verification

- Application/API tests cover the `Expense` filter, six-item limit, date order, and identifier tie-breaker.
- Web tests cover Dashboard loading of the new source and the replacement card's title, rows, optional descriptive fields, and neutral positive amount presentation.
- Existing Dashboard overview and unrelated reporting behavior remain unchanged.
