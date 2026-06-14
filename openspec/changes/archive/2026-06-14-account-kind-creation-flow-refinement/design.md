## Context

The current Accounts page mixes two distinct concerns in the same creation surface:

- primary account creation,
- low-frequency custom kind administration.

This was acceptable while validating the hybrid kind catalog foundation, but the resulting UI now feels heavier than the task it serves. The real user need is narrower: keep `Kind` selection present during account creation, provide a fast escape hatch when the right kind does not exist, and move full catalog maintenance behind a secondary entry point.

The implementation must stay inside the current Blazor Accounts feature patterns, reuse the existing kind catalog APIs, and avoid introducing new architectural layers or backend contracts.

## Goals / Non-Goals

**Goals:**
- Keep `Kind` explicitly selectable during account creation.
- Let users create a missing custom kind directly from the account creation form.
- Keep full kind administration accessible from the Accounts area without embedding it in the main creation form.
- Improve visual hierarchy so auxiliary kind actions do not compete with `Create account`.
- Reuse as much existing kind filtering/defaulting behavior as possible.

**Non-Goals:**
- Redesign navigation outside the Accounts area.
- Change kind catalog persistence, API contracts, or compatibility semantics.
- Introduce automatic kind inference beyond the current default-per-nature behavior.
- Add new backend workflows for rename or semantic automation of kinds.

## Decisions

### Decision 1: Split contextual kind creation from full kind administration

The main account creation form will keep only contextual kind tasks:

- select a compatible kind,
- create a missing compatible kind inline.

Full catalog operations such as browsing all custom kinds, enabling/disabling, and deleting will move to a secondary Accounts management surface.

Why:
- Most account creations only need selection, not catalog maintenance.
- Keeping low-frequency administration always visible dilutes the main flow.
- The user still wants both entry points, but not with the same visual weight.

Alternative considered:
- Keep all kind management embedded in the create form.
- Rejected because it preserves the current visual clutter and keeps a rare task in the hottest path.

### Decision 2: Inline kind creation inherits the current account nature automatically

When a user creates a custom kind from account creation, the inline create form will ask only for the name. The kind `Nature` will be inherited from the current account `Nature`.

Why:
- The account form already establishes the semantic context.
- Asking for `Nature` again adds redundant UI and opens the door to mismatched combinations.
- This matches the user expectation that the inline path is a quick fix when no compatible kind exists.

Alternative considered:
- Let users choose a different `Nature` while creating the kind inline.
- Rejected because it adds noise and weakens the contextual nature of the action.

### Decision 3: Reuse a selector-plus-inline-create interaction model

The implementation should move toward a reusable `KindSelector`-style component, conceptually aligned with the existing payee selector pattern:

- select existing option,
- reveal compact inline create form on demand,
- auto-select the newly created item on success.

Why:
- The interaction is already familiar in the product.
- It keeps the Accounts creation form smaller and more declarative.
- Shared behavior reduces drift if account creation remains available in multiple surfaces.

Alternative considered:
- Keep all kind logic embedded directly in `AccountsListPage.razor`.
- Rejected because it would preserve the current coupling between form layout and catalog-management logic.

### Decision 4: Full management remains inside Accounts as a secondary page-level action

The Accounts feature will expose a separate `Manage kinds` entry point at page level rather than moving kind management to global settings.

Why:
- Users conceptualize kinds as part of account configuration, not global administration.
- The user explicitly preferred keeping management close to Accounts.
- This avoids unnecessary navigation reorganization.

Alternative considered:
- Move management to Settings or first-run setup only.
- Rejected because kinds can still evolve later and need a discoverable home.

### Decision 5: Visual hierarchy must make kind actions feel auxiliary

The interaction will treat kind-related creation actions as low-emphasis utilities:

- `Create account` remains the dominant action,
- `Cancel` remains secondary,
- `New kind` is compact and attached to the kind field,
- `Manage kinds` is visible but not dominant at page level.

Why:
- The problem is not missing capability, but excessive button prominence.
- The Accounts page already carries multiple actions; added controls must not flatten hierarchy.

Alternative considered:
- Add more full-sized outline buttons in the form row.
- Rejected because it makes the form look busier without increasing clarity.

## Risks / Trade-offs

- [Risk] Inline kind creation and full management can drift in behavior. → Mitigation: reuse the same kind-loading and label-resolution paths, and centralize selector behavior where practical.
- [Risk] Hiding full management from the form could reduce discoverability. → Mitigation: add a clear page-level `Manage kinds` action inside Accounts.
- [Risk] Nature changes can invalidate the selected kind in subtle ways. → Mitigation: preserve the current default-per-nature fallback and cover invalidation with web tests.
- [Risk] A reusable selector component introduces a small refactor in a feature area already carrying page logic. → Mitigation: keep the component narrow and focused on selection plus inline create only.

## Migration Plan

1. Introduce the OpenSpec requirements for the new Accounts kind-management experience and the refinement to the kind catalog capability.
2. Refactor the Accounts creation form so the full custom-kind management block is removed from the primary form area.
3. Add the contextual inline custom-kind creation path bound to the current account `Nature`.
4. Add the secondary `Manage kinds` surface inside Accounts for low-frequency catalog administration.
5. Update web tests to cover the new flow split, nature-filtered selection, inline create success/failure, and secondary management entry.
6. If the refined UX fails in practice, rollback is limited to reverting the Accounts web interaction and related tests because no backend contract changes are involved.

## Open Questions

- Whether the secondary management surface should be rendered inline as an expandable panel or as a dedicated Accounts sub-route can be finalized during implementation, as long as it remains a secondary Accounts entry and not part of the main creation form.
