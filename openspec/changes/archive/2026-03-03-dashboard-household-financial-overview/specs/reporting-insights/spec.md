## ADDED Requirements

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
