## MODIFIED Requirements

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
