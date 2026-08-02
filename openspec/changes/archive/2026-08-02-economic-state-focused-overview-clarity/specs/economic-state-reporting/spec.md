## MODIFIED Requirements

### Requirement: Economic State Tabs SHALL Retain Existing Behavior With Premium Navigation Styling
The economic-state tabs MUST keep deterministic data-loading behavior while supporting snapshot, asset evolution, income evolution, and expense evolution views. When a global focused month is selected, every integrated Evolution Monthly Overview MUST use that selected month as its displayed and exported reporting cutoff.

#### Scenario: Tab switching behavior remains deterministic with expense parity
- **WHEN** a user switches between `Snapshot`, `Asset Evolution`, `Income Evolution`, and `Expense Evolution` tabs
- **THEN** active tab behavior and rendered panel content MUST remain deterministic
- **AND** each tab MUST load the corresponding report data scope without semantic drift

#### Scenario: Global focused month bounds every integrated Evolution overview
- **WHEN** a user selects year `Y` and focused month `M` on `/reports/economic-state` and opens Asset, Income, or Expense Evolution
- **THEN** that tab's Monthly Overview MUST show exactly the ordered months `1` through `M`
- **AND** the final visible row, context badge, and selected-period marker MUST identify month `M`
- **AND** the overview MUST NOT identify a later system current month as the selected reporting context

#### Scenario: Snapshot and Evolution tabs retain their distinct financial semantics
- **WHEN** a user compares Snapshot period net result with Asset Evolution monthly movement for the same focused month
- **THEN** Snapshot MUST continue to display the income-and-expense period result
- **AND** Asset Evolution MUST continue to display the month-over-month delta of Asset-account balances
- **AND** the UI MUST provide explicit text that the values can differ

#### Scenario: Focused-month composition legend retains chart and currency semantics
- **WHEN** Income or Expense composition is rendered for a selected reporting month
- **THEN** pie slices MUST represent percentage share of that selected month's movement
- **AND** the legend MUST render each slice's EUR amount rather than repeating the percentage
