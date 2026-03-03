# reporting-insights Specification

## Purpose
TBD - created by archiving change reporting-0-9-5-insights-pareto-anomalies. Update Purpose after archive.
## Requirements
### Requirement: Reporting SHALL Provide Pareto Insights for Expense and Income by Group and Payee
The reporting system MUST provide ranking insights identifying top contributors for expense and income across both dimensions: account groups and payees.

#### Scenario: Expense Pareto by group returns top contributors
- **WHEN** an authorized user requests expense insights for a valid period
- **THEN** the system MUST return ranked expense groups with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total expense

#### Scenario: Expense Pareto by payee returns top contributors
- **WHEN** an authorized user requests expense insights for a valid period using payee dimension
- **THEN** the system MUST return ranked payees with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total expense

#### Scenario: Income Pareto by group returns top contributors
- **WHEN** an authorized user requests income insights for a valid period
- **THEN** the system MUST return ranked income groups with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total income

#### Scenario: Income Pareto by payee returns top contributors
- **WHEN** an authorized user requests income insights for a valid period using payee dimension
- **THEN** the system MUST return ranked payees with contribution amount and contribution percentage
- **AND** the response MUST include top-N coverage percentage over total income

### Requirement: Reporting SHALL Provide Concentration Indicators
The reporting system MUST provide concentration indicators for selected dimensions.

#### Scenario: Concentration indicator provides top-N share for selected dimension
- **WHEN** concentration insight is computed for expense or income over groups or payees
- **THEN** the system MUST provide share of total represented by top-N contributors
- **AND** the denominator total MUST be explicit in the response

### Requirement: Reporting SHALL Provide Explainable Anomaly Indicators
The reporting system MUST provide deterministic anomaly indicators for unusual group or payee behavior.

#### Scenario: Monthly group or payee value outside configured threshold is flagged
- **WHEN** a group's or payee's monthly value exceeds anomaly threshold rules against historical baseline
- **THEN** the system MUST mark the contributor/month as anomalous
- **AND** the response MUST include threshold/baseline context sufficient to explain the flag

#### Scenario: Insufficient history returns non-anomalous explanatory state
- **WHEN** historical data is insufficient for anomaly determination
- **THEN** the system MUST return an explicit "insufficient history" result
- **AND** the system MUST NOT emit anomaly flags for that case

### Requirement: Insights UI SHALL Render Premium Analytical Panels Without Changing Insight Semantics
The reporting insights surface MUST use premium panel composition for Pareto and anomaly sections while preserving current insight semantics and ranking logic.

#### Scenario: Pareto and anomaly blocks keep data semantics with upgraded framing
- **WHEN** monthly summary insights are rendered
- **THEN** expense and income Pareto sections and anomaly breakdown sections MUST preserve existing dimensions and values
- **AND** panel framing, headers, and badges MUST use shared premium analytical panel styles

### Requirement: Insights Tables SHALL Preserve Dense-Data Readability
Insights contributor tables MUST remain readable in dense contexts through consistent premium table styling and numeric alignment rules.

#### Scenario: Contributor tables preserve financial readability
- **WHEN** Pareto or anomaly contributor rows are displayed
- **THEN** amount and percentage columns MUST remain right-aligned and visually scannable
- **AND** row density, borders, and typography MUST follow shared premium table primitives

### Requirement: Insights Dimension Controls SHALL Expose Clear Premium State Feedback
Dimension toggles for Groups/Payees MUST maintain current behavior and MUST provide clear premium active-state cues.

#### Scenario: Dimension switch keeps behavior and adds visual clarity
- **WHEN** a user switches insights dimension between `Group` and `Payee`
- **THEN** existing data reload behavior MUST remain intact
- **AND** control styling MUST clearly indicate active and inactive states in dark mode

### Requirement: Dashboard SHALL Use Insight Aggregation For Expense Top-N Plus Others Composition
Dashboard expense composition MUST be derived from reporting insight aggregation semantics and remain stable as category cardinality grows.

#### Scenario: Expense Top-N composition is insight-driven
- **WHEN** dashboard expense composition is rendered for the selected month
- **THEN** slice values MUST be sourced from reporting insight-compatible aggregation datasets
- **AND** dashboard values MUST remain consistent with report-level expense totals for the same period

#### Scenario: Others slice closes the composition sum
- **WHEN** expense contributors exceed configured Top-N capacity
- **THEN** non-Top-N contributors MUST be aggregated into `Others`
- **AND** the sum of `Top-N + Others` MUST equal the selected-month total expense magnitude

### Requirement: Dashboard Insight Visualization SHALL Prioritize Chart Readability Over Dense Tables
Dashboard insight representation MUST be chart-first in the primary cockpit layout.

#### Scenario: Insight consumption avoids growth-prone table blocks
- **WHEN** dashboard renders insight blocks in desktop cockpit mode
- **THEN** expense insight MUST be presented as composition chart(s) instead of a vertically growing insight table
- **AND** numeric comparability MUST remain available through chart legend labels/tooltips

#### Scenario: Sparse data remains explicit
- **WHEN** selected-month expense insight data is partial or missing
- **THEN** dashboard MUST render explicit empty/partial state messaging
- **AND** chart block footprint MUST remain layout-stable

