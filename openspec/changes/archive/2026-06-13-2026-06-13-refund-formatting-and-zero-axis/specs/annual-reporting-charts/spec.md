## MODIFIED Requirements

### Requirement: Annual Report Charts SHALL Follow Premium Comparative Visualization Standards
Annual charts MUST adopt a premium comparative visualization style that is consistent across annual state-evolution and composition contexts, and MUST consume shared token-governed chart primitives.

#### Scenario: Annual chart Y-axis zero baseline is visually emphasized
- **WHEN** an annual chart Y axis includes tick value `0`
- **THEN** the grid line at `0` MUST be rendered with stronger emphasis than non-zero Y-grid lines
- **AND** non-zero Y-grid lines MUST preserve baseline readability style

#### Scenario: Annual chart money labels use EUR suffix format
- **WHEN** annual chart Y-axis ticks or tooltip values render monetary values
- **THEN** values MUST use European numeric formatting and trailing euro symbol (`XXX,XX €`)
- **AND** sign semantics MUST remain unchanged
