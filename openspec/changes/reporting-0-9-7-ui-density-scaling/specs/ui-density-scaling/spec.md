## ADDED Requirements

### Requirement: Web UI SHALL Provide Four Global Density Levels
The Web UI MUST provide exactly four global density levels for visual scaling: `Small`, `Medium`, `Large`, and `XLarge`.

#### Scenario: Density levels are available from settings
- **WHEN** an authenticated user opens the settings page
- **THEN** the density control MUST expose exactly `Small`, `Medium`, `Large`, and `XLarge`
- **AND** selecting one level MUST set it as the active app-wide density level

#### Scenario: Active density level is applied across pages
- **WHEN** a density level is active
- **THEN** typography, controls, card spacing, and table density MUST render using that level on all report and ledger pages

### Requirement: Density Preference MUST Persist In Browser Local Storage
An explicit density choice MUST persist in browser local storage and be restored on next app load.

#### Scenario: Explicit user choice is persisted and restored
- **WHEN** a user selects a density level from settings
- **THEN** the system MUST store the preference in browser local storage
- **AND** after a page reload, the same density level MUST be restored before interactive use

#### Scenario: Density persistence remains client-side only
- **WHEN** density preference is saved
- **THEN** no backend API endpoint or server-side profile write MUST be required for persistence

### Requirement: Automatic Compact Density MUST Be Applied On Constrained Viewports
The Web UI MUST apply a compact default density on constrained viewport profiles when no explicit user preference exists.

#### Scenario: Constrained viewport defaults to compact density
- **WHEN** a user has no explicit stored density preference and the app starts on a mobile or low-resolution viewport
- **THEN** the active density MUST default to `Small`
- **AND** the density source MUST be represented as automatic mode

#### Scenario: Explicit preference overrides automatic mode
- **WHEN** a user explicitly selects any density level
- **THEN** automatic viewport logic MUST NOT override that explicit selection on subsequent loads

### Requirement: Density Scaling SHALL Use Shared Global Tokens
The UI scaling model MUST be implemented through shared global tokens so behavior is consistent across components.

#### Scenario: Shared tokens drive core surface scaling
- **WHEN** the active density changes
- **THEN** body typography, form control heights, card paddings, and table cell paddings MUST change through shared token values
- **AND** per-page one-off hardcoded overrides MUST NOT be required as the primary mechanism
