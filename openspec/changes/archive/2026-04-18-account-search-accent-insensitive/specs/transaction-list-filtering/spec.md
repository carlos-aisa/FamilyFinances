# transaction-list-filtering Delta Specification

## ADDED Requirements

### Requirement: Transactions Text Search SHALL Ignore Diacritics and Case
The transactions list text filter MUST match query text regardless of accent marks and casing.

Implementation scope:

- File: `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`
- Method: `FilterTransactions(IReadOnlyList<TransactionListItemDto> items)`
- Candidate fields:
  - `Headline`
  - `Subheadline`
  - `PayeeName`
- Query source: `_searchQuery`
- Date and amount range filters MUST continue to compose with text filtering.

#### Scenario: Plain query matches accented payee
- **WHEN** `_searchQuery` is `maria`
- **THEN** rows with payee text `María` MUST be included in filtered results
- **AND** rows not matching normalized text MUST remain excluded

#### Scenario: Accented query matches non-accented description
- **WHEN** `_searchQuery` is `José`
- **THEN** rows with headline/subheadline text `Jose` MUST be included
- **AND** matching MUST remain case-insensitive

#### Scenario: Text normalization composes with amount and date filters
- **WHEN** user applies date, amount range, and text query together
- **THEN** resulting rows MUST satisfy all active predicates
- **AND** amount-range validation semantics from `transaction-amount-range-filter` MUST remain unchanged

### Requirement: Transactions Search Normalization SHALL Be Symmetric
Normalization behavior MUST be applied symmetrically to both the search query and the candidate fields.

#### Scenario: Candidate-only normalization is not allowed
- **WHEN** query includes accented text and candidate text does not (or vice versa)
- **THEN** matching MUST still work in both directions
- **AND** implementation MUST normalize query and candidate using the same algorithm

