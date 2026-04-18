## Why

Users need to filter transactions by amount range to find specific transaction sizes (e.g., all transactions between €50-€100). Currently, only text-based and date-based filters exist. Adding amount range filtering improves transaction discovery and analysis capabilities.

## What Changes

- Add amount range filter (min/max) to TransactionsListPage (`/transactions`)
- Add amount range filter (min/max) to AccountMovementsPage (`/accounts/{id}/movements`)
- Two separate numeric input fields: "Amount From" and "Amount To"
- Filter by absolute value (e.g., range 10-50 includes both -€30 expense and +€30 income)
- Include boundary values (min/max are inclusive)
- Client-side filtering for TransactionsListPage
- Backend filtering for AccountMovementsPage (requires API extension)

## Release Impact

Type: minor
Rationale: New backward-compatible functionality adding optional filter parameters without changing existing behavior

## Capabilities

### New Capabilities
<!-- None - this extends existing transaction filtering capability -->

### Modified Capabilities
- `transaction-list-filtering`: Add amount range filter (min/max, absolute value) to transaction search UI and filtering logic
- `account-movements-filtering`: Add amount range filter (min/max, absolute value) to account movements search UI and API

## Impact

**Backend:**
- Extend `GetAccountMovementsAsync` repository method to accept `minAmount` and `maxAmount` parameters
- Add amount range filtering to EF Core query (filter on absolute value of SignedAmount)
- Extend `GET /api/v1/accounts/{id}/movements` API to accept `minAmount` and `maxAmount` query parameters
- Update API client `AccountsApi.GetMovementsAsync` signature

**Frontend:**
- TransactionsListPage: Add two numeric inputs for amount range in filter panel
- AccountMovementsPage: Add two numeric inputs for amount range in filter panel
- Client-side filtering in TransactionsListPage using absolute value comparison
- API integration in AccountMovementsPage passing new parameters
- Add localization keys for amount range labels

**Testing:**
- Add tests for amount range filtering in TransactionsListPage (client-side)
- Add tests for amount range filtering in AccountMovementsPage (API integration)
- Add API integration tests for amount range parameters
- Add repository tests for amount range query logic
- Test boundary conditions (exact min/max values included)
- Test absolute value behavior (positive and negative amounts)
