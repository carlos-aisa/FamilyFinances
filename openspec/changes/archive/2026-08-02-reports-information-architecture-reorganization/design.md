## Context

The reports landing page (`/reports`) currently exposes report entries as a flat set of cards without explicit analytical grouping. Functional coverage has grown over multiple changes, but the index does not clearly communicate report families or relative usage intent.

Current observed state:
- Existing report routes include `economic-state`, `monthly-summary`, `category-totals`, `account-totals`, `account-group-totals`, and `asset-total-balance`.
- The reports index exposes the five primary report entries. `asset-total-balance` remains a supported deep-link route but is intentionally not repeated on the index because its summary is already included in Economic State.
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
- Ensure direct discoverability of the five primary report entries from `/reports` without duplicating the asset total balance summary.
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

### Decision 3: Avoid a redundant `asset-total-balance` card
Choice:
- Do not add an explicit report entry card for asset total balance in the "Financial Snapshot" family.
- Keep the existing `/reports/asset-total-balance` deep-link route unchanged.

Rationale:
- Economic State already communicates the relevant asset summary at the reports entry surface.
- Omitting the duplicate card keeps the Financial Snapshot family concise and avoids presenting equivalent balances as separate choices.

Alternatives considered:
- Add the card for route parity: rejected because it adds no distinct decision-support value on the index.

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
  - intentional absence of the redundant `asset-total-balance` entry.

Rationale:
- Guards against regressions in discoverability and navigation.

Alternatives considered:
- Rely only on manual QA: rejected due to recurring regression risk in UI composition changes.

### Decision 6: Use semantic, neutral family copy
Choice:
- Use the semantic family headings `Financial Snapshot`, `Period Flow Analysis`, and `Account Structure Analysis`.
- Keep the index neutral; do not add a recommended first-report prompt.
- Use concise card titles with one-line descriptions that explain report purpose.

Rationale:
- Semantic headings help users identify the analytical question they need to answer without prescribing a workflow.
- Neutral guidance remains appropriate because the best starting report depends on the user's immediate question.
- Brief descriptive copy improves discoverability without increasing visual density.

### Decision 7: Size report cards for two-column desktop groups
Choice:
- Render the report cards in a two-column desktop grid (`col-lg-6`) instead of reserving each family for three cards.

Rationale:
- The two-card families use the available horizontal space, which reduces wrapping in the descriptive copy.
- The Financial Snapshot family remains intentionally focused on its single Economic State entry.

Alternatives considered:
- Retain three-column sizing for future expansion: rejected because it adds avoidable vertical density to the current, user-facing layout.

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
2. Keep the existing asset total balance route available by deep link without duplicating it as an index card.
3. Normalize report index microcopy/localization keys for clarity and consistency.
4. Use two-column desktop sizing for report card groups.
5. Update/add UI tests covering grouped IA, focused entry selection, and route stability.
6. Update release/implementation notes documenting IA rationale and user-facing outcome.
7. Validate with focused web tests and, if needed, broader solution test run.

Rollback strategy:
- Revert reports index composition and resource key additions to previous flat layout.
- Revert associated tests to prior expectations.
- No backend rollback required because no contracts are changed.

## Open Questions

None. The family headings, neutral index behavior, and concise-title/descriptive-subtitle copy style were confirmed during implementation.
