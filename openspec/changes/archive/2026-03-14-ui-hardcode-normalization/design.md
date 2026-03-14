## Context

The Web UI already has a tokenized foundation (`src/FamilyFinances.Web/wwwroot/css/premium-theme.css`) but still mixes it with hardcoded values in multiple layers:
- shared CSS (`src/FamilyFinances.Web/wwwroot/css/app.css`, `src/FamilyFinances.Web/wwwroot/app.css`),
- component-scoped CSS (`src/FamilyFinances.Web/Components/**/*.razor.css`),
- Razor markup inline styles and layout literals,
- chart rendering config (`src/FamilyFinances.Web/wwwroot/js/reportCharts.js`),
- chart series color literals in report/dashboard pages and adapters.

The previous change (`ui-full-views-review`) stabilized visual behavior and chart consistency. This change must keep those approved visuals while removing hardcoded drift and establishing one predictable source of truth for presentation values.

Constraints that shape this design:
- no domain model changes,
- no breaking backend/API behavior changes,
- no new UX features or redesign,
- no hidden semantic changes in financial calculations,
- one explicit chart semantic adjustment is allowed: month-focused daily evolution charts are normalized against month opening balance and must preserve day-1 movement impact,
- maintain current Blazor + Chart.js integration model.

Stakeholders:
- users expecting visual continuity,
- maintainers who need centralized style updates,
- QA/tests that must detect reintroduced hardcoded values early.

## Goals / Non-Goals

**Goals:**
- Define a global token governance model for typography, spacing, radius, control sizing, chart panel sizing, and chart semantic colors.
- Refactor UI layers to consume shared tokens instead of page/component literals.
- Normalize chart color and sizing inputs so dashboard/reports use shared semantic mappings.
- Normalize month-focused daily evolution visualization to a shared zero-baseline contract using month opening balance metadata.
- Add automated guardrails that fail when new hardcoded presentation values are introduced in protected files.
- Preserve runtime behavior and approved visual semantics from prior changes, except for the explicit month-focused zero-baseline normalization contract.

**Non-Goals:**
- New page flows, navigation changes, or feature additions.
- Date filter semantic refactor (covered by `global-filter-behavior-semantics`).
- Financial domain model changes.
- Breaking API contract changes.
- Replacing Chart.js or introducing a new UI framework.

## Decisions

### Decision 1: Introduce a dedicated shared token layer as canonical source
- **Choice:** Add `src/FamilyFinances.Web/wwwroot/css/ui-tokens.css` as canonical token definitions, loaded before `app.css` and `premium-theme.css`.
- **Rationale:** Current tokens are partially embedded in theme files and mixed with raw literals. A dedicated token file creates explicit ownership and reduces duplication.
- **Ownership model:**
  - `ui-tokens.css`: canonical token values and semantic aliases.
  - `app.css`: generic app consumption rules and Bootstrap-level normalization.
  - `premium-theme.css`: theme-specific overrides that consume tokens; no duplicated base constants.
  - component `.razor.css`: only local composition/layout; must consume tokens, not define global primitives.
- **Alternative considered:** Keep tokens only in `premium-theme.css`.
  - **Rejected because:** that conflates theme styling with cross-theme application primitives and makes governance harder.

### Decision 2: Define chart semantic palette contracts in C# and JS boundaries
- **Choice:** Introduce semantic color keys (`income`, `expense`, `balance`, `neutral`, indexed fallback series) and map them through shared palette helpers used by:
  - chart model adapters (`Features/Reports/Charts/*`),
  - dashboard/report page chart model composition,
  - `reportCharts.js` fallback resolution.
- **Rationale:** Existing `ColorHex` literals in Razor pages and chart adapters cause drift and inconsistent updates.
- **Alternative considered:** Keep raw hex in DTO/model producers and only normalize in JS.
  - **Rejected because:** semantic intent is lost before rendering and testability decreases.

### Decision 3: Standardize chart container and legend dimensions through tokens
- **Choice:** Replace hardcoded chart panel/legend sizes with tokenized dimensions consumed by shared chart CSS classes (including composition/pie layouts).
- **Rationale:** Chart layout drift currently appears when similar chart types are rendered from different pages/components.
- **Alternative considered:** Per-component size tuning.
  - **Rejected because:** this recreates the fragmentation this change is meant to remove.

### Decision 4: Replace page-level inline style literals with typed view-model values + token classes
- **Choice:** Move inline presentation literals in Razor markup to:
  - token-backed CSS classes when static,
  - strongly-typed view-model fields only when data-driven (e.g., dynamic color chips), with restricted allowed sources.
- **Rationale:** Inline styles bypass shared governance and are difficult to audit.
- **Alternative considered:** Keep inline styles and rely on manual review.
  - **Rejected because:** does not scale and fails regression prevention goals.

### Decision 5: Add automated anti-hardcode guardrails with explicit allowlist
- **Choice:** Add a test suite that scans frontend sources for forbidden patterns:
  - hex colors in protected files,
  - inline `style=` in Razor where not explicitly allowed,
  - duplicated token definitions outside canonical token files.
- **Rationale:** Without automated enforcement, hardcoded values quickly reappear.
- **Allowlist principle:** strictly scoped exceptions for truly data-driven values (e.g., rendered legend swatches) documented in the test itself.
- **Alternative considered:** lint-only convention in docs.
  - **Rejected because:** not enforceable in CI and historically insufficient.

### Decision 6: Deliver in migration slices that keep the app stable
- **Choice:** Implement in this order:
  1. token layer and load order,
  2. shared CSS consumption,
  3. chart semantic palette plumbing,
  4. page/component cleanup,
  5. guard tests and regression assertions.
- **Rationale:** This minimizes risk by establishing foundations before touching many views.
- **Alternative considered:** one-pass refactor by page.
  - **Rejected because:** high conflict risk and difficult rollback.

### Decision 7: Use opening-balance metadata for month-focused zero-baseline normalization
- **Choice:** Month-focused daily charts (`EvolutionChart` in `DailyInMonth` mode) normalize rendered values using month opening balance, not first rendered point.
- **Rationale:** Subtracting the first rendered point can hide day-1 movement. Using opening balance preserves first-day transaction impact while still starting evolution at a zero baseline.
- **Implementation contract:**
  - Additive DTO metadata: `OpeningBalanceCents` on `MonthlyBalanceChartDto` and `MonthlyChartSeriesDto`.
  - Backend repository populates this metadata from existing opening-balance calculations.
  - Frontend adapters map it to chart baseline fields and chart rendering applies normalization consistently across dashboard/report contexts.
- **Alternative considered:** Keep first-point normalization only in frontend.
  - **Rejected because:** day-1 movements can be visually suppressed and semantics drift between charts.

## Risks / Trade-offs

- [Risk] Token migration changes cascade unexpectedly in low-traffic views -> Mitigation: staged rollout by module and targeted UI tests for dashboard/reports/accounts/history/login.
- [Risk] Removing literals may alter subtle contrast/spacing behavior -> Mitigation: keep semantic aliases matching current approved values first, then refactor consumption only.
- [Risk] Guard tests can be noisy if rules are too strict -> Mitigation: start with high-signal patterns and explicit path-scoped allowlist.
- [Risk] CSS load-order mistakes can override tokens incorrectly -> Mitigation: define and test deterministic stylesheet order in `Components/App.razor`.
- [Risk] Baseline normalization could be interpreted as a data semantics change -> Mitigation: document contract explicitly in specs and preserve underlying day ordering/value derivation.
- [Trade-off] Additional indirection (tokens + semantic mappings) increases initial complexity -> Mitigation: document ownership and keep naming conventions strict and shallow.
- [Trade-off] Some dynamic inline style usage may remain (data-driven color chips) -> Mitigation: constrain to known components and validated sources only.

## Migration Plan

1. Create `ui-tokens.css` with canonical primitives and semantic aliases (including chart sizing and chart semantic colors).
2. Update `Components/App.razor` stylesheet load order so token definitions are available before consumers.
3. Refactor `app.css`, `wwwroot/css/app.css`, and `premium-theme.css` to consume canonical tokens and remove duplicated base literals.
4. Introduce/extend chart semantic palette helpers in `Features/Reports/Charts/*` and replace direct page-level `ColorHex` literals.
5. Normalize chart JS fallback reads in `wwwroot/js/reportCharts.js` to semantic tokens rather than isolated literals.
6. Sweep Razor and `.razor.css` files for hardcoded presentation literals and migrate to token-backed classes/variables.
7. Add guard tests in `tests/FamilyFinances.Web.Tests` for hardcoded presentation regression detection.
8. Run web test suite and targeted chart/report smoke checks to confirm visual continuity.

### Rollback Strategy

1. Revert token-consumer refactors while preserving existing behavior-critical CSS blocks.
2. Temporarily re-enable legacy chart literal mapping if semantic palette migration causes rendering regressions.
3. Disable new guard tests only if they block emergency rollback; restore after stabilization.
4. Re-run full web tests to confirm baseline restoration.

## Open Questions

- Should anti-hardcode guardrails run only in test project or also as a dedicated CI lint step for faster feedback?
- For data-driven color swatches, should we keep inline style exceptions or move to CSS custom property assignment on container elements?
- Should `ui-tokens.css` include both dark/light semantic values, or only neutral primitives with theme overlays in `premium-theme.css`?
