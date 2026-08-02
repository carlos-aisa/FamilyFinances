## MODIFIED Requirements

### Requirement: Reports Entry Surface SHALL Use Premium Navigation Card Patterns
The reports landing entry surface MUST present report options with consistent premium card patterns that improve scanability without changing navigation targets.

#### Scenario: Reports index keeps destinations while improving visual clarity
- **WHEN** an authenticated user opens `/reports`
- **THEN** each report entry MUST preserve its existing route target and accessibility behavior
- **AND** report entry cards MUST use shared premium card hierarchy and interaction feedback
- **AND** report entries MUST be organized by explicit analytical families with clear family headings and concise explanatory copy
- **AND** family and card ordering MUST remain deterministic across renders
- **AND** families with two report cards MUST use a two-column desktop layout to give descriptions sufficient horizontal space

### Requirement: Navigation Chrome SHALL Remain Task-Focused For Reporting Access
Navigation treatment for Dashboard, Quick Entry, and Reports MUST remain task-focused and MUST avoid relocating language controls to top-level navigation.

#### Scenario: Navigation keeps reporting and capture paths explicit
- **WHEN** the authenticated navigation menu is rendered
- **THEN** it MUST provide direct entries for `Dashboard`, `Quick Entry`, and `Reports`
- **AND** report deep-dive paths MUST remain discoverable through the Reports route family
- **AND** the Reports route family entry surface (`/reports`) MUST include direct discoverability for the five primary report cards without duplicating `/reports/asset-total-balance`, whose existing deep-link route remains unchanged

#### Scenario: Language control remains out of main navigation
- **WHEN** the navigation menu is rendered after IA updates
- **THEN** no language selector control MUST be introduced in navigation chrome
- **AND** language management MUST remain scoped to Settings
