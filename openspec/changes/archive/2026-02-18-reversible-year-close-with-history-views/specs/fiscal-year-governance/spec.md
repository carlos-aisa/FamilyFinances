## ADDED Requirements

### Requirement: System SHALL Support Reversible Fiscal Year Close State
The system MUST maintain explicit close state per fiscal year and MUST allow reopening a previously closed year.

#### Scenario: Close year marks state as closed
- **WHEN** an authorized user executes fiscal-year close for year `Y`
- **THEN** the system MUST persist year `Y` with `IsClosed = true`
- **AND** the system MUST persist close metadata timestamp and actor when available

#### Scenario: Reopen year marks state as open
- **WHEN** an authorized user executes fiscal-year reopen for year `Y`
- **THEN** the system MUST persist year `Y` with `IsClosed = false`
- **AND** the system MUST persist reopen metadata timestamp and actor when available

### Requirement: Year Close SHALL Generate Account Year-End Snapshots
The system MUST compute and persist one closing balance snapshot per account for the closed year.

#### Scenario: Closing year computes snapshots for all accounts
- **WHEN** year `Y` is closed
- **THEN** the system MUST compute each account balance as of `Y-12-31`
- **AND** the system MUST persist `AccountYearSnapshot` records keyed by `(Year, AccountId)` with closing balance value

### Requirement: Year Reopen SHALL Invalidate Closed-Year Snapshot Baseline
The system MUST ensure snapshots from a reopened year are not treated as authoritative until the year is closed again.

#### Scenario: Reopened year snapshots are invalidated
- **WHEN** year `Y` transitions from closed to open
- **THEN** snapshot records for year `Y` MUST be removed or marked unusable for baseline calculations
- **AND** next close operation for year `Y` MUST recompute snapshots from ledger data

### Requirement: Closed Years SHALL Reject Mutation Operations
The system MUST reject ledger mutation attempts targeting booked dates in closed years.

#### Scenario: Create transaction in closed year is rejected
- **WHEN** a create transaction request has `BookedOn.Year = Y` and year `Y` is closed
- **THEN** the system MUST reject the request with a domain/business validation error

#### Scenario: Update transaction in closed year is rejected
- **WHEN** an update request targets a transaction booked in year `Y` and year `Y` is closed
- **THEN** the system MUST reject the request with a domain/business validation error

#### Scenario: Delete transaction in closed year is rejected
- **WHEN** a delete request targets a transaction booked in year `Y` and year `Y` is closed
- **THEN** the system MUST reject the request with a domain/business validation error

#### Scenario: Reconcile account in closed year is rejected
- **WHEN** a reconcile request has `AsOfDate.Year = Y` and year `Y` is closed
- **THEN** the system MUST reject the request with a domain/business validation error

### Requirement: Movement Running Balance SHALL Use Snapshot Baseline When Available
Running-balance computation MUST use year snapshot baseline to avoid full-history recalculation when compatible baseline exists.

#### Scenario: Baseline starts from nearest available snapshot
- **WHEN** account movement query range starts after a year-end snapshot for the account
- **THEN** running balance MUST initialize from that snapshot baseline
- **AND** the query MUST apply only required in-range deltas after baseline

#### Scenario: Snapshot fallback remains functional
- **WHEN** no compatible snapshot exists
- **THEN** the system MUST fallback to the legacy full-history baseline behavior
- **AND** returned balances MUST remain numerically correct

### Requirement: Fiscal Year Governance API SHALL Expose Close/Reopen/Status Contracts
The system MUST expose governance API operations for status listing and reversible close actions.

#### Scenario: Year status list contract
- **WHEN** client calls fiscal-year status listing endpoint
- **THEN** response MUST include year, closed/open status, and close/reopen metadata fields

#### Scenario: Close and reopen command contracts
- **WHEN** client calls close or reopen endpoint for year `Y`
- **THEN** endpoint MUST execute idempotent-safe governance behavior and return success/failure semantics without changing API version pattern
