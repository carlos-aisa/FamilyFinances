## Context

`0.9.1` to `0.9.5` deliver semantics, economic state, charts, and insights. The final release step must guarantee production readiness and user trust through hardening and explicit exit criteria.

This phase is cross-cutting across Web UX, tests, CI, and release documentation.

## Goals / Non-Goals

**Goals:**
- Define and implement release-quality hardening for reporting pages.
- Add export workflows (CSV and chart image where supported).
- Validate responsive behavior, accessibility, and performance baselines.
- Formalize release closeout checklist for `0.9`.

**Non-Goals:**
- New report concepts or additional analytics capabilities.
- Reworking data contracts introduced in previous `0.9.x` changes.
- Large dependency/platform shifts.

## Decisions

### Decision 1: Keep hardening scope test-driven with measurable gates
- **Choice:** define explicit pass/fail criteria (tests, perf thresholds, accessibility checks).
- **Rationale:** prevents subjective "looks good" closeout.
- **Alternative considered:** manual QA-only closeout.
  - **Rejected because:** inconsistent and hard to regress-protect.

### Decision 2: Export behavior as additive action controls per report
- **Choice:** add standardized export controls in report header/actions.
- **Rationale:** consistent UX and low coupling with report internals.
- **Alternative considered:** separate export center page.
  - **Rejected because:** unnecessary navigation overhead.

### Decision 3: Prioritize responsive/accessibility fixes in existing components
- **Choice:** refine current report components instead of replacing layouts.
- **Rationale:** lower risk for final release closure.
- **Alternative considered:** full visual refactor.
  - **Rejected because:** high regression risk near release closure.

## Risks / Trade-offs

- **[Risk] Export generation may be inconsistent across browsers.**
  - **Mitigation:** constrain supported export formats and add browser-agnostic tests where possible.
- **[Risk] Performance regressions from chart-heavy pages.**
  - **Mitigation:** performance budgets and selective lazy rendering.
- **[Trade-off] Additional test runtime in CI.**
  - **Mitigation:** split fast regression suite and optional extended suite.

## Migration Plan

1. Define closeout acceptance checklist and measurable quality gates.
2. Implement export actions and wire to report contexts.
3. Apply responsive and accessibility fixes across report pages.
4. Add/extend regression and E2E checks for key reporting workflows.
5. Run full validation and publish release closeout notes.

### Rollback

- Disable export controls and revert latest hardening patches selectively.
- Keep validated core reporting features from earlier `0.9.x` releases.
- Re-run minimal regression suite before re-release.

## Open Questions

- Which browser matrix is mandatory for chart image export acceptance?
- Do we require Playwright/Cypress E2E in CI for final closeout or nightly only?
