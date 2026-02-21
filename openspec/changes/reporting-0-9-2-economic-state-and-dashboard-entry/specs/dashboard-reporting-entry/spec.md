## ADDED Requirements

### Requirement: Dashboard SHALL Provide Direct Economic State Access
The dashboard MUST expose a direct entry point to the economic-state report.

#### Scenario: Dashboard card links to economic-state report
- **WHEN** an authenticated user opens the dashboard
- **THEN** the dashboard MUST display an `Economic State` shortcut card
- **AND** activating that card MUST navigate to `/reports/economic-state`

### Requirement: Dashboard Preview MUST Be Date-Explicit
Any KPI preview shown in the dashboard shortcut MUST include a clear as-of reference.

#### Scenario: Dashboard preview indicates as-of date
- **WHEN** the dashboard renders economic-state preview values
- **THEN** the preview MUST show the date basis (for example, "As of today")
- **AND** values MUST follow the canonical metric semantics defined for reporting

