# Accounts Current Balance Column Design

## Context

The Accounts list currently displays each account's current-month balance and year-to-date accumulated balance. The existing account-balances API response also includes the account's total current balance, but the UI does not render it. This makes it difficult to identify an account's actual position without deriving it from other views.

## Decision

Add a `Current balance` column to the Accounts list between `Accumulated balance` and `Status`.

The column uses the existing `AccountBalanceDto.Balance` value. It represents the account's total balance as of the list's update date, including transactions from previous fiscal years. It is distinct from:

- `Current month balance`, the movement during the current calendar month.
- `Accumulated balance`, the movement from the start of the current year.

No API, persistence, or balance-calculation changes are required.

## User Experience

- The header is localized as `Current balance`.
- Values use the existing euro formatter and right-aligned numeric table style.
- Accounts without a returned balance retain the existing muted em dash treatment.
- The new column appears in every account-nature section and preserves the existing account actions.

## Testing

- Extend Accounts list component tests to assert the localized column header, its position, and rendering of the existing total-balance value.
- Preserve the API integration coverage that establishes the distinct semantics of total, current-month, and year-to-date balances.

## Scope

This change only exposes an already calculated value in the Accounts list. It does not change account balances, filtering, account movement history, or reporting semantics.
