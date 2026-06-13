## MODIFIED Requirements

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
