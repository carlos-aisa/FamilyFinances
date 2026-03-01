## ADDED Requirements

### Requirement: Economic State Snapshot SHALL Use A Premium Semantic Metric Layout
The economic-state snapshot view MUST render stock and flow metrics with a consistent premium card system while preserving current KPI semantics and values.

#### Scenario: Stock and flow families remain semantically separated
- **WHEN** `/reports/economic-state` snapshot data is displayed
- **THEN** stock metrics (`Assets`, `Liabilities`, `Net Worth`) and flow metrics (`Income`, `Expense`, `Period Net Result`) MUST remain visually distinguishable as separate semantic families
- **AND** card styling MUST use shared premium metric-card primitives

#### Scenario: Premium styling does not change KPI values
- **WHEN** snapshot KPI cards are rendered in premium mode
- **THEN** displayed values MUST match the existing economic-state payload values
- **AND** no additional transformation of report semantics MUST be introduced

### Requirement: Economic State Tabs SHALL Retain Existing Behavior With Premium Navigation Styling
The economic-state tabs MUST keep current routes and data-loading behavior while adopting premium tab and panel styling.

#### Scenario: Tab switching behavior remains deterministic
- **WHEN** a user switches between `Snapshot`, `Asset Evolution`, and `Income Evolution` tabs
- **THEN** the active tab behavior and rendered panel content MUST remain consistent with current behavior
- **AND** tab controls MUST use premium visual states for active, inactive, and disabled modes

### Requirement: Economic State Context Blocks SHALL Use Compact Premium Informational Presentation
The economic-state informational blocks MUST remain explicit and readable while using compact premium styling that reduces visual clutter.

#### Scenario: Metric explanation blocks stay explicit and readable
- **WHEN** the report renders explanatory stock/flow context text
- **THEN** semantic explanation content MUST remain visible and explicit
- **AND** informational blocks MUST use premium styling that preserves readability in dark mode

