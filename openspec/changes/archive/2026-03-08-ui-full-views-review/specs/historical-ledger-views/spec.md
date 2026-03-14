## ADDED Requirements

### Requirement: Historical Transaction Inspection SHALL Preserve Return Context
Historical read-only inspection flows MUST preserve origin context so users return to the same historical surface.

#### Scenario: History transactions detail back returns to history transactions
- **WHEN** a user opens transaction detail from `/history/transactions`
- **THEN** the detail route MUST carry historical origin metadata
- **AND** back navigation MUST return to `/history/transactions` with the same year/filter context

#### Scenario: History movements detail back returns to history movements
- **WHEN** a user opens transaction detail from `/history/movements`
- **THEN** the detail route MUST carry historical origin metadata
- **AND** back navigation MUST return to `/history/movements` with the same account/year context
