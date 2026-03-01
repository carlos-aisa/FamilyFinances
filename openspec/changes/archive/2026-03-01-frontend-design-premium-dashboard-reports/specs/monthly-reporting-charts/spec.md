## ADDED Requirements

### Requirement: Monthly Report Charts SHALL Use A Shared Premium Visual Contract
Month-level charts MUST render with a shared premium visual contract for chart container, typography, axes, grid, and tooltip presentation.

#### Scenario: Month-focused chart panels use premium chart framing
- **WHEN** a monthly report chart is rendered in integrated report tabs
- **THEN** chart container, title/subtitle, and action controls MUST use shared premium chart panel styles
- **AND** chart framing MUST remain consistent across monthly chart contexts

#### Scenario: Monthly chart axis and tooltip styling are tokenized
- **WHEN** monthly chart canvases are rendered
- **THEN** axis ticks, grid lines, and tooltip visuals MUST derive from shared design tokens
- **AND** style choices MUST preserve readability in dark mode by default

### Requirement: Monthly Chart Styling SHALL Preserve Existing Data Semantics
Visual upgrades to monthly charts MUST NOT alter the dataset semantics, day ordering, or sign interpretation provided by existing monthly chart endpoints.

#### Scenario: Month selection still drives deterministic chart reload and ordering
- **WHEN** the selected month changes in a report view
- **THEN** monthly chart requests MUST still reload for the selected month and render points in payload order
- **AND** premium chart styling MUST not modify series value meaning or ordering

