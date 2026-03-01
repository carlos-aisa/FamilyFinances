## ADDED Requirements

### Requirement: Annual Report Charts SHALL Follow Premium Comparative Visualization Standards
Annual charts MUST adopt a premium comparative visualization style that is consistent across annual state-evolution and composition contexts.

#### Scenario: Annual chart containers and legends are visually consistent
- **WHEN** annual evolution and composition charts are rendered
- **THEN** chart cards, headers, badges, and legends MUST use shared premium chart styling primitives
- **AND** interaction affordances (hover/focus/export controls) MUST remain consistent across annual chart components

#### Scenario: Annual chart readability is preserved on dashboard and report dark surfaces
- **WHEN** annual charts are displayed in dark mode
- **THEN** axis, grid, and dataset contrast MUST remain readable on premium dark surfaces
- **AND** labels and legends MUST remain legible without changing chart data semantics

### Requirement: Annual Chart Restyling SHALL Preserve Existing Series Contracts
The premium redesign of annual charts MUST preserve existing series identity contracts for key, display name, and point semantics.

#### Scenario: Annual account-group series identity remains stable
- **WHEN** account-group annual evolution charts are rendered with premium styling
- **THEN** each group series MUST keep stable keys and labels
- **AND** restyling MUST NOT alter the mapping between rendered series and underlying monthly values

