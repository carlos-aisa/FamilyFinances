## ADDED Requirements

### Requirement: Annual Evolution Charts SHALL Render Current-Year Cutoff Semantics
Annual charts MUST expose explicit current-year cutoff treatment while preserving past-year full-series readability.

#### Scenario: Current year annual charts mark current-month cutoff
- **WHEN** an annual chart is rendered for the current year
- **THEN** the chart MUST display an explicit marker at the current month boundary
- **AND** months after the marker MUST be visually de-emphasized as future periods

#### Scenario: Past year annual charts render without future-period de-emphasis
- **WHEN** an annual chart is rendered for a fully completed past year
- **THEN** all months January through December MUST be rendered with normal emphasis
- **AND** no current-month cutoff marker MUST be shown

### Requirement: Account Group State Evolution Layout SHALL Keep Monthly Evolution Visible
The account-group state evolution surface MUST avoid hiding the monthly evolution chart below oversized neighboring content.

#### Scenario: Monthly evolution chart remains visible in desktop composition
- **WHEN** a user opens account-group state evolution on desktop viewport
- **THEN** layout MUST keep monthly evolution chart visible without requiring accidental overflow discovery
- **AND** list and right-side chart composition MUST not push monthly evolution out of the primary analytical viewport by default
