## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not bundle unrelated visual redesign work into this change.
- Do not silently alter business formulas or financial aggregation semantics.
- Do not break existing reporting API contracts without compatibility strategy.
- Do not leave mixed semantics (`inclusive` and `exclusive`) active in parallel without explicit transition rules.

### Required
- Define one global date-range semantic contract for filters, with `end date inclusive` behavior.
- Remove unnecessary manual filter actions where reactive apply is safe and deterministic.
- Align labels, headers, and exported filter context with the new semantic contract.
- Cover all changed filter behaviors with automated tests and updated documentation.

## Why

Current filter behavior is inconsistent and confusing for users, especially around end-date exclusivity and when filters are actually applied. A dedicated semantic change is needed now so every view follows one predictable date-filter contract before technical UI normalization.

## What Changes

- Change global date-range semantics to `end date inclusive` for UI-driven range filtering.
- Replace or remove manual `Apply filters` and `Reset` actions where immediate/reactive apply is appropriate and safe.
- Standardize user-facing date labels and helper text to reflect inclusive semantics (remove exclusivity wording).
- Define deterministic trigger points for filter execution:
  - on preset selection,
  - on completed manual date entry/selection,
  - without accidental partial-input refresh loops.
- Align report exports and filter context metadata with the same inclusive interpretation.
- Introduce compatibility handling for backend/request boundaries where existing internals still use exclusive-end representations.

## Capabilities

### New Capabilities
- `global-date-filter-semantics`: Defines the canonical cross-view date-range semantics and filter-application trigger behavior.

### Modified Capabilities
- `system`: Shared filtering behavior and date-range interaction rules are updated across the Web app.
- `historical-ledger-views`: Historical browsing filters must follow the global inclusive-end contract and reactive-apply rules.
- `reporting-closeout-hardening`: Exported filter context and report export behavior must reflect inclusive end-date semantics consistently.

## Impact

- Affected frontend areas:
  - shared date preset components and filter UI controls,
  - report pages with from/to filtering,
  - ledger/history views with date-range filtering.
- Affected API integration boundaries:
  - request mapping for date range parameters where conversion between UI-inclusive and internal-exclusive representations may be required.
- Affected tests:
  - web interaction tests for filter behavior,
  - reporting API integration tests for range semantics,
  - export tests validating filter context metadata.
- Documentation updates required for user guidance and developer semantics conventions.

## Non-Goals

- No broad visual redesign or layout rewrite.
- No chart-style unification work.
- No app-wide tokenization/hardcode cleanup.
- No change to financial metric formulas or ledger posting rules.

## Rollback Plan

- Revert global date filter semantics to prior behavior behind a central compatibility switch if severe regressions appear.
- Restore previous manual filter controls (`Apply`/`Reset`) on affected views if reactive behavior causes instability.
- Keep export payload compatibility fallback for previous semantics during rollback window.
- Re-run web and integration tests after rollback to confirm restored baseline behavior.
