## Context

Reporting already returns graph-ready monthly points for annual scopes, but the UI still uses a chart placeholder. Users need annual trend visibility directly in report pages without losing table detail.

`0.9.3` targets annual visualizations only and should use existing report data contracts wherever possible.

## Goals / Non-Goals

**Goals:**
- Render annual charts above report tables in relevant pages.
- Provide clear visualizations for balance evolution, group evolution, and income/expense composition.
- Maintain deterministic chart-to-table consistency.

**Non-Goals:**
- Daily/month-level visualizations and overlays.
- Forecasting and anomaly analysis.
- Rewriting reporting query handlers.

## Decisions

### Decision 1: Build reusable chart wrapper components in Web layer
- **Choice:** Introduce reusable Blazor chart wrappers (line, stacked area, 100% stacked bar, pie/donut fallback) under reporting components.
- **Rationale:** limits duplication and prepares for `0.9.4`.
- **Alternative considered:** hardcode chart markup in each page.
  - **Rejected because:** poor maintainability and inconsistent behavior.

### Decision 2: Keep table as canonical source and chart as derivative projection
- **Choice:** Compute chart datasets from the same DTO/table data used for rendering.
- **Rationale:** avoids discrepancies and extra API calls.
- **Alternative considered:** separate chart-specific endpoint.
  - **Rejected because:** unnecessary contract expansion for `0.9.3`.

### Decision 3: Scope chart rendering by report tab and data availability
- **Choice:** show/hide chart sections depending on selected scope and available series.
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
2. Replace chart placeholders in annual report views with real chart panels.
3. Bind annual data series for requested chart types.
4. Add fallback empty/loading/error states.
5. Add/extend web tests validating chart presence and value consistency.

### Rollback

- Restore placeholder chart sections and disable new chart wrappers.
- Keep existing report tables and APIs unchanged.

## Open Questions

- Which chart library should be primary for Blazor (`ChartJs.Blazor` vs JS interop wrapper) under current bundle constraints?
- Should composition charts default to donut or 100% stacked bars for readability?
