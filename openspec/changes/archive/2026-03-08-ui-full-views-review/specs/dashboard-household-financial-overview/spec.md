## ADDED Requirements

### Requirement: Dashboard Month Context SHALL Be Explicitly Current-Month
Dashboard copy and context labels MUST explicitly reference the current month and avoid selected-month ambiguity.

#### Scenario: Dashboard subtitle uses current-month terminology
- **WHEN** an authenticated user opens `/`
- **THEN** the dashboard month-context subtitle MUST use localized wording equivalent to `Current month`
- **AND** wording equivalent to `Selected month` MUST NOT be shown in that context

#### Scenario: Dashboard keeps fixed current-month context in this change
- **WHEN** the dashboard overview is rendered
- **THEN** no month selector control MUST be required to interpret headline KPI and chart blocks
- **AND** current-month KPI and previous-month delta semantics MUST remain intact
