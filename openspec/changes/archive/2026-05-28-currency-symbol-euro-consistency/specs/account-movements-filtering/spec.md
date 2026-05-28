## ADDED Requirements

### Requirement: Account Movements List SHALL Render Monetary Values Using EUR Symbol
The account movements page MUST render monetary values with EUR currency identity and MUST NOT display "$" for movement amount or running-balance values.

Implementation scope:
- Surface: `/accounts/{id}/movements`
- Coverage: movement signed amount column and running-balance column
- Existing filtering, pagination, and running-balance ownership semantics MUST remain unchanged

#### Scenario: Movement amount column renders EUR symbol
- **WHEN** movement rows are rendered
- **THEN** signed amount values MUST display with `€`
- **AND** no movement amount cell MUST display `$`

#### Scenario: Running balance column renders EUR symbol
- **WHEN** running-balance values are rendered per row
- **THEN** displayed running-balance values MUST use `€`
- **AND** running-balance values MUST remain sourced from backend payload

#### Scenario: Pagination and filtering do not regress symbol consistency
- **WHEN** user changes filters or navigates pages in account movements
- **THEN** all visible amount and running-balance cells MUST keep `€` symbol semantics
- **AND** page fallback and total-count behavior MUST remain unchanged
