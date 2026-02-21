## ADDED Requirements

### Requirement: Reporting SHALL Provide Pareto Insights for Expense and Income
The reporting system MUST provide ranking insights identifying top contributors for expense and income dimensions.

#### Scenario: Expense Pareto insight returns top contributors
- **WHEN** an authorized user requests expense insights for a valid period
- **THEN** the system MUST return ranked expense groups with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total expense

#### Scenario: Income Pareto insight returns top contributors
- **WHEN** an authorized user requests income insights for a valid period
- **THEN** the system MUST return ranked income groups with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total income

### Requirement: Reporting SHALL Provide Concentration Indicators
The reporting system MUST provide concentration indicators for selected dimensions.

#### Scenario: Concentration indicator provides top-N share
- **WHEN** concentration insight is computed for expense or income groups
- **THEN** the system MUST provide share of total represented by top-N contributors
- **AND** the denominator total MUST be explicit in the response

### Requirement: Reporting SHALL Provide Explainable Anomaly Indicators
The reporting system MUST provide deterministic anomaly indicators for unusual group behavior.

#### Scenario: Monthly group value outside configured threshold is flagged
- **WHEN** a group's monthly value exceeds anomaly threshold rules against historical baseline
- **THEN** the system MUST mark the group/month as anomalous
- **AND** the response MUST include threshold/baseline context sufficient to explain the flag

#### Scenario: Insufficient history returns non-anomalous explanatory state
- **WHEN** historical data is insufficient for anomaly determination
- **THEN** the system MUST return an explicit "insufficient history" result
- **AND** the system MUST NOT emit anomaly flags for that case

