## MODIFIED Requirements

### Requirement: Annual Report Charts SHALL Follow Premium Comparative Visualization Standards
Annual charts MUST adopt a premium comparative visualization style that is consistent across annual state-evolution and composition contexts, and MUST consume shared token-governed chart primitives.

#### Scenario: Annual chart containers and legends are visually consistent
- **WHEN** annual evolution and composition charts are rendered
- **THEN** chart cards, headers, badges, and legends MUST use shared premium chart styling primitives
- **AND** interaction affordances (hover/focus/export controls) MUST remain consistent across annual chart components
- **AND** annual chart container and legend dimensions MUST resolve via shared sizing tokens

#### Scenario: Annual chart readability is preserved on dashboard and report dark surfaces
- **WHEN** annual charts are displayed in dark mode
- **THEN** axis, grid, and dataset contrast MUST remain readable on premium dark surfaces
- **AND** labels and legends MUST remain legible without changing chart data semantics
- **AND** runtime chart style fallback values MUST come from shared semantic token mappings

### Requirement: Annual Chart Restyling SHALL Preserve Existing Series Contracts
The premium redesign of annual charts MUST preserve existing series identity contracts for key, display name, and point semantics while removing page-specific style literal drift.

#### Scenario: Annual account-group series identity remains stable
- **WHEN** account-group annual evolution charts are rendered with premium styling
- **THEN** each group series MUST keep stable keys and labels
- **AND** restyling MUST NOT alter the mapping between rendered series and underlying monthly values

#### Scenario: Annual semantic colors are resolved centrally
- **WHEN** annual chart datasets are prepared for rendering
- **THEN** semantic series colors and indexed fallback colors MUST be resolved by shared palette helpers
- **AND** page/component-level hardcoded chart color literals MUST NOT be required for default annual chart rendering
