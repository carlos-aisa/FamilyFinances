# Account Kind Creation Flow Design (2026-06-14)

## Context

The current Accounts creation experience mixes two different responsibilities in the same surface:

- creating a new account,
- managing the full custom kind catalog.

Recent kind-catalog work introduced valid capabilities for custom kinds, but the current inline management block inside the Accounts page adds visual noise to a low-frequency task. The result is a heavier account-creation flow than needed and an inconsistent experience versus the dedicated account creation page.

## Goal

Keep `Kind` selection available during account creation, while moving full kind administration out of the primary creation flow.

Users must still be able to:

- select a compatible kind for the current account nature,
- create a missing kind directly from the account creation form,
- open a separate management surface for full kind administration.

## Scope

In scope:

- redesign the account creation interaction around `Kind`,
- define a lightweight inline custom-kind creation flow,
- define where full kind management lives in the Accounts area,
- define expected validation, error, and test behavior,
- refine action hierarchy so new buttons do not degrade the page visually.

Out of scope:

- backend contract changes for kind catalog APIs,
- automatic classification logic beyond current default-kind behavior,
- changes to reconciliation, imports, or future kind-based semantics,
- global navigation redesign outside the Accounts area.

## Chosen Approach

Recommended approach: lightweight inline creation plus secondary management entry.

- The account creation form keeps a visible `Kind` selector filtered by the selected `Nature`.
- A compact secondary action next to the selector allows creating a new custom kind inline when none of the existing options fit.
- A separate secondary page-level action inside Accounts opens the full kind-management surface.
- The full management surface keeps low-frequency actions such as enable, disable, delete, and reviewing the full custom kind list.

This preserves a fast path for the common case while keeping rare catalog maintenance out of the main form.

## Interaction Model

### Account creation form

The account creation form remains focused on:

- `Name`
- `Nature`
- `Kind`

Expected behavior:

- `Kind` is always selectable during account creation.
- Available kinds are filtered to those compatible with the selected `Nature`.
- When `Nature` changes, the kind list is recalculated.
- If the current selection becomes invalid after a nature change, the form selects the default valid kind for that nature.

### Inline kind creation

The `Kind` selector includes a compact adjacent action such as `+` or `New kind`.

When activated:

- a small inline creation block appears below or next to the selector,
- the block asks only for the new kind name,
- the new custom kind automatically inherits the current account `Nature`,
- successful creation refreshes the available kind list,
- the newly created kind becomes the selected kind for the account,
- the inline block closes after success.

If creation fails:

- the error stays local to the inline creation block,
- the main account form remains intact,
- the user can correct the name and retry without losing the rest of the account input.

### Full kind management

The Accounts page exposes a separate page-level secondary action such as `Manage kinds`.

This opens the dedicated management surface responsible for:

- listing custom kinds,
- showing nature association,
- enabling and disabling custom kinds,
- deleting custom kinds when allowed,
- handling low-frequency maintenance outside the account creation form.

The primary account creation form must not show the full management list by default.

## Component Structure

The design should move toward a reusable kind-selection component similar in interaction spirit to the existing payee selector pattern.

Suggested structure:

- reusable `KindSelector` component for account forms,
- internal support for filtered compatible kinds by nature,
- compact inline create state inside the selector component,
- separate `KindManagement` surface owned by the Accounts feature.

This separation keeps the main form cohesive and avoids embedding catalog-administration logic directly inside the creation form markup.

## Visual Direction

The visual direction should remain sober and utilitarian, consistent with the current Accounts UI.

Action hierarchy:

- `Create account` remains the primary action,
- `Cancel` remains a subdued secondary action,
- `New kind` is a compact auxiliary action attached to the kind field,
- `Manage kinds` is a page-level secondary action outside the form.

Visual rules:

- avoid multiple equal-weight outline buttons clustered inside the main form,
- avoid leaving management controls permanently visible in the creation area,
- prefer one clear primary action per zone,
- show advanced or low-frequency controls only when invoked,
- keep icon-only or low-emphasis secondary controls for auxiliary actions where appropriate.

The goal is to reduce noise without hiding the escape hatch when a missing kind blocks account creation.

## Error Handling

The interaction must distinguish between account-creation errors and kind-creation errors.

Rules:

- account validation errors belong to the main account form,
- inline kind creation errors belong to the inline kind block,
- loading or refresh failures for kind options should preserve the last valid account form state when possible,
- disabling or deleting kinds in the management surface must not silently invalidate the active selection in the account form; the form should recover to the default compatible kind if needed.

## Testing Strategy

Frontend coverage should verify:

- the account creation flow no longer shows full kind-management controls by default,
- the kind selector filters options correctly for each account nature,
- changing `Nature` recomputes valid kinds and applies a valid default when needed,
- inline kind creation creates a custom kind with the current account nature,
- successful inline creation refreshes the list and auto-selects the created kind,
- inline creation errors remain local and do not reset the account form,
- the secondary `Manage kinds` entry still exposes full catalog administration behavior.

Existing kind-management behavior should remain covered separately from the account creation happy path.

## Risks and Mitigation

- Risk: the Accounts page gains more buttons but still feels busier.
  - Mitigation: enforce strict visual hierarchy and keep auxiliary actions compact and contextual.
- Risk: inline creation duplicates logic already embedded elsewhere.
  - Mitigation: centralize selector behavior in a reusable component instead of repeating page-specific fragments.
- Risk: dedicated account creation page drifts from the Accounts list flow.
  - Mitigation: share the same selector interaction model anywhere account creation remains available.

## Success Criteria

- Creating an account remains a focused, low-noise workflow.
- Users can still choose a compatible kind without leaving the form.
- Users can create a missing custom kind directly from account creation in one short interaction.
- Full kind administration remains available from Accounts, but no longer competes with the primary creation flow.
- The visual hierarchy of new controls feels intentional rather than cluttered.
