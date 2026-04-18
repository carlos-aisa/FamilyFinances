## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not change ledger sign conventions or account balance formulas.
- Do not break or replace the existing `GET /api/v1/accounts/{id}/movements` contract.
- Do not fetch the entire filtered movement set in the Web layer as a workaround.
- Do not introduce database schema changes or EF Core migrations for this change.

### Required
- Running balance values shown in account movements MUST remain ledger-correct even when results are paginated.
- Users MUST be able to navigate beyond the first 50 rows from `/accounts/{id}/movements`.
- Date/search filter changes MUST reset pagination deterministically to page 1.
- Add API integration and Web UI tests covering multi-page browsing and running-balance correctness.

## Why

Account movements currently expose only the first 50 rows in the account view, with no way to navigate older rows. This creates user-visible mismatch when validating accumulated balances for early dates in a busy period because the missing movements are not reachable from the same screen.

## What Changes

- Add page navigation controls to account movements (`previous`/`next`) using the existing server pagination contract (`page`, `pageSize`, `TotalCount`).
- Keep default page size at 50, but show clear range feedback (`showing X-Y of Z`) so users can verify how much data is visible.
- Reset paging to page 1 whenever filters change (date presets, manual date range, search query) so page state and filters remain consistent.
- Harden running-balance correctness requirements under pagination and add explicit automated regression coverage for multi-page movement datasets.
- Preserve existing movement row shape and existing API routes/auth behavior.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `system`: account movements requirements are expanded to require navigable pagination and running-balance correctness independent of visible-page limits.

## Impact

- Affected frontend:
  - `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor`
  - `src/FamilyFinances.Web/Resources/SharedResource*.resx` (new pagination labels)
- Affected backend verification:
  - `tests/FamilyFinances.Api.IntegrationTests/Ledger/Accounts/AccountMovementsApiTests.cs`
- Affected web tests:
  - `tests/FamilyFinances.Web.Tests/Features/Accounts/*` (new/updated tests for pagination behavior)
- No OpenAPI contract expansion required if existing paging query parameters remain unchanged.
- No database or migration impact.

## Non-Goals

- No page-size selector in this iteration (fixed 50 remains acceptable baseline).
- No redesign of movement-table layout beyond pagination affordances and count messaging.
- No changes to historical movements page behavior in this change.
- No changes to reconciliation behavior.

## Rollback Plan

- Revert account-movements page pagination UI/state changes.
- Keep backend movement endpoint behavior unchanged (contract-compatible rollback path).
- Re-run account movements API and Web UI tests to verify return to pre-change behavior.
