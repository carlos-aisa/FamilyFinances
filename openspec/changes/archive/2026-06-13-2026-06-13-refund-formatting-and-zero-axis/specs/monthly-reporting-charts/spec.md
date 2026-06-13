## MODIFIED Requirements

### Requirement: Monthly Report Charts SHALL Use A Shared Premium Visual Contract
Month-level charts MUST render with a shared premium visual contract for chart container, typography, axes, grid, tooltip presentation, and semantic palette resolution sourced from shared token governance.

#### Scenario: Monthly chart Y-axis zero baseline is visually emphasized
- **WHEN** a monthly chart Y axis includes tick value `0`
- **THEN** the grid line at `0` MUST be rendered with stronger emphasis than non-zero Y-grid lines
- **AND** non-zero Y-grid lines MUST preserve baseline readability style

#### Scenario: Monthly chart money labels use EUR suffix format
- **WHEN** Y-axis ticks or tooltip values render monetary values
- **THEN** values MUST use European numeric formatting and trailing euro symbol (`XXX,XX €`)
- **AND** sign semantics MUST remain unchanged
