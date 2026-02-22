## Context

After annual chart availability (`0.9.3`), users still need fine-grained month-level visibility: how balance evolves during the month and how account groups contribute to that movement. Existing report contracts are monthly-bucketed and need extension for month-level chart datasets, but the UX baseline is now integrated state-evolution tabs (not a standalone Monthly Evolution page).

## Goals / Non-Goals

**Goals:**
- Provide monthly (intra-month) chart datasets and UI in existing reporting tabs.
- Enable visual comparison between total asset balance evolution and account-group evolution for selected month.
- Keep behavior deterministic and aligned with table metrics where comparable.

**Non-Goals:**
- Predictive models, anomaly flags, and alerts.
- New report routes or dashboard cards for monthly charts.
- Replacing annual charts with monthly-only views.

## Decisions

### Decision 1: Add dedicated month-level chart query contract
- **Choice:** Introduce a dedicated reporting query/endpoint for month-level points.
- **Rationale:** monthly-bucket evolution endpoint is not sufficient for intra-month charting.
- **Alternative considered:** reconstruct daily points client-side from monthly aggregates.
  - **Rejected because:** impossible to derive accurate intra-month curve from monthly totals alone.

### Decision 2: Use daily points for monthly charts
- **Choice:** monthly charts consume daily end-balance (or deterministic daily bucket) points.
- **Rationale:** best readability and supports "balance vs groups" comparison.
- **Alternative considered:** weekly points.
  - **Rejected because:** loses detail for short volatility and user-requested granularity.

### Decision 3: Keep month selection embedded in reporting page controls
- **Choice:** focused month selector is integrated inside existing state-evolution tab panels:
  - `AssetTotalEvolutionPanel`
  - `AccountGroupStateEvolutionPanel`
- **Rationale:** consistent interaction model and lower navigation overhead.
- **Alternative considered:** separate monthly chart page.
  - **Rejected because:** route no longer exists in `0.9.3` flow and would fragment reporting UX.

## Risks / Trade-offs

- **[Risk] Daily aggregation may increase query cost.**
  - **Mitigation:** bounded month range, indexed date filters, and lightweight DTO payloads.
- **[Risk] Sparse activity days can create misleading flat lines.**
  - **Mitigation:** show carry-forward semantics and legend note.
- **[Trade-off] Added endpoint and DTOs increase API surface.**
  - **Mitigation:** keep contracts narrow, explicit, and aligned with `state-evolution` naming semantics.

## Migration Plan

1. Add month-level reporting DTO/query/handler and repository methods.
2. Expose API endpoint for monthly chart datasets.
3. Integrate month selector and monthly charts in web state-evolution tab panels.
4. Add tests for daily point correctness and chart refresh behavior.
5. Validate performance and fallback states with no-data months.

### Rollback

- Hide monthly chart sections and focused-month selector integration in state-evolution tabs.
- Remove month-level endpoint wiring if necessary.
- Keep annual chart and table behavior intact.

## Open Questions

- Should weekend/no-activity days always be shown as explicit carry-forward points?
- Should monthly chart scope default to `Asset Total` or follow last selected annual scope?
