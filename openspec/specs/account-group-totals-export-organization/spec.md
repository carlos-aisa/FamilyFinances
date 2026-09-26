# account-group-totals-export-organization Specification

## Purpose
TBD - created by archiving change account-group-movements-csv-export. Update Purpose after archive.
## Requirements
### Requirement: Account-Breakdown CSV Export SHALL Be Scoped To Its Data Card

The account-group totals report MUST render its account-breakdown table in a dedicated reporting card. The existing account-breakdown CSV action MUST be hosted in that card's header together with the active reporting period. The selected group summary header MUST NOT expose that action.

#### Scenario: User exports account breakdown data

- **WHEN** the selected group has account-breakdown rows
- **THEN** the account-breakdown card MUST expose the standard CSV export action
- **AND** the exported CSV MUST retain the active group, date-range, and nature context.

#### Scenario: Group summary remains contextual

- **WHEN** a group totals report is displayed
- **THEN** its summary header MUST show its reporting context
- **AND** it MUST NOT show the account-breakdown CSV export action.
