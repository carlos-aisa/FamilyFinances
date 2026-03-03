## ADDED Requirements

### Requirement: Dashboard SHALL Provide Annual Income Versus Expense Month-Result Bars
Dashboard annual comparison MUST visualize monthly operational magnitude for Income and Expense.

#### Scenario: Dashboard annual bars render all month buckets
- **WHEN** dashboard annual Income vs Expense chart is rendered for selected year `Y`
- **THEN** the chart MUST include month buckets January through December in deterministic order
- **AND** each month MUST render paired Income and Expense bars for comparison

#### Scenario: Dashboard annual bars preserve magnitude comparability
- **WHEN** annual Income and Expense bars are rendered in dashboard context
- **THEN** both series MUST use absolute magnitude values to keep bar-height comparability
- **AND** chart subtitles/labels MUST explicitly communicate month-result semantics

### Requirement: Economic State SHALL Provide Annual Month-By-Month Income Versus Expense Bar Comparison
Annual Income versus Expense comparison in Economic State MUST be rendered as month-by-month bars.

#### Scenario: Annual Income vs Expense bars render per month
- **WHEN** annual comparison data is available for selected year `Y`
- **THEN** the UI MUST render bars for Income and Expense for each month bucket
- **AND** the chart MUST use consistent month ordering from January to December

#### Scenario: Annual comparison bars preserve flow semantics
- **WHEN** Income and Expense bars are displayed
- **THEN** series semantics MUST preserve existing income/expense meaning
- **AND** visual restyling MUST NOT alter underlying values

### Requirement: Account Group Annual Evolution SHALL Support Non-Cumulative Month-Result Bar Visualization
Account-group annual evolution in designated group-evolution context MUST support non-cumulative month-result bars.

#### Scenario: Group annual bars show month-result values
- **WHEN** account-group annual evolution is rendered in the updated state-evolution context
- **THEN** each month bar MUST represent that month's result value (not cumulative running total)
- **AND** series identity by group key and label MUST remain stable

#### Scenario: Group annual bar context remains synchronized with selected-month list
- **WHEN** selected-month context is displayed alongside annual group bars
- **THEN** list/chart interpretation MUST remain coherent
- **AND** month-result semantics MUST be clearly labeled to users
