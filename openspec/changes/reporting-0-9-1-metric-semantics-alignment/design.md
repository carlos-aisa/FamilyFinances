## Context

`MonthlySummary`, `MonthlyEvolution`, and `AssetTotalBalance` currently provide useful aggregates but with mixed semantic framing in the UI. Users compare cards and table values across pages as if all metrics represent the same business concept. In practice:

- `MonthlySummary.Net` is period flow (`Income - Expense`) for a date range.
- `MonthlyEvolution` can show stock evolution (`EndBalance`) by account, asset total, or group.
- Accounts scope can include double-entry offsets across natures, producing totals that do not match a user expectation of "money I have".

The `0.9.x` reporting roadmap adds charts and dashboard shortcuts. If metric semantics remain implicit, chart interpretation will be inconsistent and regressions will multiply.

## Goals / Non-Goals

**Goals:**
- Define one canonical metric dictionary for reporting (`Asset Balance`, `Liability Balance`, `Net Worth`, `Income`, `Expense`, `Period Net Result`, deltas).
- Enforce explicit mapping of every KPI card and chart series to one canonical metric definition.
- Standardize labels and disclaimers where metric comparison is not equivalent.
- Add deterministic tests that protect semantic contracts across reports.

**Non-Goals:**
- Implementing the full chart rendering stack.
- Rewriting report query handlers with new aggregation algorithms.
- Adding new DB tables or migration scripts.
- Introducing localization changes beyond current language behavior.

## Decisions

### Decision 1: Introduce a reporting semantic dictionary in Application layer
- **Choice:** Add a central semantic contract (enum/value object + helper map) in Application/Reporting that defines formulas and human-readable intent.
- **Rationale:** Keeps semantic definitions close to report DTOs without coupling to Blazor presentation.
- **Alternative considered:** Keep definitions only in Web labels.
  - **Rejected because:** UI-only definitions are hard to verify and can drift from API meaning.

### Decision 2: Preserve existing endpoints and DTO numeric payloads
- **Choice:** Keep current routes and numeric value fields; adjust naming/metadata/usage and tests.
- **Rationale:** Avoids breaking consumers and allows rollout during `0.9.x`.
- **Alternative considered:** Introduce new endpoints (`/reports/economic-state`, `/reports/metrics`) immediately.
  - **Rejected because:** scope for `0.9.1` is semantic alignment, not capability expansion.

### Decision 3: In `MonthlyEvolution` Accounts scope, summary cards represent asset-only stock by default
- **Choice:** For top summary cards in Accounts scope, aggregate only AccountNature `Asset` series when available; labels must explicitly say `Asset`.
- **Rationale:** Matches user expectation of "current money" and avoids ledger netting artifacts.
- **Alternative considered:** Sum all account natures in the summary.
  - **Rejected because:** double-entry offsets often result in misleading near-zero totals.

### Decision 4: Add explicit comparability disclaimers in reports index/detail pages
- **Choice:** When metrics are not directly comparable (stock vs flow), show explicit informational disclaimer text.
- **Rationale:** Prevents analytical misuse while preserving useful metrics.
- **Alternative considered:** Hide non-comparable metrics.
  - **Rejected because:** removes valuable information and reduces transparency.

### Decision 5: Add semantic consistency tests at Web and Application levels
- **Choice:** Add tests asserting formula identity and label-to-metric mapping.
- **Rationale:** Semantic regressions are subtle; deterministic tests provide low-cost protection.
- **Alternative considered:** Rely on manual QA.
  - **Rejected because:** high risk of drift when multiple report pages evolve in parallel.

## Risks / Trade-offs

- **[Risk] Users might expect liabilities to be visible in summary cards even when "Asset" is shown.**
  - **Mitigation:** explicit label (`Latest Asset ...`) and tooltip/disclaimer explaining scope.
- **[Risk] Existing tests may encode previous ambiguous labels.**
  - **Mitigation:** update test assertions to canonical terminology and include migration notes in PR.
- **[Risk] Minor UI copy changes can be perceived as behavior change.**
  - **Mitigation:** keep numeric formulas stable, only clarify names and scope.
- **[Trade-off] Backward compatibility on DTOs limits semantic metadata richness in `0.9.1`.**
  - **Mitigation:** reserve richer metadata for later `0.9.x` chart changes.

## Migration Plan

1. Add semantic dictionary and mapping helpers in reporting application layer.
2. Update report pages to use canonical labels and explicit disclaimers.
3. Align existing tests with canonical semantics and add new cross-report assertions.
4. Deploy without endpoint contract changes.
5. Monitor user feedback from `0.9.1` before introducing chart-heavy `0.9.3/0.9.4`.

### Rollback

- Revert semantic label mapping in Web pages to previous labels.
- Disable newly introduced semantic consistency tests.
- Keep existing endpoint behavior untouched (no data migration required).

## Open Questions

- Should `Net Worth` be shown in `0.9.1` cards if liabilities are available, or deferred to `0.9.2` Economic State page?
- Do we want a standardized tooltip component now, or plain info alerts until chart work starts?
