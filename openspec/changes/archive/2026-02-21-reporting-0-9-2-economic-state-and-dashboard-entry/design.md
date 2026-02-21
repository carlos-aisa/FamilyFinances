## Context

Users currently need to combine data from multiple reports to understand their current financial position. This causes navigation friction and increases interpretation risk. `0.9.2` introduces a single "Economic State" view and a navigation-menu shortcut so users can access it in one click.

The change is cross-cutting because it touches:
- reporting application/read model composition
- API reporting surface
- reports index and report page navigation
- shared navigation menu entry point

## Goals / Non-Goals

**Goals:**
- Deliver a single report page that answers current-state questions at a selected as-of date (default: today).
- Show explicit KPIs for `Assets`, `Liabilities`, `Net Worth`, `Income (period)`, `Expense (period)`, `Period Net Result`.
- Add direct navigation from menu to this report.
- Keep semantic naming aligned with `0.9.1`.

**Non-Goals:**
- Large chart suite (handled by `0.9.3` and `0.9.4`).
- Replacing existing monthly summary or monthly evolution reports.
- Data model migration or ledger write-path changes.

## Decisions

### Decision 1: Introduce a dedicated Economic State API read model
- **Choice:** Add one dedicated endpoint and DTO for economic-state composition.
- **Rationale:** Keeps one-request data loading for dashboard/report UI and centralizes formula ownership in backend.
- **Alternative considered:** Compose economic state in Web by calling multiple existing endpoints.
  - **Rejected because:** more network calls, duplicated formula logic, and potential inconsistencies.

### Decision 2: Default as-of date is current local date with explicit period preset
- **Choice:** The page opens at today and uses "current month-to-date" as the default flow period.
- **Rationale:** Matches user expectation for "how am I now?" while keeping period metrics interpretable.
- **Alternative considered:** Force users to pick dates before loading.
  - **Rejected because:** adds friction to primary use case.

### Decision 3: Sidebar shortcut is the primary quick entry, with compact asset preview
- **Choice:** Add an `Economic State` entry in navigation menu with `Asset Balance` as-of preview and CTA link to full report.
- **Rationale:** Keeps dashboard focused on account quick-entry workflows while exposing always-visible state info.
- **Alternative considered:** keep shortcut card in dashboard body.
  - **Rejected because:** reduced usable space for frequent account actions.

### Decision 4: Preserve existing report endpoints and pages
- **Choice:** Economic State is additive and does not replace current reports.
- **Rationale:** low-risk delivery in `0.9.x` and easier rollback.
- **Alternative considered:** merge Monthly Summary and Monthly Evolution into a single page immediately.
  - **Rejected because:** too broad for this phase.

## Risks / Trade-offs

- **[Risk] KPI formulas may be misunderstood if period range is not visible.**
  - **Mitigation:** show selected as-of date and flow period explicitly in header/filter area.
- **[Risk] New endpoint may duplicate logic from existing reporting handlers.**
  - **Mitigation:** extract shared calculation helpers in application reporting layer.
- **[Risk] Sidebar preview could become stale if not clearly labeled.**
  - **Mitigation:** show "as of" timestamp/date and link to full report details.
- **[Trade-off] Additive endpoint increases API surface area.**
  - **Mitigation:** keep endpoint narrow and aligned with canonical metric semantics.

## Migration Plan

1. Add economic-state query, handler, and DTO in application layer.
2. Expose `GET /api/v1/reports/economic-state` in Reports controller.
3. Add web API client method and new Blazor report page.
4. Add navigation-menu shortcut entry and reports index entry alignment.
5. Add tests across Application, API integration, and Web features.
6. Release as additive feature with no data migration.

### Rollback

- Disable report card/route from UI navigation.
- Remove endpoint wiring while leaving existing report endpoints untouched.
- Revert navigation shortcut entry and keep previous report navigation paths.

## Open Questions

- Should `Income/Expense/Period Net` always be month-to-date, or user-configurable in `0.9.2`?
- Should dashboard preview include only stock metrics for visual simplicity?
