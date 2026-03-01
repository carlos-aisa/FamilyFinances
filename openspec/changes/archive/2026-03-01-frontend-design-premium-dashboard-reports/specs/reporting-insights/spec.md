## ADDED Requirements

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

