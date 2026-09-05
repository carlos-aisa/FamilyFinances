# Account Group Dashboard Pin Card Indicator Implementation Plan

## Scope

Add a localized, text-bearing dashboard-pin status badge to account-group list cards only when the group is pinned. Reuse the existing `AccountGroupDto.IsDashboardPinned` value and the established Bootstrap semantic classes.

## Steps

1. Update `src/FamilyFinances.Web/Components/Pages/AccountGroups/AccountGroupsListPage.razor`.
   - Render a compact pin-icon badge beside the group name only when `group.IsDashboardPinned` is true.
   - Use a shared localized label and semantic CSS classes; do not add inline styling, API calls, state, controls, or navigation behavior.
   - Preserve the existing responsive card layout and accessible, text-bearing status.

2. Add the English and Spanish resource key in the shared `.resx` files.
   - Keep the default resource value aligned with English.

3. Add a focused bUnit component test under `tests/FamilyFinances.Web.Tests/Features/AccountGroups/`.
   - Verify that a pinned list item renders the badge and its label.
   - Verify that an unpinned list item has no badge.
   - Mock only the existing API service boundary and retain normal authorization/loading setup used by nearby page tests.

4. Amend `openspec/changes/dashboard-kind-pinned-groups/`.
   - Record the card-level visibility refinement in proposal, design, and a completed follow-up task.

## Non-Goals

- No API, DTO, persistence, migration, or dashboard-query change.
- No pin toggle on the list card.
- No new filtering, sorting, or placeholder badge for unpinned groups.

## Validation

Run the focused Web test class, `dotnet build FamilyFinances.sln -c Release --no-restore`, and the Web test project in Release configuration. Run `openspec validate dashboard-kind-pinned-groups --strict` after the documentation amendment.
