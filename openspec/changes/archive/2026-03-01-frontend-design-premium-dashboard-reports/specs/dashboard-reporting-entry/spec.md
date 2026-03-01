## ADDED Requirements

### Requirement: Dashboard SHALL Preserve Existing Workflows While Upgrading Visual Hierarchy
The Dashboard page MUST keep current quick-entry and account-selection behavior while adopting a premium information hierarchy for headers, widgets, and account panels.

#### Scenario: Quick-entry behavior remains unchanged after visual upgrade
- **WHEN** a user interacts with dashboard quick-entry cards and widgets
- **THEN** existing interaction semantics (expand/select/create/swap/clear) MUST remain functionally unchanged
- **AND** visual treatment MUST use shared premium panel and state styles

#### Scenario: Account nature panels use coherent premium states
- **WHEN** account lists by nature are rendered in the dashboard right column
- **THEN** card headers, badges, and selected-row states MUST follow shared premium styling primitives
- **AND** state meaning (selected from/selected to) MUST remain explicit

### Requirement: Reports Entry Surface SHALL Use Premium Navigation Card Patterns
The reports landing entry surface MUST present report options with consistent premium card patterns that improve scanability without changing navigation targets.

#### Scenario: Reports index keeps destinations while improving visual clarity
- **WHEN** an authenticated user opens `/reports`
- **THEN** each report entry MUST preserve its existing route target and accessibility behavior
- **AND** report entry cards MUST use shared premium card hierarchy and interaction feedback

### Requirement: Navigation Chrome SHALL Remain Task-Focused For Reporting Access
Navigation treatment for Dashboard and Reports MUST remain task-focused and MUST avoid relocating language controls to top-level navigation.

#### Scenario: Language control is not added to main navigation
- **WHEN** the navigation menu is rendered after premium redesign
- **THEN** no language selector control MUST be introduced in the main navigation chrome
- **AND** reporting and dashboard entries MUST remain direct and uncluttered

