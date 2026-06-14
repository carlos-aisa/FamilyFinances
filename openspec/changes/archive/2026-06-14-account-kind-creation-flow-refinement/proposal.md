## Why

The current Accounts creation experience mixes account creation with full custom kind management, which adds noise to a low-frequency task and makes the primary flow harder to scan. This change is needed now to keep `Kind` selection available during account creation while moving heavy kind administration into a secondary management surface.

## What Changes

- Refine the Accounts creation flow so `Kind` remains selectable for the current account `Nature`.
- Add a lightweight inline `New kind` path inside account creation for the case where no existing kind fits.
- Move full kind administration out of the primary account creation form into a secondary Accounts management surface.
- Establish explicit visual hierarchy rules so new kind-related actions do not compete with the main `Create account` action.
- Preserve compatibility with the current kind catalog contracts and reuse the existing kind catalog APIs.

## Capabilities

### New Capabilities
- `accounts-kind-management-experience`: Defines the Accounts UI behavior for selecting kinds during account creation, creating missing kinds inline, and opening a separate full management surface for low-frequency kind administration.

### Modified Capabilities
- `account-kind-catalog`: Clarify that custom kind creation must remain available from account creation flows while full catalog administration can live in a separate management surface.

## Impact

- Affected web feature area: `src/FamilyFinances.Web/Components/Pages/Accounts/*`
- Likely affected shared UI area: `src/FamilyFinances.Web/Components/Shared/*`
- Affected web tests: `tests/FamilyFinances.Web.Tests/Features/Accounts/*`
- No API contract changes are expected.
- No data model or migration changes are expected.

## Non-Goals

- Do not redesign global navigation outside the Accounts area.
- Do not change kind catalog persistence, identifiers, or compatibility rules.
- Do not introduce automatic kind inference beyond the current default-per-nature behavior.
- Do not change reconciliation, import, or future kind-based automation flows.

## Release Impact

Type: minor
Rationale: This introduces a user-visible refinement to the Accounts workflow while staying backward compatible at the API and data levels.

## Rollback Plan

- Revert the Accounts UI back to the current embedded management layout if the refined interaction proves too fragmented in practice.
- Keep kind catalog APIs and persistence untouched so rollback remains limited to web behavior and related tests.
