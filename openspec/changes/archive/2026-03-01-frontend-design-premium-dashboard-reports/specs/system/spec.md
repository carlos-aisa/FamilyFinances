## ADDED Requirements

### Requirement: Web Host SHALL Enforce A Dark-First Premium Presentation Baseline
The Web host MUST enforce dark mode as the default presentation baseline and MUST initialize premium visual primitives for Dashboard and Reports without requiring user interaction.

#### Scenario: Default startup keeps dark-first premium baseline
- **WHEN** a user opens the Web app with no persisted theme preference
- **THEN** the app MUST start in dark mode
- **AND** premium shell and surface primitives MUST be applied to the rendered UI baseline

### Requirement: Language Selection SHALL Remain Scoped To Settings
The system MUST keep language selection controls in the Settings page and MUST NOT expose language selection in top-level navigation chrome.

#### Scenario: Navigation does not expose language selector
- **WHEN** an authenticated user opens the main navigation menu
- **THEN** no language selector control MUST be present in the menu chrome
- **AND** navigation entries for Dashboard and Reports MUST remain focused on task navigation

#### Scenario: Settings keeps language switching capability
- **WHEN** the user opens `/settings`
- **THEN** language controls MUST remain available there
- **AND** changing language MUST preserve current live-switch behavior

### Requirement: Premium Styling SHALL Be Implemented Through Shared Primitives
Cross-page premium styling MUST be implemented through shared primitives and MUST avoid one-off page-level visual forks.

#### Scenario: Shared primitives are applied before page-specific overrides
- **WHEN** Dashboard and Reports premium styling is implemented
- **THEN** shared tokenized primitives MUST be the primary mechanism for cards, tables, tabs, and panel framing
- **AND** page-level custom overrides MUST be limited to documented exceptions only

