## MODIFIED Requirements

### Requirement: Account-Group Totals SHALL Display And Navigate From Movements

The account-group totals report MUST display the selected group's filtered movement rows with date, description, source, destination, group net amount, and accumulation. A movement row MUST link to its transaction detail and preserve group, period, and nature context for return navigation. Movement rows MUST be hosted in a reporting card that shows the active reporting period and provides a CSV export action when rows are available.

#### Scenario: Transaction detail returns to the same report context

- **WHEN** a user opens a transaction from an account-group movement row and uses Back or Edit navigation
- **THEN** the generated link MUST target the account-group totals report
- **AND** it MUST retain the original group, period, and nature filter values.

#### Scenario: No matching movement communicates the filtered state

- **WHEN** the selected group has no matching movements
- **THEN** the report MUST render localized empty-state feedback
- **AND** it MUST retain the group totals report and its active filters
- **AND** the movement card MUST NOT offer a CSV export action.

#### Scenario: Movement card exports the filtered rows

- **WHEN** movement rows are available for the selected group and active report filters
- **THEN** the movement card MUST expose the standard CSV export action
- **AND** the generated CSV MUST contain the same newest-first rows shown in the table
- **AND** it MUST identify the selected account group, reporting period, and active nature filter when one is applied.
