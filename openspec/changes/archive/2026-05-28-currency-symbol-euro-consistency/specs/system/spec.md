## ADDED Requirements

### Requirement: System SHALL Present Monetary Values With EUR Identity Across Web UI Surfaces
The system MUST present user-facing monetary values with EUR currency identity in all Web UI surfaces that render ledger amounts, including list rows, table cells, and summary amount labels.

Scope constraints:
- This requirement applies to presentation formatting only.
- Domain/accounting value semantics MUST remain unchanged.
- Storage and API numeric payload contracts MUST remain unchanged unless a preformatted display string is the only source of rendered symbol.

#### Scenario: Transactions and account movements surfaces render EUR symbol
- **WHEN** an authenticated user views transactions and account movements list rows
- **THEN** rendered monetary values MUST use the "€" symbol
- **AND** no "$" symbol MUST be rendered for monetary values on those surfaces

#### Scenario: Reporting list/table surfaces render EUR symbol
- **WHEN** a user opens reporting views that show tabular/list-like monetary values
- **THEN** rendered monetary values MUST use EUR symbol semantics
- **AND** symbol rendering MUST remain consistent with transactions/account movements surfaces

#### Scenario: Sign semantics remain unchanged while symbol is standardized
- **WHEN** positive and negative amounts are rendered after this change
- **THEN** sign-preserving semantics MUST remain unchanged
- **AND** only currency identity presentation is standardized to EUR
