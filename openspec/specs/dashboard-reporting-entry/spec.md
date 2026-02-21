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

