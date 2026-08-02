# Account state composition-only design

## Goal

The annual account evolution list in Account Totals State Evolution does not provide useful information alongside the account summaries. The panel should focus exclusively on the existing expense and income composition analysis.

## Design

- Remove the Evolution/Composition mode selector from the annual account panel.
- Always render the existing composition card.
- Retain the existing Expense/Income nature selector and focused-month selector.
- Keep the account summary, grouped account details, year selector, and all other report sections unchanged.

## Scope cleanup

Remove the annual account list rendering path and only the supporting state, dataset adaptation, export context, and tests that exist solely for that path. Do not change the composition calculations, data contracts, account details, or account-group state view.

## Testing

- Verify Account Totals State Evolution renders composition directly without the removed mode selector or annual account list.
- Verify expense and income composition switching remains available and renders the existing composition data.
- Run the affected Web tests, the complete solution suite, and strict OpenSpec validation.
