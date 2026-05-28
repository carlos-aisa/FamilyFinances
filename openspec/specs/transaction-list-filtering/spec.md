# transaction-list-filtering Specification

## Purpose
TBD - created by archiving change transaction-amount-range-filter. Update Purpose after archive.
## Requirements
### Requirement: Transactions List Filter Panel SHALL Expose Amount Range Inputs
The Transactions list page (`/transactions`) MUST provide two optional amount-range inputs in the filter card:

- `Amount From` (minimum absolute amount)
- `Amount To` (maximum absolute amount)

Implementation contract:

- File MUST be updated: `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`
- Backing state MUST include nullable decimal fields:
  - `private decimal? _amountFrom;`
  - `private decimal? _amountTo;`
- Inputs MUST support cents precision (`step="0.01"`).
- Inputs MUST be placed in the same filter card as date and text filters, without removing existing controls.
- Labels MUST be localized using shared resource keys.

#### Scenario: Transactions filter card renders amount range inputs
- **WHEN** an authenticated user opens `/transactions`
- **THEN** the filter card MUST render `Amount From` and `Amount To` controls
- **AND** both controls MUST allow empty values (optional filtering)
- **AND** both controls MUST support decimal entry with two-digit precision

#### Scenario: Localized labels are used for amount range inputs
- **WHEN** the page renders in any supported UI culture
- **THEN** `Amount From` and `Amount To` labels MUST come from `SharedResource` keys
- **AND** no hardcoded label literals for these controls MUST remain in the Razor markup

### Requirement: Transactions List Filtering SHALL Apply Inclusive Absolute Amount Bounds
Transactions list filtering MUST apply amount bounds to the absolute transaction amount already exposed in `TransactionListItemDto.Amount`.

Filtering contract:

- Method contract to preserve and extend:
  - `private IReadOnlyList<TransactionListItemDto> FilterTransactions(IReadOnlyList<TransactionListItemDto> items)`
- Amount predicate MUST be applied in this method using inclusive comparisons:
  - `t.Amount >= _amountFrom` when `Amount From` exists
  - `t.Amount <= _amountTo` when `Amount To` exists
- Existing date and text predicates MUST remain functional and composable with amount predicates.
- Amount filtering MUST NOT call backend APIs; it remains client-side for `/transactions`.

#### Scenario: Both bounds include rows within range
- **WHEN** `Amount From = 10.00` and `Amount To = 50.00`
- **THEN** only rows with absolute `Amount` between `10.00` and `50.00` (inclusive) MUST be shown
- **AND** rows below `10.00` or above `50.00` MUST be excluded

#### Scenario: Lower bound only filters minimum absolute amount
- **WHEN** only `Amount From` is provided
- **THEN** rows with absolute `Amount` lower than that value MUST be excluded
- **AND** rows equal to or higher than that value MUST remain visible

#### Scenario: Upper bound only filters maximum absolute amount
- **WHEN** only `Amount To` is provided
- **THEN** rows with absolute `Amount` higher than that value MUST be excluded
- **AND** rows equal to or lower than that value MUST remain visible

#### Scenario: Boundary values are included
- **WHEN** a row amount is exactly equal to `Amount From` or `Amount To`
- **THEN** that row MUST remain visible in results

### Requirement: Transactions List SHALL Reject Invalid Amount Range Deterministically
If both amount bounds are present and `Amount From` is greater than `Amount To`, the page MUST treat the filter range as invalid.

Validation contract:

- Validation MUST execute when user triggers filter apply action (`Apply Filters`).
- Invalid range MUST display a localized validation/error message.
- Invalid range MUST NOT trigger re-filtering that mutates the current visible results.
- Validation message key MUST be defined in shared resources (e.g., `Filter_AmountRangeInvalid`).

#### Scenario: Invalid range shows error and does not change result set
- **WHEN** user sets `Amount From = 100.00` and `Amount To = 50.00` and clicks `Apply Filters`
- **THEN** the page MUST display a localized invalid-range message
- **AND** the currently displayed transaction list MUST remain unchanged

#### Scenario: Valid range clears amount-range validation error
- **WHEN** user corrects an invalid range to `Amount From <= Amount To` and applies filters
- **THEN** amount-range validation error state MUST be cleared
- **AND** filtering MUST proceed with the corrected values

### Requirement: Transactions List Reset and Incremental Loading SHALL Respect Amount Filters
Reset and load-more behavior MUST stay consistent after amount filtering is added.

State-flow contract:

- `Reset Filters` MUST clear `_amountFrom` and `_amountTo` in addition to existing filter state.
- `LoadMoreAsync()` MUST continue to expand the currently filtered result set when any filter is active, including amount filters.
- Existing default load behavior (`take: 1000`, initial page slice) MUST remain unchanged.

#### Scenario: Reset clears amount range values
- **WHEN** user clicks `Reset Filters`
- **THEN** `Amount From` and `Amount To` controls MUST reset to empty values
- **AND** result list MUST return to unfiltered default slice behavior

#### Scenario: Load More appends rows from amount-filtered results
- **WHEN** amount filters are active and user clicks `Load More`
- **THEN** additional rows MUST be appended from the already amount-filtered dataset
- **AND** rows excluded by amount bounds MUST remain excluded after appending

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

