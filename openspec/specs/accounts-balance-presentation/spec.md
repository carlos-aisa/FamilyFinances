# accounts-balance-presentation Specification

## Purpose
Define the balance perspectives shown for each account in the Accounts list.
## Requirements
### Requirement: Accounts View SHALL Present Current-Month And Year-To-Date Balances Per Account
The Accounts list MUST display both the current-month balance and the current-calendar-year accumulated balance for each account row.

#### Scenario: Accounts row renders current-month and year-to-date balance
- **WHEN** an authenticated user opens the Accounts list view
- **THEN** each account row MUST include current-month and year-to-date balance fields
- **AND** both values MUST be formatted as monetary amounts with existing sign conventions

#### Scenario: Year-to-date balance excludes prior-year movements
- **WHEN** an account has movements from previous calendar years
- **THEN** its accumulated balance in the Accounts list MUST include only movements from 1 January of the current calendar year through the current month

#### Scenario: Dual-balance rendering does not remove existing account context
- **WHEN** dual balances are introduced in the Accounts list
- **THEN** existing account identity/context columns MUST remain available
- **AND** account lifecycle status visibility MUST remain preserved

### Requirement: Selected-Period Balance SHALL Default To Current Month In This Iteration
The selected-period balance shown in Accounts MUST represent current-month behavior for this iteration.

#### Scenario: Period lens defaults to current month
- **WHEN** no explicit period override is provided
- **THEN** selected-period balance MUST correspond to the current month window
- **AND** the period basis MUST be consistent across all account rows

#### Scenario: Period semantics remain read-model aligned
- **WHEN** selected-period balance values are rendered
- **THEN** values MUST be sourced from reporting/read-model semantics
- **AND** frontend MUST NOT recompute ledger semantics independently

