## MODIFIED Requirements

### Requirement: Economic State Tabs SHALL Retain Existing Behavior With Premium Navigation Styling
The economic-state tabs MUST keep deterministic data-loading behavior while supporting snapshot, asset evolution, income evolution, and expense evolution views.

#### Scenario: Tab switching behavior remains deterministic with expense parity
- **WHEN** a user switches between `Snapshot`, `Asset Evolution`, `Income Evolution`, and `Expense Evolution` tabs
- **THEN** active tab behavior and rendered panel content MUST remain deterministic
- **AND** each tab MUST load the corresponding report data scope without semantic drift

## ADDED Requirements

### Requirement: Economic State Snapshot SHALL Include Monthly Net List And Annual Income-Expense Comparison
The economic-state snapshot MUST include compact monthly net listing and annual Income vs Expense bar comparison for quick interpretation.

#### Scenario: Snapshot shows monthly net list semantics
- **WHEN** snapshot data is rendered for selected period context
- **THEN** the view MUST display a monthly net list computed as `Income - Expense`
- **AND** list values MUST preserve existing sign semantics

#### Scenario: Snapshot shows annual Income vs Expense bars
- **WHEN** annual comparison data is available in economic-state snapshot context
- **THEN** the UI MUST render a month-by-month Income vs Expense bar chart
- **AND** chart labels MUST clearly communicate monthly (non-aggregated-per-bar) semantics

### Requirement: Economic State Summary SHALL Preserve Readability Under Limited History
Economic-state summary blocks MUST remain explicit when historical comparison data is sparse.

#### Scenario: Sparse history shows explicit comparison state
- **WHEN** same-month-last-year data is unavailable for summary comparisons
- **THEN** summary comparison blocks MUST display an explicit insufficient-history state
- **AND** visual layout MUST remain stable and readable
