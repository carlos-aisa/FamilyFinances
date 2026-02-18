# Historical Ledger Views Specification

## Purpose
Define dedicated read-only historical browsing for transactions and account movements, separate from operational mutation flows.

## Requirements

### Requirement: System SHALL Provide Dedicated Historical Ledger Navigation
The Web UI MUST provide a dedicated historical section separate from operational transactions pages.

#### Scenario: User accesses history from navigation
- **WHEN** an authenticated user opens the main navigation menu
- **THEN** the UI MUST expose a `History` entry that routes to historical ledger pages
- **AND** this entry MUST be distinct from the operational `Transactions` route

### Requirement: Historical Transactions View SHALL Be Read-Only
The system MUST provide a historical transactions view that supports year-based browsing without mutation actions.

#### Scenario: Historical transactions filtered by year
- **WHEN** user selects year `Y` in historical transactions view
- **THEN** the system MUST display transactions with `BookedOn.Year = Y`

#### Scenario: Historical transaction actions are read-only
- **WHEN** historical transactions are displayed
- **THEN** create/edit/delete controls MUST NOT be available in that view

### Requirement: Historical Account Movements View SHALL Be Read-Only
The system MUST provide historical account movements view filtered by year and account, including running balance display.

#### Scenario: Historical movements filtered by year and account
- **WHEN** user selects account `A` and year `Y` in historical movements view
- **THEN** the system MUST display movement rows for account `A` booked in year `Y`

#### Scenario: Historical movement rows include running balance
- **WHEN** historical movements are rendered
- **THEN** each row MUST include running-balance value consistent with backend movement calculation semantics

#### Scenario: Historical movement actions are read-only
- **WHEN** historical movements are displayed
- **THEN** UI MUST NOT expose create/edit/delete actions in the historical view

### Requirement: Historical APIs SHALL Support Read-Only Retrieval for Transactions and Movements
The backend MUST provide read-only contracts that support historical pages.

#### Scenario: Historical transactions API contract
- **WHEN** client requests historical transactions with year filter
- **THEN** endpoint MUST return year-scoped transaction list suitable for read-only presentation

#### Scenario: Historical movements API contract
- **WHEN** client requests historical movements with account and year filters
- **THEN** endpoint MUST return year/account-scoped movements including running balance

### Requirement: Operational and Historical Views SHALL Remain Functionally Separated
The system MUST keep operational mutation flows and historical browsing flows separated in UI behavior.

#### Scenario: Operational transactions retain mutation behavior for open years
- **WHEN** user works in operational transactions pages
- **THEN** existing create/edit/delete workflows remain available subject to fiscal-year guard policies

#### Scenario: Historical pages never initiate mutation flows
- **WHEN** user navigates historical pages
- **THEN** no UI event from those pages MUST trigger mutation endpoints
