## ADDED Requirements

### Requirement: Transactions List SHALL Render Monetary Values Using EUR Symbol
The transactions list page MUST render all monetary values using EUR symbol semantics and MUST NOT display "$" for transaction amounts.

Implementation scope:
- Surface: `/transactions`
- Coverage: row amount cells, totals or summary labels in the same page when backed by transaction monetary values
- Existing search/date/amount range filtering behavior MUST remain unchanged

#### Scenario: Transaction row amount uses EUR symbol
- **WHEN** a transaction row is rendered in the list
- **THEN** its amount display MUST include `€` currency identity
- **AND** it MUST NOT include `$`

#### Scenario: Filtering interactions preserve EUR symbol rendering
- **WHEN** user applies or resets filters (date, text, amount range)
- **THEN** resulting visible rows MUST continue to render monetary values with `€`
- **AND** filtering semantics MUST remain functionally unchanged

#### Scenario: Load-more appended rows keep EUR symbol rendering
- **WHEN** user loads additional rows in the transactions list
- **THEN** appended rows MUST render monetary values with `€`
- **AND** no newly appended row MUST render `$`
