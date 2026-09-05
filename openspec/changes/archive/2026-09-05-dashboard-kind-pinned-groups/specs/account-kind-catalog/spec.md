## ADDED Requirements

### Requirement: Dashboard Expense Aggregation SHALL Use Catalog-Backed Kind Identity

Dashboard expense aggregation MUST group eligible expense-account movement by the account’s `AccountKindCatalog` identity rather than by legacy enum value, display label alone, or account-group membership.

#### Scenario: Custom and system expense kinds are aggregated consistently

- **WHEN** a dashboard expense ranking is requested
- **THEN** both system and active custom expense kinds assigned to expense accounts MUST be eligible for aggregation
- **AND** grouping identity MUST remain stable when labels are not unique.

#### Scenario: Kind aggregation does not expand reporting-insight dimensions implicitly

- **WHEN** dashboard kind aggregation is implemented
- **THEN** it MUST NOT add a partially supported `Kind` value to `ReportingInsightDimension`
- **AND** existing Pareto, anomaly, and insight API semantics MUST remain unchanged.

