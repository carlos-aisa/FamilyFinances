## ADDED Requirements

### Requirement: Dashboard SHALL Provide Deterministic Latest Expense Movements

The system MUST provide an authorized read-only source of the six most recent transactions containing at least one split on an Expense-nature account. Results MUST be ordered by booked date descending and transaction identifier descending.

#### Scenario: Latest Expense endpoint returns only Expense-related transactions

- **WHEN** an authorized user requests the latest expenses endpoint
- **THEN** the response MUST contain no more than six transactions with at least one Expense-nature split
- **AND** transactions without an Expense-nature split MUST be excluded

#### Scenario: Same-date movements use deterministic identifier ordering

- **WHEN** eligible transactions share the same booked date
- **THEN** the response MUST order those transactions by transaction identifier descending

### Requirement: Dashboard SHALL Render Latest Expenses Through A Reusable Movement List

The Dashboard MUST render its latest Expense movements with a reusable visual list component that accepts display-ready movement items and is independent from the concrete query source.

#### Scenario: Latest expense item presents neutral spending information

- **WHEN** a latest Expense movement is rendered
- **THEN** the item MUST show its date, available description, and amount magnitude
- **AND** the amount MUST be non-negative and MUST NOT use unfavorable-result color semantics

#### Scenario: Missing description remains renderable

- **WHEN** a latest Expense movement has no description
- **THEN** the movement list MUST render the item without failing
- **AND** it MUST preserve the date and amount columns
