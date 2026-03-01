## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not change accounting semantics, report formulas, or API contracts as part of this redesign.
- Do not introduce one-off page styles that bypass shared design tokens.
- Do not make light mode the startup default; dark mode must remain the default experience.
- Do not add a language selector to top navigation; language stays inside Settings.

### Required
- Adopt a dark-first premium visual direction: `Data cockpit premium` + `Minimal luxe`.
- Prioritize Dashboard and Reports pages first, then propagate only reusable primitives to the rest of the UI.
- Implement a centralized token system for color, typography, spacing, radius, elevation, and chart palette.
- Preserve current navigation flows and feature coverage while modernizing visual presentation.

## Why

The current interface is functional but visually inconsistent and not premium enough for data-heavy financial workflows. A unified dark-first design language is needed now to improve perceived quality, readability, and confidence when using Dashboard and Reports as the primary decision surfaces.

## What Changes

- Introduce a premium visual system for the web UI:
  - dark-first color tokens with improved contrast hierarchy,
  - expressive typography scale for titles, KPIs, tables, and annotations,
  - consistent card, border, shadow, and spacing primitives.
- Redesign Dashboard presentation to feel more modern and intentional while preserving current data semantics and interactions.
- Redesign Reports presentation (especially summary and evolution areas) with clearer information hierarchy, denser but readable tables, and chart styling consistency.
- Standardize chart presentation across report pages (palette, axis/grid styling, legends, labels, emphasis states) for a coherent analytical experience.
- Update shared UI foundations used by Dashboard/Reports (table headers, badges, filters, tabs, panels) to match the new premium style.
- Add/update documentation for the frontend visual language so future changes follow the same system.

## Capabilities

### New Capabilities
- `premium-frontend-design-system`: Defines dark-first premium UI tokens, visual primitives, and chart styling rules used across Dashboard and Reports.

### Modified Capabilities
- `dashboard-reporting-entry`: Update dashboard composition and visual hierarchy to the new premium design language without changing feature behavior.
- `economic-state-reporting`: Apply premium layout and visual hierarchy rules to economic state report views and summary blocks.
- `reporting-insights`: Refresh insights cards/tables/chart framing for improved readability and consistency with the new design system.
- `monthly-reporting-charts`: Align monthly report chart rendering style and surrounding UI chrome with premium tokenized rules.
- `annual-reporting-charts`: Align annual charts and comparative visuals with the same premium style system and readability targets.
- `system`: Extend shell-level appearance requirements to enforce dark-first startup and shared premium primitives.

## Impact

- Affected frontend areas:
  - `src/FamilyFinances.Web/Components/Layout/*`
  - `src/FamilyFinances.Web/Components/Pages/Dashboard/*`
  - `src/FamilyFinances.Web/Components/Pages/Reports/*`
  - shared styles/scripts under `src/FamilyFinances.Web/wwwroot/*`
- Affected documentation:
  - frontend style guidance and report UX notes in `docs/` and/or `README.md` sections related to UI behavior.
- Affected testing:
  - update Web UI tests for layout markers, class/state expectations, and any visual-behavior assertions impacted by the redesign.
- No backend schema/API change is planned.

## Non-Goals

- No change to business rules, calculations, or reporting metric definitions.
- No addition/removal of report features in this change; scope is visual/UX quality and consistency.
- No full redesign of all application pages in one pass; initial focus is Dashboard + Reports with reusable primitives only.
- No migration from current frontend framework stack.

## Rollback Plan

- Keep premium styling behind a single top-level theme switch/fallback path so legacy styling can be restored quickly.
- Revert dashboard/report page style bindings to previous classes/tokens if severe regressions appear.
- Preserve data/interaction contracts so rollback is visual-only and low risk.
- Re-run Web UI and report regression tests after rollback to validate restoration of baseline behavior.
