## Context

The reports landing page (`/reports`) currently exposes report entries as a flat set of cards without explicit analytical grouping. Functional coverage has grown over multiple changes, but the index does not clearly communicate report families, relative usage intent, or complete route discoverability.

Current observed state:
- Existing report routes include `economic-state`, `monthly-summary`, `category-totals`, `account-totals`, `account-group-totals`, and `asset-total-balance`.
- The reports index currently exposes only five cards and does not provide a direct card entry for `asset-total-balance`.
- Some report labels/microcopy communicate intent inconsistently (for example, title key naming that suggests account analysis while the destination is monthly summary).

Constraints:
- Existing routes and route semantics must be preserved.
- No backend/API/data model changes are allowed for this change.
- Accessibility behavior and premium card interaction patterns must remain intact.
- Existing OpenSpec capabilities around reporting and dashboard/reporting entry must remain coherent.

Stakeholders:
- End users who need to discover the correct report quickly.
- Product/UX owner focused on reporting clarity and reduced navigation friction.
- Engineering team maintaining report pages and UI tests.

## Goals / Non-Goals

**Goals:**
- Reorganize the reports index into explicit analytical families.
- Ensure complete discoverability of relevant report deep-dive routes from `/reports`.
- Improve card naming and microcopy consistency while preserving destinations.
- Keep interaction behavior deterministic and accessible.
- Update tests and docs to encode the new information architecture.

**Non-Goals:**
- No change to report calculation logic or API payload semantics.
- No change to report page internals beyond entry context copy.
- No new report route creation.
- No dashboard KPI click-through behavior work.

## Decisions

### Decision 1: Introduce sectioned information architecture in reports index
Choice:
- Replace a single flat card grid with sectioned composition by analytical family:
  1. Financial Snapshot
  2. Period Flow Analysis
  3. Account Structure Analysis

Rationale:
- Sectioning reduces scan complexity and improves first-click accuracy.
- Families map naturally to user intent: "state now", "flow in period", "where flow/state comes from".

Alternatives considered:
- Keep flat layout and only reorder cards: rejected because it does not solve category comprehension.
- Add tabs for families: rejected to avoid hiding options and increasing interaction depth.

### Decision 2: Preserve route targets and card interaction mechanics
Choice:
- Keep card click navigation model and route paths unchanged.
- Only change grouping layout, card ordering, and copy labels/descriptions.

Rationale:
- Aligns with existing requirement to preserve navigation targets and accessibility behavior.
- Minimizes regression risk and avoids unnecessary route migration.

Alternatives considered:
- Add route aliases/new route names: rejected because it introduces avoidable navigation complexity.

### Decision 3: Include explicit discoverability for `asset-total-balance`
Choice:
- Add an explicit report entry card in the "Financial Snapshot" family for asset total balance.

Rationale:
- Route already exists but lacks first-level discoverability on reports index.
- Improves parity between available routes and index entry surface.

Alternatives considered:
- Keep hidden and rely on deep links: rejected because discoverability remains incomplete.

### Decision 4: Normalize card naming and microcopy by semantic contract
Choice:
- Align each card title/subtitle/badge with report purpose and time scope semantics.
- Avoid ambiguous naming that can imply another report family.

Rationale:
- Reduces user confusion and aligns mental model with route destination.

Alternatives considered:
- Keep current resource keys/texts as-is: rejected due to ambiguity persistence.

### Decision 5: Validate with UI tests focused on IA, presence, and destinations
Choice:
- Add or update Web UI tests to assert:
  - family section rendering,
  - card presence per family,
  - unchanged destination routes,
  - inclusion of `asset-total-balance` entry.

Rationale:
- Guards against regressions in discoverability and navigation.

Alternatives considered:
- Rely only on manual QA: rejected due to recurring regression risk in UI composition changes.

## Risks / Trade-offs

- [Risk] Localization key churn could create missing resource entries.
  -> Mitigation: prefer additive keys, verify fallback behavior, and include explicit UI tests for visible labels where feasible.

- [Risk] Sectioned layout may alter perceived prominence of some reports.
  -> Mitigation: define deterministic order within each family and keep prominent snapshot reports first.

- [Risk] Card structure refactor may impact accessibility roles/keyboard behavior.
  -> Mitigation: preserve existing interactive element structure and validate with existing accessibility-oriented test conventions.

- [Trade-off] Improved clarity requires additional copy maintenance effort.
  -> Mitigation: centralize naming rationale in change docs and keep consistent key conventions.

## Migration Plan

1. Implement grouped reports index composition in `ReportsIndexPage.razor` while preserving route navigation behavior.
2. Add the missing `asset-total-balance` card entry in the appropriate group.
3. Normalize report index microcopy/localization keys for clarity and consistency.
4. Update/add UI tests covering grouped IA and route stability.
5. Update release/implementation notes documenting IA rationale and user-facing outcome.
6. Validate with focused web tests and, if needed, broader solution test run.

Rollback strategy:
- Revert reports index composition and resource key additions to previous flat layout.
- Revert associated tests to prior expectations.
- No backend rollback required because no contracts are changed.

## Open Questions

- Should section headers be purely semantic (for example, "Financial Snapshot") or explicitly workflow-oriented (for example, "Start Here")?
- Should the reports index include a lightweight "recommended first report" hint for new users, or remain neutral?
- Confirm final copy style preference (strictly concise labels vs descriptive labels) before implementation.
