## ADDED Requirements

### Requirement: System SHALL Provide Monthly YTD Evolution Series For Expense Total
The system MUST provide a monthly year-to-date evolution series for aggregate expense scope.

#### Scenario: Expense total scope returns single deterministic series
- **WHEN** an authorized user requests monthly evolution with `scope=expense-total` for year `Y`
- **THEN** the system MUST return exactly one expense-total series
- **AND** each point MUST include `EndBalanceCents`, `DeltaVsPreviousMonthCents`, and `DeltaVsYearStartCents`

#### Scenario: Expense total scope rejects invalid query combinations
- **WHEN** a request includes invalid `year` or unsupported scope token for expense-total evolution
- **THEN** the API MUST return `400 BadRequest`
- **AND** no partial payload MUST be emitted

### Requirement: Group Evolution Read Models SHALL Expose Exact Selected-Month Balance Context
Group-evolution read models MUST support exact selected-month balance interpretation for list/chart coordination.

#### Scenario: Selected-month list context uses exact month balance
- **WHEN** group evolution is rendered for selected month `M`
- **THEN** the list context MUST expose exact balance for month `M`
- **AND** the value MUST align with the underlying evolution series for that same month

#### Scenario: Selected-month context remains deterministic across refresh
- **WHEN** selected month changes and data reload occurs
- **THEN** list and chart contexts MUST reference the same selected month
- **AND** month context labels MUST remain explicit
