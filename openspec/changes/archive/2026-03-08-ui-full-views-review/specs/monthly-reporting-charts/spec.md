## ADDED Requirements

### Requirement: Monthly Evolution Charts SHALL Use One Shared Visual Contract Across Views
All month-level evolution charts MUST use one shared chart contract in dashboard and report contexts.

#### Scenario: Monthly chart structure and styling is consistent across reporting surfaces
- **WHEN** a monthly evolution chart is rendered in dashboard or reports
- **THEN** axes, legend placement, tooltip behavior, and panel framing MUST follow one shared rendering contract
- **AND** chart-specific visual forks between equivalent monthly views MUST NOT be introduced

#### Scenario: Monthly charts show explicit current-period cutoff treatment
- **WHEN** a monthly chart is rendered for a period that includes future days relative to the current date context
- **THEN** the chart MUST render a visible cutoff marker at the current day boundary
- **AND** the future area after the cutoff MUST be visually de-emphasized with disabled/shaded styling
- **AND** line segments after cutoff MUST use a clearly de-emphasized style (for example dashed)

### Requirement: Monthly Chart Panels SHALL Reuse Standard Action Controls
Monthly chart panels MUST use the same action-control primitives as the rest of the application.

#### Scenario: Export controls match standard button primitives
- **WHEN** monthly chart panels expose export actions
- **THEN** those controls MUST reuse shared standard button primitives used elsewhere in the app
- **AND** control geometry and radius MUST remain aligned with non-chart actions
