## Why

The current reports index presents all entries in a flat grid without explicit analytical families, which makes report discovery slower and increases cognitive load when users need to choose the right report quickly. A focused information-architecture revision is needed now to improve scanability, reduce ambiguity in report naming, and expose currently under-discoverable report routes.

## What Changes

- Reorganize the Reports index (`/reports`) into explicit analytical sections with clear group headings and concise section descriptions.
- Keep existing report routes and navigation targets intact while changing only the entry organization and presentation hierarchy.
- Improve report card naming and microcopy so each card communicates report intent and scope consistently.
- Add discoverability for existing report routes that are currently not represented on the reports landing surface.
- Preserve accessibility behavior and existing interaction patterns (card click behavior, keyboard/accessibility expectations).
- Add/adjust UI tests for reports index grouping, card presence, and navigation stability.
- Update implementation notes/documentation to describe the new grouping model and naming rationale.

### Critical Implementation Constraints

- Do not change backend API contracts or report calculation semantics.
- Do not remove or rename existing report routes.
- Do not introduce new architectural patterns or cross-layer dependencies.
- Do not degrade current accessibility behavior.

### Non-Goals

- No redesign of report detail pages (`/reports/*`) beyond entry-point naming/context framing in the index.
- No changes to report formulas, metrics semantics, filters, or data retrieval logic.
- No new report computation capabilities in API/Application layers.
- No dashboard KPI-to-report click navigation work in this change.

## Capabilities

### New Capabilities
- `reports-index-information-architecture`: Defines the grouped reports entry experience, discoverability rules, and copy consistency requirements for `/reports`.

### Modified Capabilities
- `dashboard-reporting-entry`: Refines reports landing requirements to explicitly require semantically grouped report families and complete discoverability of report deep-dive routes.

## Impact

- Affected UI surface:
  - `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor`
  - Related localization resources for reports index labels/descriptions.
- Affected test surface:
  - `tests/FamilyFinances.Web.Tests/Features/Reports/*` (reports index rendering and navigation assertions).
- APIs/data model:
  - No API contract changes.
  - No database or migration changes.
- Documentation:
  - Add a release/implementation note for the reports IA reorganization.

## Release Impact

Type: minor
Rationale: Introduces backward-compatible UX and information-architecture improvements to report discovery without breaking existing routes or API contracts.

## Rollback Plan

- Revert the reports index view and related localization keys to the prior flat-card composition.
- Revert associated reports index UI tests to pre-change assertions.
- Keep report routes unchanged so rollback does not affect deep-link compatibility.
