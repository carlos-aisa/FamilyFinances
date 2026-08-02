## MODIFIED Requirements

### Requirement: Semantic mismatch is not silently displayed
When two metrics are numerically present in the same view but are not comparable, the UI MUST display an explicit disclaimer that they are different metric families. In Asset Evolution, the monthly Asset-account balance delta MUST be labeled as asset movement and MUST be distinguished from Snapshot `Income + Expense` period net result.

#### Scenario: Asset movement is explicitly distinguished from period net result
- **WHEN** a user views the Asset Evolution Monthly Overview on `/reports/economic-state`
- **THEN** its month-over-month Asset-account delta column MUST be labeled as asset movement, not a generic balance
- **AND** the panel MUST state that asset movement is a stock delta and Snapshot period net result is an income-and-expense flow
- **AND** the explanation MUST state that the two values can differ when a transaction affects Liability or Equity accounts

#### Scenario: Clarification does not alter financial values
- **WHEN** the asset semantic explanation is rendered
- **THEN** the existing Asset Evolution values, signs, and colors MUST remain derived from `DeltaVsPreviousMonthCents`
- **AND** Snapshot period net result MUST remain derived from `PeriodNetResultCents`
- **AND** the UI MUST NOT transform either value to force equality
