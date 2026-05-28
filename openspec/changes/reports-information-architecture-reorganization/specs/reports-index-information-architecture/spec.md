## ADDED Requirements

### Requirement: Reports Index SHALL Organize Entries By Explicit Analytical Families
The reports index page (`/reports`) MUST organize report entries into explicit analytical families to improve scanability and first-click accuracy.

#### Scenario: Reports index renders grouped analytical families
- **WHEN** an authenticated user opens `/reports`
- **THEN** the page MUST render report entries grouped into explicit analytical families
- **AND** each family MUST include a visible heading and short explanatory description
- **AND** report cards MUST remain directly actionable without hidden tabs or secondary navigation layers

#### Scenario: Family ordering remains deterministic
- **WHEN** the grouped reports index is rendered
- **THEN** family blocks MUST appear in a deterministic order
- **AND** card ordering inside each family MUST remain deterministic across renders

### Requirement: Reports Index SHALL Preserve Existing Route Targets While Improving Entry Clarity
Reorganization of the reports index MUST preserve existing report destinations and only change presentation hierarchy and entry copy.

#### Scenario: Existing report card destinations remain unchanged
- **WHEN** a user activates a report entry card from `/reports`
- **THEN** navigation MUST continue to the same existing route target associated with that report
- **AND** no existing report route path MUST be renamed or removed as part of this change

#### Scenario: Entry copy clarifies report intent
- **WHEN** report cards are displayed in grouped layout
- **THEN** title, description, and badge text MUST communicate report intent and scope consistently
- **AND** card labeling MUST avoid ambiguous phrasing that suggests a different report family

### Requirement: Reports Index SHALL Provide Complete Discoverability For Existing Report Deep-Dive Routes
The reports landing surface MUST provide direct discoverability for all intended report deep-dive routes in the active reports route family.

#### Scenario: Existing asset total balance report is directly discoverable from reports index
- **WHEN** an authenticated user opens `/reports`
- **THEN** the page MUST include a direct entry card for `/reports/asset-total-balance`
- **AND** that card MUST be presented within the financial snapshot family

#### Scenario: Grouped discoverability remains route-family consistent
- **WHEN** the reports index is rendered after reorganization
- **THEN** report entries MUST remain discoverable through the `/reports` route family entry surface
- **AND** discoverability improvements MUST not require users to navigate through unrelated pages first
