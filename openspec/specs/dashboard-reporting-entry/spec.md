# dashboard-reporting-entry Specification

## Purpose
TBD - created by archiving change reporting-0-9-2-economic-state-and-dashboard-entry. Update Purpose after archive.
## Requirements
### Requirement: Navigation Menu SHALL Provide Direct Economic State Access
The application navigation menu MUST expose a direct entry point to the economic-state report.

#### Scenario: Navigation entry links to economic-state report
- **WHEN** an authenticated user opens the application navigation menu
- **THEN** the menu MUST display an `Economic State` shortcut entry
- **AND** activating that entry MUST navigate to `/reports/economic-state`

### Requirement: Navigation Preview MUST Be Date-Explicit
Any asset preview shown in the navigation shortcut MUST include a clear as-of reference.

#### Scenario: Navigation preview indicates as-of date
- **WHEN** the navigation menu renders the `Economic State` preview
- **THEN** the preview MUST show the date basis (for example, "As of today")
- **AND** the preview MUST display the `Asset Balance` stock metric semantics

### Requirement: Dashboard SHALL Preserve Existing Workflows While Upgrading Visual Hierarchy
The Dashboard page MUST preserve analytical workflow continuity while shifting to an analytics-first household overview that excludes embedded quick-entry workload.

#### Scenario: Dashboard renders analytics-first composition
- **WHEN** an authenticated user opens `/`
- **THEN** Dashboard MUST prioritize KPI and chart-first analytical blocks
- **AND** the composition MUST avoid tabbed containers for primary dashboard analysis

#### Scenario: Quick-entry workflow is no longer hosted on dashboard
- **WHEN** users need transaction quick-capture interactions
- **THEN** those interactions MUST be available through `/quick-entry`
- **AND** Dashboard MUST not host the primary quick-entry card/workspace composition

### Requirement: Reports Entry Surface SHALL Use Premium Navigation Card Patterns
The reports landing entry surface MUST present report options with consistent premium card patterns that improve scanability without changing navigation targets.

#### Scenario: Reports index keeps destinations while improving visual clarity
- **WHEN** an authenticated user opens `/reports`
- **THEN** each report entry MUST preserve its existing route target and accessibility behavior
- **AND** report entry cards MUST use shared premium card hierarchy and interaction feedback

### Requirement: Navigation Chrome SHALL Remain Task-Focused For Reporting Access
Navigation treatment for Dashboard, Quick Entry, and Reports MUST remain task-focused and MUST avoid relocating language controls to top-level navigation.

#### Scenario: Navigation keeps reporting and capture paths explicit
- **WHEN** the authenticated navigation menu is rendered
- **THEN** it MUST provide direct entries for `Dashboard`, `Quick Entry`, and `Reports`
- **AND** report deep-dive paths MUST remain discoverable through the Reports route family

#### Scenario: Language control remains out of main navigation
- **WHEN** the navigation menu is rendered after IA updates
- **THEN** no language selector control MUST be introduced in navigation chrome
- **AND** language management MUST remain scoped to Settings

