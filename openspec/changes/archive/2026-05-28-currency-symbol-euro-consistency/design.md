## Context

FamilyFinances is a single-currency (EUR) ledger, but some Web UI listing surfaces currently render amounts with "$" due to currency formatting paths that depend on default culture currency identity or hardcoded literals. This causes semantic inconsistency: amounts represent EUR in data and domain logic, but presentation can look like USD.

This change is cross-cutting at presentation level because list-like views exist in transactions, account movements, and reporting tables. The architecture constraint is to keep domain/application semantics unchanged and implement the correction in presentation formatting and localization behavior.

## Goals / Non-Goals

**Goals:**
- Enforce a deterministic EUR-only symbol policy for user-facing monetary rendering.
- Preserve culture-driven numeric conventions (decimal/group separators) while fixing currency identity to EUR.
- Remove "$" rendering paths from listing surfaces.
- Provide regression-safe automated tests for representative list/table views.
- Keep API and persistence contracts stable unless a preformatted display field forces minimal adaptation.

**Non-Goals:**
- Multi-currency support, conversion rates, or currency configuration per user.
- Rework of balance calculations, ledger sign semantics, or transaction storage models.
- UI redesign of pages unrelated to amount symbol semantics.
- Changes to endpoint topology or versioning.

## Decisions

### Decision 1: Centralize UI amount rendering behind one EUR-aware formatter
- Choice: Ensure list/table amount rendering flows through shared formatting helpers that emit EUR symbol semantics.
- Rationale: A single rendering policy prevents symbol drift and reduces duplicate fixes.
- Alternatives considered:
  - Per-page hardcoded replacement of "$" with "€": rejected due to fragility and regression risk.
  - Backend preformatted currency strings for all views: rejected because formatting belongs to UI culture/presentation concerns.

### Decision 2: Keep culture formatting for numeric shape, force EUR identity
- Choice: Number/date localization remains driven by active culture, but money symbol identity is always EUR.
- Rationale: Aligns with user requirement (all app in euros) while preserving localization readability.
- Alternatives considered:
  - Fully culture-native currency symbol (existing behavior): rejected because it can emit "$".
  - Fully fixed invariant formatting independent of culture: rejected because it degrades localization UX.

### Decision 3: Define representative surface coverage for validation
- Choice: Validate at least transactions list, account movements list, and one reporting table family for EUR symbol semantics.
- Rationale: These are high-traffic list/table surfaces where inconsistencies are most visible.
- Alternatives considered:
  - Exhaustive page-by-page first pass before merge: rejected as too slow for a focused consistency fix.

### Decision 4: No domain or persistence changes
- Choice: Keep Money/domain storage untouched; fix presentation pathways only.
- Rationale: Problem is display inconsistency, not financial model correctness.
- Alternatives considered:
  - Introduce currency metadata in entities/DTOs: rejected as scope expansion without user need.

## Risks / Trade-offs

- [Risk] Existing localization expectation says currency follows selected culture, which may conflict with EUR-only symbol policy.
  → Mitigation: Modify localization spec requirement to explicitly preserve culture separators while forcing EUR symbol identity.

- [Risk] Hidden formatters in niche pages may remain unfixed.
  → Mitigation: Add grep/static review in implementation tasks and extend tests to representative report/list surfaces.

- [Risk] Some tests may assert culture-native symbol and fail.
  → Mitigation: Update assertions to EUR symbol contract and keep culture-specific separator checks.

- [Risk] Over-correction could replace non-monetary "$" text in unrelated contexts.
  → Mitigation: Scope implementation to monetary formatting pathways and money-bound UI components only.

## Migration Plan

1. Update shared monetary formatting helpers and localization contract in Web UI.
2. Update list/table components in transactions, account movements, and reporting surfaces that bypass shared helper.
3. Add/adjust automated tests validating EUR symbol rendering and separator behavior.
4. Run focused Web and API integration suites, then full solution tests.
5. Ship as backward-compatible behavioral consistency update.

Rollback:
1. Revert formatter and component-level patches introduced by this change.
2. Re-run focused rendering tests to confirm return to prior baseline.
3. If rollback is necessary after release, document temporary inconsistency in release notes and keep issue open for follow-up patch.

## Open Questions

- Should exports (CSV/PDF-like outputs) also force EUR symbol textual representation, or keep numeric-only values where currently implemented?
- Should settings UI expose a read-only statement that account currency is fixed to EUR to reduce future ambiguity?
