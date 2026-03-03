## MODIFIED Requirements

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
