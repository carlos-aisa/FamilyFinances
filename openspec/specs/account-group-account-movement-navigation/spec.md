# account-group-account-movement-navigation Specification

## Purpose
TBD - created by archiving change account-group-movements-csv-export. Update Purpose after archive.
## Requirements
### Requirement: Account-Breakdown Rows SHALL Open Contextual Account Movements

The account-group totals report MUST make each account-breakdown row navigable to that account's movements. The navigation MUST carry the active group-report date range and preserve the originating group and nature context.

#### Scenario: User opens an account from the breakdown

- **WHEN** a user selects an account in the account-breakdown card
- **THEN** the account movement page MUST load that account with the same inclusive and exclusive date range as the group report.

### Requirement: Contextual Account Movement Navigation SHALL Preserve Its Parent Chain

When an account movement page was opened from an account-group report, its Back action MUST return to that group report with the original group, period, and nature context. A transaction opened from that account movement page MUST return to the account movement page first, including the parent context needed for its own Back action.

#### Scenario: User returns from account movements to the group report

- **WHEN** a user uses Back from account movements opened by an account-group report
- **THEN** the target MUST be the original account-group report with its original group, date range, and nature filter.

#### Scenario: User returns from a transaction to account movements

- **WHEN** a user opens a transaction from contextually opened account movements and uses Back after viewing or editing it
- **THEN** the target MUST be that account's movement page with the active date range
- **AND** a subsequent Back action from that page MUST return to the original account-group report.

