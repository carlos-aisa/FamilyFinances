## ADDED Requirements

### Requirement: Account Movements View SHALL Display Running Balance Evolution
The system MUST display the running account balance for each movement row in the account movements list so users can observe balance evolution across the selected period.

#### Scenario: Running balance shown per movement row
- **WHEN** an authenticated user opens `/accounts/{id}/movements` and movements are returned
- **THEN** each movement row MUST display its `RunningBalance` value in a dedicated running-balance column

#### Scenario: Running balance uses backend-provided value
- **WHEN** movement data is rendered in the account movements table
- **THEN** the running-balance value MUST come from `AccountMovementDto.RunningBalance` without frontend recomputation

#### Scenario: Running balance formatting remains monetary
- **WHEN** a running-balance value is displayed
- **THEN** the value MUST be formatted as currency with sign-preserving semantics consistent with current amount formatting behavior
