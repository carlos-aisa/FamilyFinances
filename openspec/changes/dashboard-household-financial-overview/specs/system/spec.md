## ADDED Requirements

### Requirement: System Navigation IA SHALL Separate Analysis And Capture Surfaces
The system MUST separate dashboard analysis and quick-entry capture surfaces while keeping report navigation explicit in main menu.

#### Scenario: Navigation exposes dedicated capture and analysis routes
- **WHEN** authenticated navigation chrome is rendered
- **THEN** users MUST be able to access `/` for dashboard analysis and `/quick-entry` for capture workflows
- **AND** `Reports` menu access MUST remain available for deep-dive report pages

#### Scenario: Dashboard does not become a report-link launcher
- **WHEN** users open `/`
- **THEN** dashboard content MUST prioritize financial status analysis blocks
- **AND** report entry card grids MUST not be required to access report details

### Requirement: System SHALL Preserve Language-Control Scope In Settings During IA Changes
Language-selection scope MUST remain constrained to Settings while navigation IA changes are introduced.

#### Scenario: Navigation IA updates do not surface language selector
- **WHEN** dashboard and quick-entry navigation updates are deployed
- **THEN** main navigation MUST not include a language selector control
- **AND** Settings MUST remain the canonical language-switching surface
