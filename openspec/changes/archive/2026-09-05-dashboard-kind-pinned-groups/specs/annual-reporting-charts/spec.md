## MODIFIED Requirements

### Requirement: Dashboard SHALL Provide Annual Income Versus Expense Month-Result Bars

Dashboard annual comparison MUST visualize monthly operational magnitude for Income and Expense and the corresponding monthly result.

#### Scenario: Dashboard annual mixed chart renders all month buckets

- **WHEN** the dashboard annual comparison is rendered for selected year `Y`
- **THEN** it MUST include January through December in deterministic order
- **AND** each available month MUST render paired Income and Expense bars plus one Result line using `Income - Expense`.

#### Scenario: Dashboard annual mixed chart preserves magnitude comparability

- **WHEN** annual Income and Expense bars are rendered in dashboard context
- **THEN** both bar series MUST use absolute magnitude values
- **AND** the Result line MUST use its signed monthly result
- **AND** labels/subtitles MUST communicate month-result semantics.

#### Scenario: Existing annual bar callers remain compatible

- **WHEN** an existing caller does not configure a line series
- **THEN** the reusable annual chart component MUST render its existing grouped-bar behavior unchanged.

