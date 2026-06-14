## Why

Account kind is currently a fixed enum used mostly for UX labels, which limits category growth and makes future semantic usage fragile. The product now needs a scalable kind model that supports both predefined and user-defined categories while remaining deterministic for future filters and business logic.

## What Changes

- Introduce a global unified account kind catalog that merges system-defined kinds and user-defined kinds.
- Replace direct enum persistence for account kind with a catalog reference model that supports stable semantic keys.
- Bind custom account kinds to a required `Nature` to keep selection and future filters deterministic.
- Keep current account behavior intact while allowing users to create and manage additional global kinds.
- Allow changing the kind of existing accounts with validation against kind lifecycle and nature compatibility.
- Allow deleting custom kinds only when they are not in use by existing accounts.
- Update account creation/edit/select UX to present a single combined kind list (predefined + custom).
- Ensure kind selectors are ordered by visible label to improve discoverability.
- Centralize kind label resolution to remove duplicated switch blocks and ensure consistent rendering across pages.
- Prepare data contracts for future kind-based filters/logic without implementing those filters yet.
- **BREAKING**: account kind storage and API contracts move from enum identity semantics to catalog identity semantics.

## Release Impact

Type: minor
Rationale: Introduces new backward-compatible functionality (custom kinds + unified catalog) with controlled contract/data migration.

## Capabilities

### New Capabilities
- `account-kind-catalog`: global kind catalog with predefined and user-defined entries, stable keys, lifecycle rules, and unified UI selection semantics.

### Modified Capabilities
- `system`: account API and persistence contracts are updated to use catalog-backed kind identity and migration-safe semantics.
- `quick-entry-workspace`: kind label/search behavior moves from enum switch labels to catalog-driven labels while preserving existing capture semantics.

## Non-Goals

- No bank-import workflow implementation in this change.
- No transaction auto-classification, mapping rules, or reconciliation-by-kind behavior.
- No tenant/workspace scoping for kinds (kinds remain global in this iteration).
- No redesign of unrelated account/reporting screens beyond required kind selection and label consistency updates.

## Rollback Plan

- Revert schema and application changes that introduce catalog-backed kind storage.
- Restore enum-based account kind persistence and enum-driven label rendering.
- Run EF migration rollback to previous stable migration and verify account CRUD/UI behavior via focused tests.
- Re-run OpenSpec validation and targeted web/API tests to confirm pre-change baseline behavior.

## Impact

- Domain/Application/Infrastructure: account kind model, account creation/update flows, EF configuration, seed data, migration path.
- API/Web: account DTO/request contract updates, kind catalog endpoints (if needed by UI), account forms and selectors.
- API/Web: account kind update endpoint for existing accounts and nature-bound custom kind creation.
- Tests: domain/application/API integration/web tests for kind catalog behavior and migration invariants.
- Documentation: specs and implementation docs covering kind catalog semantics and migration notes.
