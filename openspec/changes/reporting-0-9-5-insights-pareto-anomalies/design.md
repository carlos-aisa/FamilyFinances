## Context

By `0.9.4`, users can inspect data visually, but interpretation remains manual. `0.9.5` introduces derived insights to highlight dominant groups/payees and detect unusual movements without replacing user control.

Insights must be deterministic, explainable, and aligned with existing report filters.

## Goals / Non-Goals

**Goals:**
- Provide Pareto-style ranking insights (top contributors to expense/income).
- Provide concentration indicators (share of top-N groups).
- Provide anomaly flags for unusual monthly group behavior with transparent threshold logic.
- Integrate insights in report pages without disrupting current workflows.

**Non-Goals:**
- Predictive forecasting and future trend estimation.
- Personalized advice engine.
- Complex probabilistic models requiring external infrastructure.

## Decisions

### Decision 1: Deterministic rule-based anomaly detection
- **Choice:** use deterministic thresholds (z-score / robust deviation or configurable percentile rules) with documented formulas.
- **Rationale:** explainable and testable behavior suitable for `0.9.x`.
- **Alternative considered:** ML-based anomaly classifiers.
  - **Rejected because:** high complexity and low explainability for current scope.

### Decision 2: Reuse existing report filters and periods for insight context
- **Choice:** insight APIs consume the same year/month/date filters as base reports.
- **Rationale:** avoids ambiguous interpretation and keeps UX consistent.
- **Alternative considered:** separate insight-only filters.
  - **Rejected because:** duplicate controls and user confusion.

### Decision 3: Insight panels are additive and collapsible
- **Choice:** display insights in compact cards/sections with optional expand details.
- **Rationale:** balances discoverability and visual density.
- **Alternative considered:** always-on detailed insight table.
  - **Rejected because:** cluttered report UI.

## Risks / Trade-offs

- **[Risk] False positive anomaly flags with sparse historical data.**
  - **Mitigation:** minimum-history guardrails and "insufficient history" state.
- **[Risk] Users may misread Pareto percentages without denominator context.**
  - **Mitigation:** always display total base amount and top-N coverage definition.
- **[Trade-off] Additional calculation cost for insights.**
  - **Mitigation:** bounded windows and server-side aggregation reuse.

## Migration Plan

1. Add insight DTOs and calculation services in Application reporting layer.
2. Add insight endpoints or endpoint extensions in reporting controller.
3. Integrate insight panels in relevant report pages.
4. Add deterministic tests for ranking, concentration, and anomaly logic.
5. Validate performance and no-history edge cases.

### Rollback

- Hide/disable insight sections in UI.
- Revert insight endpoints or contract extensions.
- Leave base report tables/charts unaffected.

## Open Questions

- Should anomaly threshold be globally fixed for `0.9.5` or configurable in settings?
- Should top-N default to 5 or be user-selectable in UI?
