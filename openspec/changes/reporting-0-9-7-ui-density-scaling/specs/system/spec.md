## ADDED Requirements

### Requirement: Web Host SHALL Initialize Appearance Preferences At Startup
The Web host MUST initialize persisted appearance preferences (theme and density) on startup so users receive consistent rendering before interacting with pages.

#### Scenario: Startup initializes persisted appearance state
Given stored appearance preferences exist in browser storage  
When the web app is loaded  
Then the host MUST apply theme and density attributes from persisted values before interactive preference controls are used

#### Scenario: Startup falls back to deterministic defaults
Given no stored appearance preferences exist  
When the web app is loaded  
Then theme initialization MUST use the existing theme default behavior  
And density initialization MUST use deterministic automatic mode based on viewport constraints

### Requirement: Web UI Navigation SHALL Expose A Settings Entry For Authenticated Users
The Web UI navigation model MUST expose a stable route to user preferences.

#### Scenario: Settings entry appears in authenticated navigation
Given an authenticated user session  
When the navigation menu is rendered  
Then the menu MUST include a `Settings` entry that routes to `/settings`

#### Scenario: Existing navigation contracts remain intact
Given the settings entry is added  
When the navigation menu is rendered  
Then previously available entries (`Home`, `Accounts`, `Reports`, and other existing items) MUST remain available and functional
