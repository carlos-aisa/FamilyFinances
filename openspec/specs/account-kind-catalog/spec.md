# account-kind-catalog Specification

## Purpose
Define the global account-kind catalog capability that provides stable catalog-backed identity for predefined and user-defined account kinds.

## Requirements
### Requirement: Global Account Kind Catalog SHALL Support System and Custom Entries
The system MUST provide a global account kind catalog with two entry origins: predefined system kinds and user-defined custom kinds.

Catalog contract:
- Each catalog row MUST include: `Id`, `Key`, `Name`, `IsSystem`, `IsActive`, `SortOrder`, and `Nature`.
- `Key` MUST be unique globally.
- System kind rows (`IsSystem = true`) MUST be seeded and MUST remain available after migrations.
- Custom kind rows (`IsSystem = false`) MUST be createable from the application.
- Custom kind rows MUST be bound to one account `Nature` and MUST only be selectable by accounts with the same `Nature`.

#### Scenario: Seeded system kinds are available after migration
- **WHEN** the application starts after applying the migration introducing kind catalog
- **THEN** predefined system kinds MUST exist in persistence with deterministic keys
- **AND** each seeded row MUST be active and selectable by account creation flows

#### Scenario: User can create a global custom kind
- **WHEN** a valid custom kind name is submitted
- **THEN** the system MUST create an active custom kind row with unique key semantics
- **AND** the new kind MUST become available in the same unified kind selectors used for account forms
- **AND** the new kind MUST be selectable only for accounts with matching `Nature`

#### Scenario: Account creation can create a missing compatible custom kind inline
- **WHEN** a user creates a custom kind from an account creation flow that already has a selected account `Nature`
- **THEN** the system MUST allow the custom kind to be created without leaving that account creation flow
- **AND** the resulting kind MUST inherit the same compatible `Nature` used by the current account form

#### Scenario: User can delete an unused custom kind
- **WHEN** a custom kind is not referenced by any account
- **THEN** the system MUST allow deleting that custom kind
- **AND** subsequent kind list reads MUST NOT include the deleted entry

#### Scenario: Used or system kinds cannot be deleted
- **WHEN** a delete operation targets a system kind or a custom kind that is referenced by at least one account
- **THEN** the operation MUST be rejected
- **AND** existing account references MUST remain unchanged

### Requirement: Account Kind Selectors SHALL Be Ordered By Visible Label
Account kind selectors in account create/edit flows MUST be ordered by the localized label visible to the user.

#### Scenario: Selector order follows visible labels
- **WHEN** the account kind selector is rendered for a nature
- **THEN** options MUST be sorted by the label text shown in the active UI language
- **AND** users MUST be able to find kinds consistently regardless of system/custom origin

### Requirement: Account Persistence SHALL Reference Kind Catalog Identity
Account entities MUST reference catalog-backed kind identity instead of enum-backed numeric identity.

Persistence contract:
- Account kind storage MUST use a foreign key to kind catalog (`KindId`).
- Legacy enum-backed account kind values MUST be migrated deterministically to seeded system catalog rows.
- Existing account records MUST preserve their semantic kind after migration.

#### Scenario: Existing account kind is preserved during migration
- **WHEN** an account record has a legacy enum kind value before migration
- **THEN** migration MUST assign the corresponding seeded catalog `KindId`
- **AND** the migrated account MUST retain equivalent semantic category in UI/API representations

#### Scenario: New account stores catalog kind reference
- **WHEN** a new account is created with a selected kind from the unified catalog
- **THEN** persistence MUST store the selected catalog `KindId`
- **AND** no enum-kind storage field MUST remain as source of truth

### Requirement: Kind Keys SHALL Be Stable for Future Semantic Features
Kind identity used by future filters/logic MUST be stable and independent of display name.

Key governance contract:
- System kind keys MUST be immutable after seeding.
- Custom kind names MAY be editable without changing historical account references.
- Future features MUST target kind identity (`Id`/`Key`) and MUST NOT depend on localized display labels.

#### Scenario: Renaming kind name does not break identity
- **WHEN** a custom kind display name is changed
- **THEN** existing account references to that kind MUST remain valid
- **AND** identity used by future features MUST remain stable

#### Scenario: Duplicate generated key receives a unique suffix
- **WHEN** a custom kind creation operation normalizes to a key that already exists
- **THEN** the system MUST generate a unique suffixed key before persisting the new kind
- **AND** persisted kind identity uniqueness MUST remain intact
