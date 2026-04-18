# quick-entry-workspace Delta Specification

## ADDED Requirements

### Requirement: Quick Entry Global Account Search SHALL Ignore Diacritics and Case
The Quick Entry account search input MUST match candidate values independent of accent marks and character casing.

Implementation scope:

- File: `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor`
- Search source: `_globalAccountSearchQuery`
- Candidate fields:
  - account name (`AccountDto.Name`)
  - nature label (`GetNatureLabel(...)`)
  - kind label (`GetKindLabel(...)`)
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

