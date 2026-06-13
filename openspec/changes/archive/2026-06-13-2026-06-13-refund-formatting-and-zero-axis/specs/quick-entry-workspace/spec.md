## MODIFIED Requirements

### Requirement: Quick Entry Workspace SHALL Preserve Existing Capture Semantics
Quick-entry flows moved to `/quick-entry` MUST preserve existing operational behavior.

#### Scenario: Refund original-expense picker uses deterministic display formatting
- **WHEN** a user opens Refund quick entry and searches original expenses
- **THEN** each listed expense date MUST be rendered as `dd/MM/yyyy`
- **AND** each listed amount MUST be rendered with EUR suffix format (`XXX,XX €`)
- **AND** selecting or clearing linked expenses MUST preserve existing workflow semantics
