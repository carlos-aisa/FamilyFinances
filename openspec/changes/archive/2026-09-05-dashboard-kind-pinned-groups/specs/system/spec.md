## MODIFIED Requirements

### Requirement: Account Groups SHALL Support Group CRUD and Membership Management

Account groups SHALL support their existing CRUD and membership operations plus an explicit dashboard-monitoring preference.

#### Scenario: Account group exposes dashboard pin preference

- **WHEN** an account group is created or retrieved
- **THEN** its DTO MUST expose `IsDashboardPinned`
- **AND** a newly created group MUST default to `false`.

#### Scenario: User updates dashboard pin preference through general partial update

- **WHEN** an authorized writer sends `PATCH /api/v{version:apiVersion}/account-groups/{id}` with a valid `IsDashboardPinned` value
- **THEN** the system MUST persist that value and return `204 No Content`
- **AND** subsequent group reads MUST expose the persisted value.

#### Scenario: Pin update of an unknown group returns not found

- **WHEN** an authorized writer sends the general PATCH request for an unknown account-group ID
- **THEN** the API MUST return `404 NotFound`.

#### Scenario: Existing rename endpoint remains compatible

- **WHEN** an authorized writer uses `PATCH /api/v{version:apiVersion}/account-groups/{id}/rename`
- **THEN** existing rename behavior and response semantics MUST remain unchanged.

