## Context

Account kind is currently modeled as a fixed enum persisted as numeric identity. This is simple but rigid: adding categories requires code changes, repeated label switch updates, and test rewrites across layers. The current product need is to support more categories now and allow future kind-based filters/business logic without reworking the model again.

Current constraints:
- Global scope only (no tenant/workspace partitioning).
- Layered architecture and EF Core migration discipline are mandatory.
- Existing account creation/search UX must remain stable.
- Bank import workflows are explicitly out of scope in this change.

## Goals / Non-Goals

**Goals:**
- Introduce a global unified kind catalog that supports both predefined and user-defined kinds.
- Move account persistence/API semantics from enum-kind identity to catalog-kind identity.
- Expose a single combined kind list in UI surfaces where account kind is selected or rendered.
- Introduce stable kind keys to preserve future compatibility for filters/logic.
- Keep behavior parity for current account flows while enabling category extensibility.

**Non-Goals:**
- No bank import implementation, mapping rules, or movement auto-classification.
- No multi-tenant kind scoping.
- No redesign of unrelated reporting/account pages.
- No rollout of kind-based business rules in this iteration.

## Decisions

### Decision 1: Replace enum persistence with catalog-backed kind reference
- Accounts will reference a kind catalog row (`KindId`) instead of storing enum values directly.
- Catalog entries include immutable semantic key and mutable display name.
- Rationale: enables extensibility and future semantic logic while avoiding text-coupled behavior.
- Alternative considered: keep enum and add more values. Rejected because it does not solve long-term flexibility and keeps duplicated label logic.

### Decision 2: Keep predefined kinds as seeded catalog entries
- Existing predefined kinds remain available, now represented as system entries in the catalog.
- Seed data is deterministic and migration-safe.
- Rationale: preserves familiar baseline UX while enabling user extension.
- Alternative considered: custom-only model. Rejected because it loses standardized baseline semantics.

### Decision 3: Use a single combined list in account-kind UX
- Kind selectors and relevant labels will consume one merged source (system + custom).
- UI may visually distinguish source, but selection semantics remain identical.
- Rationale: aligns with product expectation and avoids fragmented UX.
- Alternative considered: separate selectors for system/custom. Rejected because it adds friction and cognitive load.

### Decision 3.1: Bind custom kinds to account nature
- Custom kinds store a required `Nature` and are only selectable for accounts of the same nature.
- System kinds retain legacy compatibility behavior and continue to be available according to existing nature rules.
- Rationale: prevents semantic drift and prepares deterministic future filters.
- Alternative considered: nature-agnostic custom kinds. Rejected because it allows invalid cross-nature combinations and weakens future logic.

### Decision 4: Introduce key governance for future logic compatibility
- Catalog `Key` is mandatory and unique.
- System keys are immutable; custom keys are generated from normalized name and remain unique.
- Logic/features in future changes must target key/id, not display name.
- Rationale: supports stable references when names are localized or renamed.
- Alternative considered: name-only identity. Rejected because renames would break rule/filter semantics.

### Decision 5: Migrate API account contracts to catalog semantics in one controlled step
- Account create/update/read contracts expose catalog-backed kind identity (id/key/name surface as required by API shape decision in implementation).
- Transition is handled in one migration-focused release with explicit test updates.
- Rationale: avoids prolonged dual-contract complexity and hidden backward-compatibility debt.
- Alternative considered: temporary dual enum+catalog contract. Rejected to reduce migration complexity horizon and maintenance overhead.

### Decision 6: Support editing kind on existing accounts
- Accounts lifecycle exposes an explicit account-kind update operation for existing records.
- Update flow validates: selected kind exists, is active, and is compatible with account nature.
- Rationale: allows catalog normalization after account creation and supports evolving classification needs.
- Alternative considered: create-time-only kind assignment. Rejected because it blocks expected maintenance workflows.

### Decision 7: Allow deletion of custom kinds only when unused
- Only non-system kinds can be deleted.
- Delete flow validates that the target kind is not referenced by any account before deletion.
- Rationale: keeps catalog clean while preserving referential safety and historical account integrity.
- Alternative considered: soft-delete only. Rejected for now because inactive+delete split supports both hide and cleanup workflows.

### Decision 8: Order account-kind selectors by visible label
- Selector options are ordered by localized display label in UI.
- Rationale: users discover kinds faster when option order matches what they read in their active language.
- Alternative considered: raw `SortOrder`/database name ordering. Rejected because it can feel inconsistent after localization/custom naming.

## Risks / Trade-offs

- [Data migration mismatch between historical enum values and seeded catalog rows] -> Mitigation: deterministic enum-to-key mapping table in migration and integration tests validating all known enum values.
- [UI regressions from replacing local switch label resolvers] -> Mitigation: centralize kind display resolver and add focused web tests for account forms, quick entry search, and account listing labels.
- [API contract churn for clients reading `Kind`] -> Mitigation: document contract changes in OpenSpec/API docs and update internal consumers in same change.
- [Custom kind key collisions] -> Mitigation: enforce unique index and deterministic collision suffix strategy.
- [Nature mismatch between accounts and custom kinds] -> Mitigation: enforce compatibility checks in create and set-kind handlers plus integration tests.
- [Accidental deletion of in-use kinds] -> Mitigation: enforce server-side in-use guard and surface deterministic validation errors.

## Migration Plan

1. Add new kind catalog persistence model and EF migration.
2. Seed predefined kinds with stable keys and deterministic ordering.
3. Backfill account `KindId` from existing enum values using migration mapping.
4. Update domain/application/API contracts to consume catalog-based kind identity.
5. Replace UI enum label switches with catalog-driven resolver/services.
6. Add minimal management flow for global custom kind creation/activation lifecycle.
7. Bind custom kinds to a required nature and backfill existing data in migration.
8. Add account kind update operation for existing accounts and wire UI/API flows.
9. Run full test suite tiers affected by model and contract changes.
10. Validate OpenSpec strict and update related docs.

Rollback strategy:
- Revert deployment to pre-migration version.
- Execute rollback migration restoring enum-backed account kind storage.
- Re-run account CRUD and quick-entry regression tests.

## Open Questions

- Should custom kind rename preserve key or allow optional key edit under strict constraints?
- Should deactivating a custom kind be blocked when referenced by existing accounts, or allowed with selection-only restrictions?
- Should API expose both `KindId` and `KindKey` in every account payload, or only one canonical identity plus display fields?
