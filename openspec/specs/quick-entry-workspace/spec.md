# quick-entry-workspace Specification

## Purpose
TBD - created by archiving change dashboard-household-financial-overview. Update Purpose after archive.
## Requirements
### Requirement: System SHALL Provide A Dedicated Quick Entry Workspace Route
The system MUST expose a dedicated route for rapid transaction capture outside Dashboard.

#### Scenario: Quick Entry route is reachable from authenticated navigation
- **WHEN** an authenticated user opens the main navigation menu
- **THEN** the menu MUST include a `Quick Entry` destination
- **AND** selecting it MUST navigate to `/quick-entry`

#### Scenario: Dashboard no longer hosts primary quick-entry workload
- **WHEN** a user opens `/`
- **THEN** Dashboard MUST remain analytics-first
- **AND** primary quick-entry interaction components MUST be hosted under `/quick-entry`

### Requirement: Quick Entry Workspace SHALL Preserve Existing Capture Semantics
Quick-entry flows moved to `/quick-entry` MUST preserve existing operational behavior.

#### Scenario: Capture actions keep existing transaction behavior
- **WHEN** a user performs expense, income, transfer, or refund capture in `/quick-entry`
- **THEN** validation and transaction creation semantics MUST match pre-move behavior
- **AND** account selection workflows MUST remain deterministic

#### Scenario: Existing widgets retain behavior in new workspace
- **WHEN** quick-entry widgets are rendered in `/quick-entry`
- **THEN** widget expand/collapse and submission behavior MUST remain equivalent to baseline
- **AND** no additional ledger-side side effects MUST be introduced by relocation

#### Scenario: Refund original-expense picker uses deterministic display formatting
- **WHEN** a user opens Refund quick entry and searches original expenses
- **THEN** each listed expense date MUST be rendered as `dd/MM/yyyy`
- **AND** each listed amount MUST be rendered with EUR suffix format (`XXX,XX €`)
- **AND** selecting or clearing linked expenses MUST preserve existing workflow semantics

### Requirement: Quick Entry Global Account Search SHALL Ignore Diacritics and Case
The Quick Entry account search input MUST match candidate values independent of accent marks and character casing.

Implementation scope:

- File: `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor`
- Search source: `_globalAccountSearchQuery`
- Candidate fields:
  - account name (`AccountDto.Name`)
  - nature label (`GetNatureLabel(...)`)
  - catalog-driven kind label (resolved from unified kind catalog, including system and custom kinds)
- The same normalization algorithm MUST be applied to query and candidate fields before `Contains` checks.
- Empty or whitespace query MUST preserve current unfiltered behavior.

#### Scenario: Unaccented query matches accented account name
- **WHEN** user types `maria` in Quick Entry global account search
- **THEN** the result list MUST include account names such as `María`
- **AND** matching MUST remain case-insensitive

#### Scenario: Accented query matches non-accented account name
- **WHEN** user types `José` in Quick Entry global account search
- **THEN** the result list MUST include account names such as `Jose`
- **AND** non-accented storage values MUST remain searchable

#### Scenario: Label-based matching remains functional after normalization
- **WHEN** user query matches normalized nature or kind labels
- **THEN** accounts MUST still be filtered by those labels as before
- **AND** existing accordion section and auto-expand behavior MUST remain unchanged

#### Scenario: Custom kind labels are searchable in quick entry
- **WHEN** an account uses a custom catalog kind and user query matches that kind label
- **THEN** the account MUST be included in search results
- **AND** search behavior MUST be equivalent to predefined kind labels

### Requirement: Shared Account Selector Search SHALL Ignore Diacritics and Case
The reusable `AccountSelector` component MUST apply accent-insensitive, case-insensitive text matching for account-selection workflows.

Implementation scope:

- File: `src/FamilyFinances.Web/Components/Shared/AccountSelector.razor`
- Search source: `_searchQuery`
- Candidate fields:
  - account name (`AccountDto.Name`)
  - nature text (`AccountNature.ToString()`)
- Existing nature constraints (`FilterByNature`, `AllowedNatures`) MUST remain unchanged.

#### Scenario: Account selector matches accented names with plain query
- **WHEN** user types `cafe` in account selector search
- **THEN** accounts named `Café` MUST be included in results
- **AND** accounts excluded by existing nature constraints MUST remain excluded

#### Scenario: Empty selector query keeps current filtered-by-nature baseline
- **WHEN** account selector query is empty
- **THEN** component MUST return the same list it would return before search text filtering
- **AND** no additional normalization-only filtering MUST be applied

