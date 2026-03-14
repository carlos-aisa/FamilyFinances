## ADDED Requirements

### Requirement: Accounts Groups SHALL Use Single-Open Accordion Presentation
The accounts page MUST present grouped account blocks as a single-open accordion while preserving existing account operations.

#### Scenario: Accounts groups are shown in accordion sections
- **WHEN** a user opens `/accounts`
- **THEN** grouped account sections MUST render as accordion items
- **AND** only one section MUST be expanded at a time

#### Scenario: Existing account actions remain available in accordion layout
- **WHEN** accounts are rendered in accordion sections
- **THEN** existing per-account actions (rename, movements, close/reopen, delete where allowed) MUST remain available
- **AND** dual-balance values MUST remain visible per account row

### Requirement: Accounts Page SHALL Show One Page-Level Updated-As-Of Message
The accounts page MUST present update timing at page level instead of repeating per-group footers.

#### Scenario: Group-level current-month footer text is removed
- **WHEN** grouped account sections are rendered
- **THEN** repeated per-group informational footer text about month basis MUST NOT be shown
- **AND** no duplicate timing message MUST appear at each group footer

#### Scenario: Page-level updated-as-of message is visible
- **WHEN** the accounts page header/description is rendered
- **THEN** one localized message equivalent to `Accounts updated as of {current date}` MUST be shown
- **AND** the date value MUST correspond to the app current date context
