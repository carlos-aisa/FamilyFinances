## ADDED Requirements

### Requirement: Account-Group Movements SHALL Be Available For Selected Reporting Filters

The system MUST expose an authenticated `GET /api/v1/reports/account-groups/{groupId}/movements` operation that requires `from` and `to` date parameters and accepts an optional `nature` parameter. It MUST return movements for the requested group within `[from, to)` without modifying ledger data.

#### Scenario: Filtered movements include every transaction touching a member account

- **WHEN** a transaction in the selected period contains one or more splits for an account that belongs to the selected group and satisfies the optional nature filter
- **THEN** the response MUST contain exactly one movement row for that transaction
- **AND** the row MUST include the names of all source and destination accounts from the transaction.

#### Scenario: Empty group result remains a successful report response

- **WHEN** the group exists but has no members or no matching splits in the selected filters
- **THEN** the operation MUST return a successful response with an empty item collection.

#### Scenario: Missing group is not treated as an empty result

- **WHEN** the requested group does not exist
- **THEN** the operation MUST return the existing reporting not-found behavior.

### Requirement: Account-Group Movement Amounts SHALL Be Deterministic

Each movement row MUST expose the group's signed net amount and a running net amount. The running net MUST begin at zero at the beginning of the selected range, use chronological booking-date order with deterministic ties, and rows MUST be returned newest first.

#### Scenario: Multi-split transaction is aggregated once

- **WHEN** multiple matching splits of one transaction belong to the selected group
- **THEN** their signed amounts MUST be aggregated into one group net amount
- **AND** the response MUST NOT duplicate the transaction row.

#### Scenario: Internal transfer is represented with both account sides

- **WHEN** a matching transaction transfers value between accounts
- **THEN** the movement row MUST include its source and destination account names
- **AND** it MUST remain eligible for the group drill-down when a participating account belongs to the group.

### Requirement: Account-Group Totals SHALL Display And Navigate From Movements

The account-group totals report MUST display the selected group's filtered movement rows with date, description, source, destination, group net amount, and accumulation. A movement row MUST link to its transaction detail and preserve group, period, and nature context for return navigation.

#### Scenario: Transaction detail returns to the same report context

- **WHEN** a user opens a transaction from an account-group movement row and uses Back or Edit navigation
- **THEN** the generated link MUST target the account-group totals report
- **AND** it MUST retain the original group, period, and nature filter values.

#### Scenario: No matching movement communicates the filtered state

- **WHEN** the selected group has no matching movements
- **THEN** the report MUST render localized empty-state feedback
- **AND** it MUST retain the group totals report and its active filters.
