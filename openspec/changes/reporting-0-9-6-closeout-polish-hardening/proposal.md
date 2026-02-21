## Why

To declare `0.9` complete, reporting must be production-ready beyond features: UX polish, stability, performance, export options, and verification quality gates are required. This final phase closes remaining gaps and establishes the "done" bar.

## What Changes

- Perform end-to-end hardening of reporting UX, performance, and reliability.
- Add export capabilities for reporting outputs (CSV for tabular data, chart image export where applicable).
- Add accessibility and responsive behavior checks for report pages.
- Add final regression/E2E validation matrix and release closeout checklist.

### Non-Goals

- No new business metrics or additional report types.
- No architectural rewrites.
- No major visual redesign after closeout acceptance.

### Rollback Plan

- Keep hardening and export features additive; disable export actions if regressions occur.
- Revert specific polish/perf changes behind feature flags/toggles where feasible.
- Keep core reporting data and endpoints unchanged during rollback.

## Capabilities

### New Capabilities
- `reporting-closeout-hardening`: Final quality, exportability, accessibility, and release-gate behavior for reporting.

### Modified Capabilities
- `system`: Extend baseline with reporting release-readiness requirements for performance, UX reliability, and regression safety.

## Impact

- Web report components, chart wrappers, and export action handlers.
- CI/test pipelines (broader regression and optional E2E suites).
- Documentation and release process artifacts for `0.9` closure sign-off.
