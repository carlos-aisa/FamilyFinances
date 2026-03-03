## ADDED Requirements

### Requirement: Dashboard SHALL Support Compact Insights List Derived From Reporting Insights
The dashboard MUST expose a compact analytical list derived from existing insight semantics for at-a-glance interpretation.

#### Scenario: Compact list reuses existing insight semantics
- **WHEN** dashboard compact insights are rendered
- **THEN** items MUST use existing reporting insight semantics (for example top contributors or anomaly statuses)
- **AND** values MUST remain aligned with source insight datasets

#### Scenario: Compact list applies deterministic row cap
- **WHEN** more insights are available than dashboard capacity
- **THEN** the dashboard MUST limit rendered rows to configured cap
- **AND** row ordering MUST follow deterministic priority rules

### Requirement: Dashboard Insights SHALL Preserve Readability In Dense Viewports
Dashboard insights list MUST remain readable in constrained analytical layout.

#### Scenario: Dense layout keeps key columns scannable
- **WHEN** compact insights list is rendered within dashboard right-column block
- **THEN** key columns (name, amount, percentage or status) MUST remain visually scannable
- **AND** text overflow handling MUST avoid clipping critical numeric values
