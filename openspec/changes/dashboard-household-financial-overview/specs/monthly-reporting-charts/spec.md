## ADDED Requirements

### Requirement: Monthly Income-Expense Charts SHALL Be Reusable In Dashboard And Report Contexts
Monthly Income vs Expense chart contracts MUST be reusable across Dashboard and report pages without semantic divergence.

#### Scenario: Dashboard and report render equivalent monthly semantics
- **WHEN** monthly Income vs Expense charts are rendered in Dashboard and Economic State contexts
- **THEN** both views MUST represent the same underlying monthly values for the same period
- **AND** chart series meaning MUST remain consistent across contexts

#### Scenario: Reused chart contract preserves deterministic ordering
- **WHEN** monthly chart datasets are loaded for a selected month
- **THEN** points MUST render in payload order
- **AND** frontend style layers MUST NOT alter day ordering or value interpretation

### Requirement: Monthly Dashboard Chart SHALL Respect Fixed-Height Analytical Layout Contract
The dashboard monthly chart container MUST support constrained fixed-height rendering for glanceability.

#### Scenario: Fixed-height chart remains readable in dashboard contract
- **WHEN** the dashboard monthly chart is rendered in the defined desktop analytical layout
- **THEN** the chart MUST remain readable without requiring internal scroll
- **AND** axis/tooltip styling MUST preserve contrast and legibility in dark mode

#### Scenario: Constrained chart layout avoids tab-based overflow patterns
- **WHEN** dashboard analytical blocks are rendered together
- **THEN** monthly chart placement MUST not depend on tabbed grouping
- **AND** core chart interpretation MUST remain available in the primary dashboard viewport
