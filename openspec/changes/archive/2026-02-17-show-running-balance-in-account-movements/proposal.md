## Why

Account movements currently show individual movement amounts but do not visibly present balance evolution per row. Users need to see the running account balance directly in the movements list to understand how each transaction changes the account over time.

## What Changes

- Add visible running-balance presentation to the account movements list UI.
- Use the already-available `RunningBalance` field in `AccountMovementDto` for each displayed movement row.
- Add a dedicated table column for running balance and apply sign-based visual styling for quick scanning.
- Keep API contracts unchanged (no endpoint, DTO, or query parameter changes).

### Non-goals

- No changes to movements retrieval logic in backend repositories.
- No changes to account balance calculation algorithms.
- No pagination behavior changes.
- No changes to auth, routes, or API response formats.

### Rollback Plan

- Revert UI changes in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor`.
- Keep backend unchanged (rollback is frontend-only for this change).

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `system`: Account movements view requirement is updated to include a visible running balance per movement row.

## Impact

- Affected UI: `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor`
- Affected documentation artifacts: this OpenSpec change set (`proposal`, `design`, `specs`, `tasks`)
- No API/OpenAPI impact.
- No database or migration impact.
