## ADDED Requirements

### Requirement: Web UI SHALL Provide A Dedicated Settings Center
The Web UI MUST provide a dedicated settings page for user-level preferences at route `/settings`.

#### Scenario: Authenticated user can open settings from navigation
- **WHEN** an authenticated user opens the application navigation menu
- **THEN** a `Settings` entry MUST be visible
- **AND** selecting it MUST navigate to `/settings`

#### Scenario: Settings route is access-protected consistently with app navigation
- **WHEN** an unauthenticated user attempts to open `/settings`
- **THEN** the existing authorization flow MUST apply
- **AND** the route MUST NOT expose protected settings controls without authentication

### Requirement: Settings Center MUST Host Appearance Preferences
The settings page MUST be the canonical UI for appearance preferences, including theme and density.

#### Scenario: Theme preference is editable in settings
- **WHEN** the user opens the settings appearance section
- **THEN** the page MUST provide theme selection controls compatible with existing dark/light mode behavior
- **AND** changing theme MUST update the active app theme immediately

#### Scenario: Density preference is editable in settings
- **WHEN** the user opens the settings appearance section
- **THEN** the page MUST provide density selection controls for `Small`, `Medium`, `Large`, and `XLarge`
- **AND** changing density MUST update the active app density immediately

### Requirement: Settings Center SHALL Reserve Future Preference Sections
The settings page MUST expose clear placeholders for future preferences planned in the `0.9.x` roadmap.

#### Scenario: Language and backup/restore placeholders are visible
- **WHEN** a user views `/settings`
- **THEN** the page MUST show explicit placeholder sections for `Language` and `Backup/Restore`
- **AND** those sections MUST indicate they are planned and not yet functional in this version
