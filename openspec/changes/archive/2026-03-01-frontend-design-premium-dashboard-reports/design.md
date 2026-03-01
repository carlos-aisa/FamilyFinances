## Context

The current web presentation is functional but visually fragmented. Dashboard and Reports pages mix Bootstrap defaults, ad-hoc dark mode overrides, and page-specific spacing/chart sizing rules. This makes the product look less premium and creates inconsistent readability in dense financial views.

Current state in code:
- Layout and navigation: `src/FamilyFinances.Web/Components/Layout/MainLayout.razor`, `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`
- Dashboard surface: `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor`
- Reports surfaces: `src/FamilyFinances.Web/Components/Pages/Reports/*.razor`
- Chart components: `src/FamilyFinances.Web/Components/Pages/Reports/Charts/AnnualLineChart.razor`, `src/FamilyFinances.Web/Components/Pages/Reports/Charts/MonthlyLineChart.razor`
- Current style layer: `src/FamilyFinances.Web/wwwroot/app.css`, `src/FamilyFinances.Web/wwwroot/css/app.css`
- Current chart rendering script: `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`
- Settings (already includes Theme, Language, Backup/Restore): `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor`

Constraints from proposal and product decisions:
- Dark mode remains default.
- Language selector stays inside Settings (no language control in top navigation).
- Scope is visual and UX consistency, not accounting logic changes.
- Priority pages are Dashboard and Reports.
- Style direction is `Data cockpit premium` + `Minimal luxe`.

Stakeholders:
- Daily operators using Dashboard as primary transaction cockpit.
- Users consuming report-heavy monthly/economic analysis.
- Maintainers who need a reusable tokenized system instead of one-off CSS fixes.

## Goals / Non-Goals

**Goals:**
- Deliver a dark-first premium visual system that is coherent across Dashboard and Reports.
- Standardize core UI primitives with design tokens: color, typography, spacing, radius, elevation, borders, chart palette.
- Improve hierarchy and scanability for KPI cards, dense tables, and charts.
- Align chart visuals (axis, grid, tooltip, legend, emphasis) across report components.
- Preserve existing routes, interactions, and business semantics.
- Keep Settings as the home for language and appearance controls.

**Non-Goals:**
- No changes to backend APIs, DTOs, persistence, or report formulas.
- No feature additions/removals in Dashboard/Reports workflows.
- No navigation IA redesign outside visual treatment.
- No full app-wide redesign in one pass (first class target is Dashboard + Reports, then reusable carry-over).

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Keep default startup theme as dark.
- [MUST] Use a single shared token source in CSS custom properties; avoid hardcoded per-page colors/sizes unless explicitly justified.
- [MUST] Apply premium styling through reusable classes/utilities, not page-specific inline style patches.
- [MUST] Keep language switcher only in `SettingsPage.razor`.
- [MUST] Preserve existing localization keys and runtime culture switching behavior.
- [MUST] Keep chart rendering deterministic and compatible with current bUnit/JS-interop test patterns.
- [MUST] Preserve existing route paths for Dashboard and Reports.
- [MUST] Keep accessibility baselines: focus visibility, keyboard navigation, contrast-aware text.
- [MUST NOT] introduce purple-accent visual baseline.
- [MUST NOT] replace Bootstrap with a new component framework.
- [MUST NOT] change report semantics or computation paths in Application/API layers.
- [MUST NOT] duplicate token definitions in both `wwwroot/app.css` and `wwwroot/css/app.css` without a migration strategy.

## DETAILED UI FLOWS

### Flow 1: App startup with premium dark baseline
1. User opens app root `/`.
2. Existing theme helper resolves persisted theme; dark remains fallback default.
3. Root theme classes/tokens load before first interactive paint.
4. Main layout renders premium shell primitives (background gradients, panel chrome, typography scale).
5. Dashboard opens with updated hierarchy and no logic behavior change.

Expected outcome:
- Immediate dark premium appearance without white flash.
- Existing feature behavior unchanged.

### Flow 2: Dashboard operation (quick entry + account lists)
1. User lands on Dashboard.
2. Header and subtitle render in premium type scale with stronger hierarchy.
3. Left rail quick-entry widgets keep existing interactions (`Expense`, `Income`, `Transfer`, `Refund`, widgets).
4. Right rail account nature cards use standardized card/badge/list tokens.
5. Selection/active states remain semantically identical but use premium state colors and surfaces.

Expected outcome:
- Faster visual parsing of active transaction mode and account targets.
- No change to quick-entry logic or account selection behavior.

### Flow 3: Reports navigation and page framing
1. User opens `/reports`.
2. Report cards/list keep existing navigation destinations.
3. Card chrome, hover/focus, and iconography update to premium style tokens.
4. On report page entry, consistent report-page scaffolding appears: title band, filter panel, data panel, chart panel.

Expected outcome:
- Unified design language across report pages.
- No route or permission behavior changes.

### Flow 4: Monthly Summary reading workflow
1. User opens `/reports/monthly-summary`.
2. Filter card renders with consistent premium controls, labels, and spacing.
3. After load, KPI cards render with a single metric card pattern.
4. Chart panel and insights panel use standardized header/actions/table densities.
5. Pareto/anomaly blocks retain same data content with improved readability and visual grouping.

Expected outcome:
- Better scanability for metrics and anomaly rows.
- Existing insights calculations and dimensions unchanged.

### Flow 5: Economic State workflow
1. User opens `/reports/economic-state`.
2. Tab strip (`Snapshot`, `AssetEvolution`, `IncomeEvolution`) keeps current behavior with upgraded tab styling.
3. Snapshot KPI cards and month-focused chart adopt premium card and chart tokens.
4. Evolution panels inherit the same chart/table primitives for consistent cross-tab reading.
5. Future expense evolution tab (from other change) can reuse the same primitives without re-styling.

Expected outcome:
- Consistent visual rhythm between snapshot and evolution views.
- No semantic changes to underlying metrics.

### Flow 6: Settings behavior alignment
1. User opens `/settings`.
2. Existing cards (Appearance, Language, Backup/Restore) adopt premium settings card style.
3. Language selector remains present only here.
4. Theme change still applies live.

Expected outcome:
- Visual consistency with the new system while preserving scope decision: no language selector in nav.

## DETAILED PAGE WIREFRAMES

### 1) Main layout shell (desktop)

```text
+--------------------------------------------------------------------------------------------------+
| NAV (left rail, fixed) | CONTENT (fluid)                                                        |
|------------------------ |------------------------------------------------------------------------|
| Logo / Brand            | Page Header: Title + Subtitle + contextual action                       |
| Dashboard               |------------------------------------------------------------------------|
| Accounts                | Main Content Grid                                                       |
| Reports                 | +--------------------------------------+------------------------------+ |
| Settings                | | Primary analysis / input surface      | Secondary context surface     | |
| Logout                  | | (cards/forms/charts/tables)           | (lists/insights/charts)       | |
|                         | +--------------------------------------+------------------------------+ |
+--------------------------------------------------------------------------------------------------+
```

### 2) Dashboard (desktop, premium cockpit)

```text
+--------------------------------------------------------------------------------------------------+
| Dashboard title + subtitle                                                                        |
|--------------------------------------------------------------------------------------------------|
| Left column (7/12)                                | Right column (5/12)                          |
| +----------------------------------------------+   | +-----------------------------------------+ |
| | Quick Entry Card (active mode)               |   | | Account Nature Card: Assets             | |
| +----------------------------------------------+   | +-----------------------------------------+ |
| +----------------------------------------------+   | +-----------------------------------------+ |
| | Quick Entry Card                              |   | | Account Nature Card: Liabilities        | |
| +----------------------------------------------+   | +-----------------------------------------+ |
| +----------------------------------------------+   | +-----------------------------------------+ |
| | Mortgage widget                               |   | | Account Nature Card: Expenses           | |
| +----------------------------------------------+   | +-----------------------------------------+ |
| +----------------------------------------------+   | +-----------------------------------------+ |
| | Multi split widget                            |   | | Account Nature Card: Income             | |
| +----------------------------------------------+   | +-----------------------------------------+ |
+--------------------------------------------------------------------------------------------------+
```

### 3) Monthly Summary (desktop)

```text
+--------------------------------------------------------------------------------------------------+
| Title + back button                                                                               |
|--------------------------------------------------------------------------------------------------|
| Filter panel (date presets, date range, account, payee, load/reset)                              |
|--------------------------------------------------------------------------------------------------|
| KPI row: [Income] [Expense] [Net] [Transactions]                                                  |
|--------------------------------------------------------------------------------------------------|
| Left area (5/12): month-focused chart     | Right area (7/12): insights panel                   |
| +---------------------------------------+  | +--------------------------------------------------+ |
| | Monthly line chart                    |  | | Pareto expense / income cards                   | |
| +---------------------------------------+  | | Anomaly breakdown table                          | |
|                                            | +--------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------+
```

### 4) Economic State Snapshot (desktop)

```text
+--------------------------------------------------------------------------------------------------+
| Title + back button                                                                               |
| Tabs: [Snapshot] [Asset Evolution] [Income Evolution]                                             |
|--------------------------------------------------------------------------------------------------|
| As-of filter + load action                                                                         |
|--------------------------------------------------------------------------------------------------|
| Stock KPIs row: [Assets] [Liabilities] [Net Worth]                                                |
| Flow KPIs row:  [Income] [Expense] [Period Net]                                                   |
|--------------------------------------------------------------------------------------------------|
| Info block (compact)                                                                               |
|--------------------------------------------------------------------------------------------------|
| Month-focused Income vs Expense chart                                                              |
+--------------------------------------------------------------------------------------------------+
```

### 5) Settings (desktop)

```text
+--------------------------------------------------------------------------------------------------+
| Settings title + subtitle                                                                          |
|--------------------------------------------------------------------------------------------------|
| [Appearance card] [Language card] [Backup/Restore card]                                           |
| - Theme toggles       - Culture select (es-ES/en-US)      - CTA to backup page                   |
+--------------------------------------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Theme persistence | `wwwroot/js/theme.js` | Keep API and dark default behavior | None |
| Localization switching | `wwwroot/js/culture.js` and `SettingsPage.razor` culture selector | Premium styling only | None |
| Layout shell | `MainLayout.razor`, `NavMenu.razor` | Apply premium classes/tokens, improve nav visual hierarchy | Optional shared layout helper class set |
| Dashboard composition | `DashboardPage.razor` structure and widgets | Replace visual classes and spacing primitives only | Optional `dashboard-premium.css` section inside shared styles |
| Report pages | Existing report page components | Apply consistent page shell, card/table/chart wrappers | Optional reusable `ReportSectionHeader` component |
| Chart components | `AnnualLineChart.razor`, `MonthlyLineChart.razor` | Add token-aware style hooks and optional chart variants | Optional `AnnualBarChart.razor` only if style parity needs dedicated rendering |
| Chart JS | `wwwroot/js/reportCharts.js` | Token-driven colors/grid/tooltip styles and optional line/bar config | Optional helper `resolveChartTheme()` |
| Shared CSS | `wwwroot/app.css` and `wwwroot/css/app.css` | Consolidate/normalize into one source of truth | `wwwroot/css/premium-theme.css` if split strategy chosen |
| Settings page | `SettingsPage.razor` | Card/controls styling uplift without behavior changes | None |
| Testing | Existing Web tests under `tests/FamilyFinances.Web.Tests` | Update assertions for classes/structures and add style-behavior guards | New tests for premium shell markers and chart token usage |

## Decisions

### Decision 1: Tokenized premium layer on top of Bootstrap
- **Choice:** Keep Bootstrap as structural base, add a tokenized premium layer (CSS custom properties + semantic utility classes).
- **Rationale:** Fastest path to consistency without framework migration risk.
- **Alternative considered:** Build a full custom design system replacing Bootstrap classes.
- **Rejected because:** high migration cost and avoidable regression risk.

### Decision 2: Dark-first semantic palette with restrained accents
- **Choice:** Introduce neutral dark surfaces with accent colors reserved for semantics (income/expense/info/warn/error) and avoid purple baseline.
- **Rationale:** Supports premium feel and financial data readability.
- **Alternative considered:** Vibrant neon palette.
- **Rejected because:** reduces legibility and looks less trustworthy for finance context.

### Decision 3: Typography pairing with explicit intent
- **Choice:** Use a two-tier type system:
  - Heading/UI display: `Sora` (or `Plus Jakarta Sans` fallback)
  - Body/table/data dense text: `Source Sans 3` (or `Segoe UI` fallback)
  - Numeric optional mono accents: `IBM Plex Mono` for compact badges/code-like labels
- **Rationale:** Distinct premium tone with readable dense tables.
- **Alternative considered:** Keep default Bootstrap/system stack only.
- **Rejected because:** fails premium target and weakens visual hierarchy.

### Decision 4: Chart theming driven by CSS tokens
- **Choice:** Chart colors (axes, grid, tooltip, legend, dataset defaults) come from CSS variables read in JS (`getComputedStyle`).
- **Rationale:** Single source of truth and theme consistency.
- **Alternative considered:** hardcoded hex values in `reportCharts.js`.
- **Rejected because:** duplicates theme logic and blocks future theming agility.

### Decision 5: Unified report page shell contract
- **Choice:** Define a reusable report shell contract: `header band`, `filter panel`, `metric row`, `analysis panels`, `data table panels`.
- **Rationale:** Coherent user expectation across all report pages.
- **Alternative considered:** per-page custom layout rules.
- **Rejected because:** perpetuates current inconsistency.

### Decision 6: Progressive scope rollout
- **Choice:** Implement premium primitives and apply first to Dashboard + Reports + Settings styling alignment.
- **Rationale:** Matches priority and limits blast radius.
- **Alternative considered:** redesign all pages in one sprint.
- **Rejected because:** lower delivery confidence and test burden explosion.

### Decision 7: Keep Settings as single language control location
- **Choice:** Preserve language switching only in `/settings`.
- **Rationale:** Explicit product requirement and cleaner navigation.
- **Alternative considered:** keep or re-add language toggle in nav.
- **Rejected because:** conflicts with approved UX decision.

### Decision 8: Motion as meaningful, not decorative
- **Choice:** Add subtle enter/transition animations for KPI cards and panel reveals, with reduced-motion guard.
- **Rationale:** Premium feel without distracting financial analysis.
- **Alternative considered:** remove all motion or add aggressive micro-interactions.
- **Rejected because:** either too static or noisy.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Premium token foundation (`src/FamilyFinances.Web/wwwroot/css/premium-theme.css`)

```css
:root {
  --ff-font-heading: "Sora", "Plus Jakarta Sans", "Segoe UI", sans-serif;
  --ff-font-body: "Source Sans 3", "Segoe UI", sans-serif;
  --ff-font-mono: "IBM Plex Mono", "Consolas", monospace;

  --ff-surface-0: #0d1117;
  --ff-surface-1: #111826;
  --ff-surface-2: #182233;
  --ff-surface-3: #1f2a3b;
  --ff-border-strong: #314055;
  --ff-border-soft: #243247;

  --ff-text-strong: #ecf2ff;
  --ff-text-muted: #aebcd3;
  --ff-accent-primary: #4da3ff;
  --ff-accent-success: #2bd67b;
  --ff-accent-danger: #ff5d73;
  --ff-accent-warning: #ffb84d;

  --ff-radius-sm: 10px;
  --ff-radius-md: 14px;
  --ff-radius-lg: 18px;

  --ff-shadow-soft: 0 8px 24px rgba(0, 0, 0, 0.28);
  --ff-shadow-elevated: 0 14px 36px rgba(0, 0, 0, 0.34);
}

[data-bs-theme="dark"] body.ff-premium {
  background:
    radial-gradient(1200px 600px at 80% -10%, rgba(77, 163, 255, 0.16), transparent 65%),
    radial-gradient(900px 500px at -10% 10%, rgba(43, 214, 123, 0.08), transparent 60%),
    var(--ff-surface-0);
  color: var(--ff-text-strong);
  font-family: var(--ff-font-body);
}
```

### Example 2: Premium panel primitives (`src/FamilyFinances.Web/wwwroot/css/premium-theme.css`)

```css
.ff-panel {
  background: linear-gradient(180deg, var(--ff-surface-2), var(--ff-surface-1));
  border: 1px solid var(--ff-border-soft);
  border-radius: var(--ff-radius-md);
  box-shadow: var(--ff-shadow-soft);
}

.ff-panel-header {
  border-bottom: 1px solid var(--ff-border-soft);
  padding: 0.85rem 1rem;
}

.ff-kpi-card {
  border-left: 4px solid var(--ff-accent-primary);
  border-radius: var(--ff-radius-md);
}

.ff-data-table th {
  color: var(--ff-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  font-size: 0.74rem;
}

.ff-data-table td {
  color: var(--ff-text-strong);
}
```

### Example 3: Layout shell marker (`src/FamilyFinances.Web/Components/Layout/MainLayout.razor`)

```razor
<div class="page ff-premium">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <article class="content px-3 py-3 ff-content-shell">
            @Body
        </article>
    </main>
</div>
```

### Example 4: Dashboard section framing (`src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor`)

```razor
<div class="d-flex align-items-center justify-content-between mb-4 ff-page-header">
    <div>
        <h3 class="mb-1 ff-page-title">
            <i class="bi bi-speedometer2 me-2"></i>@L["Dashboard_Title"]
        </h3>
        <p class="ff-page-subtitle mb-0">@L["Dashboard_Subtitle"]</p>
    </div>
</div>

<div class="row g-4 ff-dashboard-grid">
    <div class="col-12 col-lg-7 ff-dashboard-primary">...</div>
    <div class="col-12 col-lg-5 ff-dashboard-secondary">...</div>
</div>
```

### Example 5: Token-aware chart theming (`src/FamilyFinances.Web/wwwroot/js/reportCharts.js`)

```javascript
function resolveChartTheme() {
  const style = getComputedStyle(document.documentElement);
  return {
    tickColor: style.getPropertyValue("--ff-text-muted").trim() || "#adb5bd",
    gridColor: style.getPropertyValue("--ff-border-soft").trim() || "rgba(173,181,189,0.15)",
    tooltipBg: style.getPropertyValue("--ff-surface-3").trim() || "#1f2a3b",
    tooltipText: style.getPropertyValue("--ff-text-strong").trim() || "#ecf2ff"
  };
}

function applyLineChartOptions(scales) {
  const theme = resolveChartTheme();
  scales.x.ticks.color = theme.tickColor;
  scales.x.grid.color = theme.gridColor;
  if (scales.y) {
    scales.y.ticks.color = theme.tickColor;
    scales.y.grid.color = theme.gridColor;
  }
}
```

### Example 6: Motion with reduced-motion guard (`src/FamilyFinances.Web/wwwroot/css/premium-theme.css`)

```css
.ff-reveal {
  opacity: 0;
  transform: translateY(8px);
  animation: ffReveal 280ms ease-out forwards;
}

@keyframes ffReveal {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .ff-reveal {
    animation: none;
    opacity: 1;
    transform: none;
  }
}
```

## Risks / Trade-offs

- [Risk] Premium tokens conflict with existing Bootstrap utility assumptions -> Mitigation: keep token layer additive and map only semantic surfaces first.
- [Risk] Duplicate style files (`wwwroot/app.css` vs `wwwroot/css/app.css`) create drift -> Mitigation: define one canonical file and document deprecation path.
- [Risk] New typography could shift table density unexpectedly -> Mitigation: validate key tables on Dashboard and report pages with fixed baseline snapshots.
- [Risk] Chart styles become inconsistent between line and pie charts -> Mitigation: centralize chart theme resolver in `reportCharts.js`.
- [Risk] Dark-first styling might reduce contrast in secondary text -> Mitigation: explicit contrast token ladder and manual WCAG checks for primary report states.
- [Risk] Animation introduces perceived latency -> Mitigation: keep transitions <= 280ms and disable under reduced-motion.
- [Risk] Refactor touches many Razor files and may break test selectors -> Mitigation: preserve existing `data-testid` and add compatibility markers where needed.
- [Trade-off] Keeping Bootstrap limits freedom versus full custom UI kit -> Mitigation: use custom primitives for signature look while retaining Bootstrap grid/forms.
- [Trade-off] Two-font approach increases asset size -> Mitigation: use variable fonts and subset ranges if self-hosted.

## Migration Plan

1. Define premium tokens and primitives in a dedicated stylesheet (`premium-theme.css`) and include it after current app styles.
2. Add shell-level marker class (for scoped activation) and verify dark-default startup path remains stable.
3. Refactor shared report shell classes and chart wrapper classes to premium primitives.
4. Apply premium styles to Dashboard page structure without changing event handlers or state logic.
5. Apply premium styles to Report pages in priority order: `ReportsIndex`, `MonthlySummary`, `EconomicState`, then remaining report panels.
6. Align Settings card styling to the same primitives while preserving existing theme/language/backup behavior.
7. Update chart JS to consume tokenized colors and unify tooltip/axis/grid styling.
8. Update/add tests for shell markers, class expectations, chart config behavior, and no-regression interaction flows.
9. Update documentation with the new visual system rules and do/don't examples.

### Rollback Strategy

1. Keep premium theme behind a single activation marker (`ff-premium` class on shell).
2. If regressions appear, remove marker to return to baseline visuals while keeping functionality intact.
3. Revert chart theme resolver changes independently if only chart visuals regress.
4. Preserve Settings behavior and localization logic during rollback.
5. Run web test suite and reporting smoke checks after rollback to certify baseline restore.

## Open Questions

- Should typography assets be self-hosted under `wwwroot/fonts` or loaded via trusted CDN?
- Should chart legends remain hidden by default or become toggleable in dense reports?
- Should the premium shell marker be enabled globally in one release or phased by route?
- Is there a desired upper bound for card elevation/shadow strength in dark mode for older displays?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Architecture and scope integrity
- [ ] Confirm no backend project files changed for this design-only change.
- [ ] Confirm no DTO contract changes in Application/API.
- [ ] Confirm no database migrations are introduced.
- [ ] Confirm all changes remain in `FamilyFinances.Web`.
- [ ] Confirm route paths for Dashboard/Reports/Settings are unchanged.
- [ ] Confirm localization runtime switch still works after styling changes.

### Theme and token system
- [ ] Confirm dark mode remains default when no preference exists.
- [ ] Confirm premium tokens are defined once as canonical source.
- [ ] Confirm token naming is semantic (`surface`, `text`, `accent`) rather than page-specific.
- [ ] Confirm no purple default accents are introduced.
- [ ] Confirm body background uses premium multi-layer surface treatment.
- [ ] Confirm text contrast for primary content remains readable.
- [ ] Confirm muted text contrast remains readable in dark mode.
- [ ] Confirm border tokens differentiate panel boundaries clearly.
- [ ] Confirm radius/elevation tokens are reused across cards and panels.
- [ ] Confirm hardcoded hex values are minimized in Razor CSS blocks.

### Typography and spacing
- [ ] Confirm heading font and body font are applied consistently.
- [ ] Confirm KPI numeric readability is preserved.
- [ ] Confirm table headers remain scannable at report densities.
- [ ] Confirm table rows do not collapse excessively with new fonts.
- [ ] Confirm filter controls maintain comfortable click targets.
- [ ] Confirm page titles/subtitles follow the same hierarchy across Dashboard and Reports.
- [ ] Confirm spacing rhythm is consistent between cards/panels.
- [ ] Confirm mobile breakpoints still produce usable spacing.

### Layout and navigation
- [ ] Confirm main layout shell receives premium activation class.
- [ ] Confirm nav entries remain functionally unchanged.
- [ ] Confirm Settings entry remains available and visually aligned.
- [ ] Confirm no language selector appears in top navigation.
- [ ] Confirm language selector still appears in Settings.
- [ ] Confirm page headers align with action buttons without overlap.
- [ ] Confirm desktop dashboard two-column layout remains stable.
- [ ] Confirm report pages keep existing filter-to-results flow.

### Dashboard visuals
- [ ] Confirm quick-entry cards use premium panel primitives.
- [ ] Confirm active quick-entry state remains visually clear.
- [ ] Confirm account nature cards use consistent badge/style treatment.
- [ ] Confirm list-group hover/active styles remain accessible.
- [ ] Confirm loading and empty states match premium visual language.
- [ ] Confirm no dashboard logic behavior changed.

### Reports visuals
- [ ] Confirm report filter cards use shared premium form styling.
- [ ] Confirm KPI cards use unified metric card pattern.
- [ ] Confirm monthly summary chart panel uses premium framing.
- [ ] Confirm insights panel table density remains readable.
- [ ] Confirm anomaly badges remain semantically distinct.
- [ ] Confirm economic state tabs remain functionally identical.
- [ ] Confirm economic state KPI rows retain semantic color mapping.
- [ ] Confirm info cards/alerts use premium style without losing clarity.
- [ ] Confirm report empty/loading/error states are visually consistent.

### Chart system consistency
- [ ] Confirm chart axis colors derive from shared tokens.
- [ ] Confirm chart grid colors derive from shared tokens.
- [ ] Confirm chart tooltip background/text styles are coherent in dark mode.
- [ ] Confirm line chart and monthly chart share the same style language.
- [ ] Confirm composition chart border and surface styles match tokens.
- [ ] Confirm chart export buttons retain visibility and function.
- [ ] Confirm chart rendering still works when JS interop is unavailable in tests.
- [ ] Confirm no chart semantic data transformations changed.

### Motion and accessibility
- [ ] Confirm entry animations are subtle and not distracting.
- [ ] Confirm reduced-motion media query disables animations.
- [ ] Confirm keyboard focus remains visible on buttons/inputs/tabs.
- [ ] Confirm tab controls remain keyboard-operable.
- [ ] Confirm hover-only cues are not the sole state indicator.
- [ ] Confirm alert colors preserve meaning in dark mode.

### Testing and documentation
- [ ] Confirm web tests are updated for new class markers where needed.
- [ ] Confirm existing report tests remain green after style changes.
- [ ] Confirm existing dashboard tests remain green.
- [ ] Confirm settings tests still validate language/theme controls.
- [ ] Confirm test IDs used by existing tests are preserved.
- [ ] Confirm documentation describes premium token usage and rules.
- [ ] Confirm docs explicitly state dark mode default and nav language policy.
- [ ] Confirm rollback instructions are captured in change documentation.

