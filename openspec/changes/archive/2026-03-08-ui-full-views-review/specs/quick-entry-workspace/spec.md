## ADDED Requirements

### Requirement: Quick Entry Account Selection SHALL Provide Global Search And Single-Open Grouping
Quick Entry account discovery MUST support one global account search and single-open grouped account navigation.

#### Scenario: Global account search is available above grouped accounts
- **WHEN** a user opens `/quick-entry`
- **THEN** the accounts side panel MUST show one global search input above all account groups
- **AND** search matching MUST include account name and account nature/type label

#### Scenario: Account groups behave as a single-open accordion
- **WHEN** grouped accounts are displayed in Quick Entry
- **THEN** account groups MUST be rendered as accordion sections
- **AND** opening one group MUST close previously open groups in that panel

### Requirement: Quick Entry Modes SHALL Expose Configurable Guidance Text
Each quick-entry mode MUST display configurable contextual guidance in the mode header area.

#### Scenario: Guidance is shown per mode near the top of the active card
- **WHEN** the user switches between Expense, Income, Transfer, and Refund modes
- **THEN** each mode MUST display its configured guidance text in the mode description/header area
- **AND** guidance MUST NOT be hardcoded only in one mode footer

### Requirement: Quick Entry Date Selection SHALL Persist Across Mode Switches
Quick Entry MUST keep the selected transaction date when the user changes the active mode.

#### Scenario: Date persists across active mode transitions
- **WHEN** a user sets a date in one quick-entry mode and then changes to another mode
- **THEN** the newly active mode MUST reuse the same selected date value
- **AND** the value MUST persist until the user explicitly changes it
