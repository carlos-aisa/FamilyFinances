## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not implement one-off per-page font or control size hacks that bypass shared UI scale tokens.
- Do not persist UI scale preference in backend storage or user profile APIs.
- Do not introduce breaking API contract changes for existing reporting endpoints.
- Do not remove existing dark mode behavior while introducing the new settings entry point.

### Required
- Implement app-wide scaling through centralized UI tokens that affect typography, control height, spacing, tables, and cards.
- Provide exactly four user-selectable scale levels: `Small`, `Medium`, `Large`, `XLarge`.
- Persist explicit user selection in browser local storage and apply it on app startup.
- Apply automatic compact mode for mobile/low-resolution contexts, while still allowing user override from settings.
- Introduce a unified Settings entry point where appearance preferences are managed (theme now, density now, language/backup-restore ready for next iterations).

## Why

The current UI density is not usable enough on smaller or lower-resolution screens, especially in data-heavy report pages. This change is needed now to close the 0.9.x reporting usability gap with a global, consistent scaling model instead of page-by-page tweaks.

## What Changes

- Add a global UI density system with four levels (`Small`, `Medium`, `Large`, `XLarge`) that scales fonts, controls, spacing, and table/card density across the whole app.
- Add browser-local persistence for UI density preference so users keep their chosen scale between sessions.
- Add automatic compact scaling for mobile/low-resolution devices as a default behavior, with explicit user override support in Settings.
- Introduce a centralized Settings surface in the Web app shell and move appearance controls there, making it the canonical home for theme and density preferences.
- Prepare Settings information architecture for language and backup/restore options to be added in later changes without reworking navigation.
- Keep functional report behavior and API contracts unchanged (visual and usability change only).

## Capabilities

### New Capabilities
- `ui-density-scaling`: App-wide density tokens, four size levels, local persistence, and automatic compact defaults for constrained screens.
- `ui-settings-center`: Dedicated settings entry point and navigation for user preferences (theme and density now, extensible for language and backup/restore).

### Modified Capabilities
- `system`: Web UI shell and preference behavior requirements are extended to include global density scaling and centralized settings access.

## Impact

- Affected frontend areas: shared layout shell, style token definitions, reusable form/table/card components, and report pages that currently depend on fixed density assumptions.
- Affected cross-cutting concerns: browser storage integration for preference persistence and startup preference hydration in Blazor Web app initialization.
- Test impact: update/add Web UI tests for density selector rendering, persistence, automatic compact behavior, and settings navigation.
- Documentation impact: update system/reporting user guidance to describe density levels and settings entry location.
- Runtime/API impact: no backend schema change and no reporting API contract change.

## Non-Goals

- No server-side profile synchronization of UI preferences in this version.
- No redesign of report calculations, metric semantics, or data contracts.
- No full accessibility overhaul beyond density scaling behavior.
- No implementation of backup/restore workflow in this change (only settings extensibility groundwork).

## Rollback Plan

- Keep the previous static density CSS path behind a guarded fallback stylesheet during rollout.
- If regressions appear, disable dynamic density application and force baseline density via a single configuration switch in the Web host.
- Keep Settings navigation entry but hide density controls behind a feature toggle until issues are resolved.
- Re-run Web/UI regression tests and reporting smoke tests after rollback to confirm restored baseline behavior.
