## ADDED Requirements

### Requirement: Reporting MUST Expose Canonical Metric Semantics
The system MUST define a canonical semantic dictionary for reporting metrics, including formula intent and comparability class.

#### Scenario: Canonical dictionary includes stock and flow metrics
- **WHEN** reporting semantics are loaded for a report page
- **THEN** the dictionary MUST include at least `Asset Balance`, `Liability Balance`, `Net Worth`, `Income`, `Expense`, and `Period Net Result`
- **AND** each metric MUST be classified as stock (`balance at time`) or flow (`result over period`)

### Requirement: KPI and Chart Labels MUST Map to Canonical Metrics
Every KPI card and chart series shown in reporting pages MUST map deterministically to one canonical metric definition.

#### Scenario: KPI card label is deterministic
- **WHEN** a KPI card is rendered in any reporting page
- **THEN** its label MUST identify the canonical metric represented by the numeric value
- **AND** equivalent formulas across pages MUST use equivalent canonical naming

#### Scenario: Semantic mismatch is not silently displayed
- **WHEN** two metrics are numerically present in the same view but are not comparable (stock vs flow)
- **THEN** the UI MUST display an explicit disclaimer that they are different metric families

### Requirement: Semantic Alignment MUST Be Regression-Tested
Semantic mapping behavior MUST be protected with deterministic automated tests.

#### Scenario: Semantic tests fail on label-formula drift
- **WHEN** a report page label changes without the corresponding canonical metric mapping
- **THEN** the semantic consistency test suite MUST fail

