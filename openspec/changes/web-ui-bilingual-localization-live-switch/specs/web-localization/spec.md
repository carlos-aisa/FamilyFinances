## ADDED Requirements

### Requirement: Web UI SHALL Provide Runtime Language Selection From Settings
The Web UI MUST expose a language selector in the Settings page so authenticated users can switch between supported UI languages without changing backend/user profile data.

#### Scenario: Language selector is available from Settings
- **WHEN** a user renders `/settings`
- **THEN** the Settings page MUST display a language selector control in the language preferences section
- **AND** the selector MUST include exactly `es-ES` and `en-US` options

#### Scenario: Selector reflects active language
- **WHEN** the page is rendered
- **THEN** the selector value MUST match the currently active UI culture

### Requirement: Language Changes SHALL Apply Immediately
The Web UI MUST apply a selected language immediately in the current route context.

#### Scenario: Switching language refreshes current route in selected culture
- **WHEN** a user changes selector value from `es-ES` to `en-US` or from `en-US` to `es-ES`
- **THEN** the app MUST persist the selected culture via the client culture helper
- **AND** the app MUST reload the current route so all visible UI text and formatting re-render in the selected language

### Requirement: Language Preference SHALL Persist Across Sessions
The Web UI MUST persist language preference client-side and reuse it on subsequent visits.

#### Scenario: Returning user sees previous language preference
- **WHEN** a user selects a language and later refreshes or reopens the browser
- **THEN** the app MUST resolve and apply the previously selected language at startup

### Requirement: Localization Scope SHALL Be Web UI Only
This change MUST localize only the Blazor Web UI and MUST NOT alter backend response contracts.

#### Scenario: API payloads remain unchanged
- **WHEN** users trigger API calls from localized pages
- **THEN** request and response DTO structures MUST remain unchanged
- **AND** no API endpoint path, version, or schema MUST be modified by this capability

### Requirement: Localized Text SHALL Replace Hardcoded UI Labels in Targeted Pages
The targeted shared and page-level UI elements MUST use localization resources instead of hardcoded display strings.

#### Scenario: Targeted shell and high-traffic pages are resource-driven
- **WHEN** the following pages/components are rendered: settings page, transactions list/detail, account movements, reports index, and shared date presets
- **THEN** user-facing labels and messages in those components MUST be served through localization resources for both supported cultures

### Requirement: User-Facing Formatting SHALL Follow Active Culture
Date and currency presentation in targeted components MUST follow active `CurrentCulture`/`CurrentUICulture` semantics.

#### Scenario: Date formatting follows selected language
- **WHEN** the user language is `es-ES`
- **THEN** month/day names in targeted pages MUST render in Spanish

#### Scenario: Date formatting changes after switch
- **WHEN** the user switches language to `en-US`
- **THEN** month/day names in targeted pages MUST render in English after immediate refresh

#### Scenario: Currency formatting follows selected language
- **WHEN** amounts are displayed in targeted pages/components
- **THEN** formatting MUST reflect selected culture conventions for separators and currency placement

### Requirement: Unsupported Culture Values SHALL Fallback Safely
The Web UI MUST normalize unsupported persisted culture values to a supported default.

#### Scenario: Invalid persisted culture is recovered
- **WHEN** persisted client culture value is missing, malformed, or outside supported set
- **THEN** the app MUST fallback to default `es-ES`
- **AND** the selector MUST render the fallback value consistently

### Requirement: Culture Helper Contract SHALL Be Stable for Settings Integration
The client-side culture helper MUST expose a stable API used by the Settings language selector.

#### Scenario: Culture helper API surface
- **WHEN** Settings page initializes language controls
- **THEN** JavaScript helper MUST provide methods with signatures compatible with:
  - `string getCulture()`
  - `string setCulture(string culture)`
- **AND** `setCulture` MUST persist both UI culture and UI-culture pair for server request localization compatibility
