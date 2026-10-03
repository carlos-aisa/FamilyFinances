## MODIFIED Requirements

### Requirement: Dashboard SHALL Provide Deterministic Latest Expense Movements

The system MUST provide an authorized read-only source of the six most recent transactions containing at least one split on an Expense-nature account. When a Dashboard year and month are supplied together, results MUST be restricted to that complete calendar month. Results MUST be ordered by booked date descending and transaction identifier descending.

#### Scenario: Latest Expense endpoint returns only Expense-related transactions

- **WHEN** an authorized user requests the latest expenses endpoint
- **THEN** the response MUST contain no more than six transactions with at least one Expense-nature split
- **AND** transactions without an Expense-nature split MUST be excluded.

#### Scenario: Latest Expense endpoint scopes a requested month

- **WHEN** an authorized user requests latest expenses with a valid year and month
- **THEN** every returned transaction MUST have a booked date within that calendar month
- **AND** the response MUST contain no more than six qualifying transactions.

#### Scenario: Same-date movements use deterministic identifier ordering

- **WHEN** eligible transactions share the same booked date
- **THEN** the response MUST order those transactions by transaction identifier descending.
