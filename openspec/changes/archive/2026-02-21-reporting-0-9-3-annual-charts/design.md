## Context

Reporting already returns graph-ready monthly points for annual scopes, but the UI required richer annual visualizations integrated in daily report workflows.

`0.9.3` targets annual visualizations only and should use existing report data contracts wherever possible.

## Goals / Non-Goals

**Goals:**
- Render annual charts in the reporting tabs where users already work (`Economic State`, `Account Totals`, `Account Group Totals`).
- Provide clear visualizations for balance evolution, group evolution, and income/expense composition.
- Maintain deterministic chart-to-table consistency.

**Non-Goals:**
- Daily/month-level visualizations and overlays.
- Forecasting and anomaly analysis.
- Rewriting reporting query handlers.

## Decisions

### Decision 1: Build reusable chart wrapper components in Web layer
- **Choice:** Introduce reusable Blazor chart wrappers (line and composition) under reporting components.
- **Rationale:** limits duplication and prepares for `0.9.4`.
- **Alternative considered:** hardcode chart markup in each page.
  - **Rejected because:** poor maintainability and inconsistent behavior.

### Decision 2: Keep table as canonical source and chart as derivative projection
- **Choice:** Compute chart datasets from the same DTO/table data used for rendering.
- **Rationale:** avoids discrepancies and extra API calls.
- **Alternative considered:** separate chart-specific endpoint.
  - **Rejected because:** unnecessary contract expansion for `0.9.3`.

### Decision 3: Scope chart rendering by report tab and data availability
- **Choice:** show/hide chart sections depending on selected report tab, scope, and available series.
- **Rationale:** prevents empty or misleading visuals.
- **Alternative considered:** always render fixed chart shells.
  - **Rejected because:** noisy UX with sparse data.

## Risks / Trade-offs

- **[Risk] Chart and table values may drift if transformation logic diverges.**
  - **Mitigation:** centralized dataset adapter functions + test assertions against table values.
- **[Risk] Chart library performance on large series counts.**
  - **Mitigation:** limit initial rendering to annual monthly points and aggregate where needed.
- **[Trade-off] Additional frontend dependency complexity.**
  - **Mitigation:** keep abstraction isolated and minimal.

## Migration Plan

1. Add reusable chart components and dataset adapter helpers.
2. Replace report placeholders with real chart panels in integrated state-evolution tabs.
3. Bind annual data series for requested chart types.
4. Add fallback empty/loading/error states.
5. Add/extend web tests validating chart presence and value consistency.

### Rollback

- Restore placeholder chart sections and disable new chart wrappers.
- Keep existing report tables and APIs unchanged.

## Open Questions

- Primary chart runtime is `Chart.js` via JS interop wrappers.
- Composition defaults to pie (`quesitos`) for current UX readability.
