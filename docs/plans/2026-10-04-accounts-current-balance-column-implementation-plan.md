# Accounts Current Balance Column Implementation Plan

## Scope

Expose the already calculated total account balance in the Accounts list. The implementation remains presentation-only: it reuses `AccountBalanceDto.Balance` and does not modify account calculations, API contracts, persistence, filters, or account movement history.

## Implementation Steps

1. Add a localized `Current balance` table-header resource.
   - Update `src/FamilyFinances.Web/Resources/SharedResource.resx`, `SharedResource.en-US.resx`, and `SharedResource.es-ES.resx` with the same resource key and their respective localized values.
   - Reuse the existing resource naming convention for Accounts table columns.

2. Render the total balance in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`.
   - Add a right-aligned `Current balance` header between `Accumulated balance` and `Status` in every account-nature table.
   - Add a display helper that reads `AccountBalanceDto.Balance` from the existing `_balances` dictionary and formats it with `MoneyFormatter.FormatEuros`.
   - Render the formatted value with the same numeric emphasis as the two existing balance columns; render the existing muted em dash when the account has no balance payload.
   - Keep all account actions and existing column semantics unchanged.

3. Extend `tests/FamilyFinances.Web.Tests/Features/Accounts/AccountsListPageTests.cs`.
   - Update the current balance-column test to assert the localized header, its ordering after accumulated balance, and the rendered total-balance value.
   - Assert the row-cell positions for current-month balance, accumulated balance, and total current balance so the three figures cannot be accidentally swapped or hidden.
   - Preserve existing tests for accounts that have no returned balance, ensuring the new column uses the em dash fallback.

4. Verify the presentation-only change.
   - Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj --no-restore --filter "FullyQualifiedName~AccountsListPageTests"`.
   - Run `dotnet build FamilyFinances.sln --no-restore`.
   - Run `dotnet test FamilyFinances.sln --no-restore`.
   - Run `git diff --check`.

## Non-Goals

- Do not add an endpoint or modify `AccountBalanceDto`.
- Do not redefine the meanings of current-month or year-to-date balances.
- Do not add sorting, filtering, exports, or navigation based on total current balance.
- Do not modify OpenAPI, because the existing API contract already includes the required value.
