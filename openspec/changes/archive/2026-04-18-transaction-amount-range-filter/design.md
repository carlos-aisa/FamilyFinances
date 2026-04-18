## Context

The change adds amount-range filtering to two existing transaction exploration surfaces:

- `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor` (client-side filtering over a locally cached list)
- `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` (server-side filtering through `GET /api/v1/accounts/{id}/movements`)

Current state:

- `TransactionsListPage` supports date (`_fromDate`, `_toDate`) and text (`_searchQuery`) filters only.
- `AccountMovementsPage` supports date and text filters, plus server pagination.
- Backend account-movements query supports date/search/pagination, but no amount range parameters.

Requested behavior:

- Two numeric inputs: `Amount From` and `Amount To`.
- Inclusive boundaries.
- Absolute-value semantics:
  - `10-50` includes values with signed amount `-30` and `+30`.
- `TransactionsListPage`: filter in Web layer.
- `AccountMovementsPage`: filter in API/repository layer.

Architecture constraints:

- Keep strict layering intact (Presentation -> Application -> Domain; Infrastructure can depend on Application/Domain).
- Do not add new architecture patterns.
- No schema/migration changes.
- Keep tests deterministic and update coverage for changed behavior.

## Goals / Non-Goals

**Goals:**

- Add optional amount range filter inputs to both pages with consistent labels and behavior.
- Apply absolute-value filtering and inclusive min/max bounds on both pages.
- Extend account movements API contract with optional `minAmount` / `maxAmount` query parameters.
- Preserve existing date/search/pagination behavior and running-balance semantics.
- Add localization keys for new labels and range-validation feedback.
- Add automated tests across Web UI/API/repository-integration paths.

**Non-Goals:**

- No change to global date semantics in this change (inclusive/exclusive date work is tracked elsewhere).
- No backend persistence changes, migrations, or schema updates.
- No page-size selector or pagination redesign.
- No currency conversion or multi-currency support.
- No changes to reporting/history endpoints outside account movements.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Treat `minAmount` and `maxAmount` as absolute EUR values.
- [MUST] Use inclusive comparisons (`>= min`, `<= max`).
- [MUST] Keep amount filtering optional; empty inputs must preserve current behavior.
- [MUST] Validate `Amount From <= Amount To` when both are provided.
- [MUST] Keep account-movements running-balance calculation unchanged (filtering must not redefine ledger math).
- [MUST] Keep API additions backward-compatible (new query params optional).
- [MUST] Use `InvariantCulture` when serializing numeric query params in Web API client.
- [MUST] Keep filtering query-translatable for EF Core SQLite.
- [MUST NOT] Introduce client-side full-fetch for account movements.
- [MUST NOT] add new endpoints for this feature.
- [MUST NOT] bypass repository abstraction from controller.
- [MUST NOT] change existing sign-color conventions in account movements table.

## DETAILED UI FLOWS

### Flow 1: Transactions page amount filtering (`/transactions`)

1. User opens `/transactions`; data is loaded once into `_allItems` via `TransactionsApi.ListAsync(take: 1000, ct)`.
2. User enters `Amount From` and/or `Amount To`.
3. User clicks `Apply Filters`.
4. Component validates range:
   - If both values exist and `from > to`, show localized validation error and do not apply predicate.
5. `FilterTransactions(...)` applies:
   - date filters (existing),
   - text filter (existing),
   - amount range filters (new, absolute value semantic).
6. Results are displayed and `Load More` keeps operating on filtered list when filters are active.

### Flow 2: Account movements amount filtering (`/accounts/{id}/movements`)

1. User opens account movements page; initial load uses existing date range defaults and page 1.
2. User enters `Amount From` and/or `Amount To`.
3. User clicks `Search`.
4. Component validates range:
   - If invalid (`from > to`), show localized error and skip API call.
5. `LoadMovementsAsync()` calls `AccountsApi.GetMovementsAsync(...)` with new optional args.
6. `AccountsApi` appends optional query params:
   - `minAmount=<decimal>`
   - `maxAmount=<decimal>`
7. API controller model-binds optional values and passes them to repository.
8. Repository applies absolute-value filtering in SQL-compatible query path, then existing ordering/pagination logic.
9. UI renders filtered page with unchanged pagination controls and running-balance values.

### Flow 3: Partial ranges

- If only `Amount From` is provided, show rows where `abs(amount) >= from`.
- If only `Amount To` is provided, show rows where `abs(amount) <= to`.
- If neither is provided, no amount predicate is applied.

### Flow 4: Clearing amount filters

- Transactions page:
  - `Reset Filters` clears both amount inputs along with existing filters.
- Account movements page:
  - User clears inputs and clicks `Search`; request is sent without amount params.

## DETAILED PAGE WIREFRAMES

### `/transactions` filter card

```text
+--------------------------------------------------------------------------------+
| Quick Date Filter [preset chips] [All Time]                                    |
| From Date [date]   To Date [date]   Search [text]                              |
| Amount From [number]   Amount To [number]                                      |
| [Apply Filters] [Reset]                                                        |
+--------------------------------------------------------------------------------+
```

Placement decision:

- Keep existing 3-column row for date/search.
- Add second row with two amount fields (`col-12 col-md-4` each) and leave spacing consistent with current Bootstrap grid.

### `/accounts/{id}/movements` filter card

```text
+--------------------------------------------------------------------------------+
| Quick Select [preset chips]                                                    |
| From Date [date]  To Date [date]  Search [text]  [Search button]              |
| Amount From [number]  Amount To [number]                                       |
+--------------------------------------------------------------------------------+
```

Placement decision:

- Keep existing row and button alignment.
- Add a second row for amount range fields to avoid shrinking current text search control.

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Transactions filter UI | Existing card layout, apply/reset actions | Add amount fields/state/validation/filter predicate | None |
| Account movements filter UI | Existing filter card and search action | Add amount fields/state/validation; include params in API call | None |
| Web accounts API abstraction | Existing `IAccountsApi.GetMovementsAsync` method | Extend signature with optional amount args | None |
| Web accounts API HTTP builder | Existing query param construction and auth handling | Append `minAmount`/`maxAmount` with invariant formatting | None |
| Accounts API endpoint | Existing `GetMovements` route and pagination validation | Bind optional amount query params and pass through | None |
| Reporting repository abstraction | Existing `GetAccountMovementsAsync` contract | Extend signature with optional amount range args | None |
| Reporting repository query | Existing LINQ pipeline for date/search/pagination | Add absolute amount predicate while keeping order/paging/balance flow | None |
| Localization resources | Existing shared resource files | Add amount labels + invalid-range message keys | None |
| Tests (Web/API integration) | Existing test suites for filters, query params, pagination | Add amount-range cases and boundary checks | None |

## Decisions

### Decision 1: Keep a single semantic contract for amount filtering (absolute + inclusive)

- **Choice:** Apply absolute value and inclusive bounds in both client and server paths.
- **Rationale:** User mental model stays consistent regardless of page type.
- **Alternative considered:** signed-value filtering in account movements.
  - **Rejected because:** proposal explicitly requires absolute-value matching.

### Decision 2: Extend existing account movements endpoint instead of adding a new endpoint

- **Choice:** Add optional query parameters (`minAmount`, `maxAmount`) to existing endpoint.
- **Rationale:** Maintains backward compatibility and avoids endpoint sprawl.
- **Alternative considered:** dedicated filtered endpoint.
  - **Rejected because:** unnecessary contract duplication and higher maintenance.

### Decision 3: Validate invalid ranges in Presentation layer before filtering/request

- **Choice:** When both inputs exist and `from > to`, show localized error and skip filtering call.
- **Rationale:** Prevents ambiguous behavior and avoids silent normalization surprises.
- **Alternative considered:** auto-swap values.
  - **Rejected because:** hidden value mutation can confuse users and tests.

### Decision 4: Filter account movements using absolute cents in query path

- **Choice:** Apply predicate on absolute cents (`Math.Abs(...)`) in queryable pipeline.
- **Rationale:** Keeps decimal rounding deterministic and compatible with money storage model (`AmountCents`).
- **Alternative considered:** convert to euros first and compare decimals in query.
  - **Rejected because:** less explicit precision control and potentially weaker SQL translation behavior.

### Decision 5: Preserve running-balance ownership in repository

- **Choice:** Do not alter running-balance computation; amount filtering only controls visible rows.
- **Rationale:** Running balance is accounting-critical and already centralized.
- **Alternative considered:** recompute running balance from filtered rows.
  - **Rejected because:** would distort ledger semantics.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Transactions page state and filtering

```csharp
private decimal? _amountFrom;
private decimal? _amountTo;

private bool HasInvalidAmountRange()
    => _amountFrom.HasValue && _amountTo.HasValue && _amountFrom.Value > _amountTo.Value;

private IReadOnlyList<TransactionListItemDto> FilterTransactions(IReadOnlyList<TransactionListItemDto> items)
{
    var filtered = items.AsEnumerable();

    if (_fromDate.HasValue)
        filtered = filtered.Where(t => t.BookedOn >= _fromDate.Value);
    if (_toDate.HasValue)
        filtered = filtered.Where(t => t.BookedOn < _toDate.Value);
    if (!string.IsNullOrWhiteSpace(_searchQuery))
    {
        var query = _searchQuery.ToLowerInvariant();
        filtered = filtered.Where(t =>
            t.Headline.ToLowerInvariant().Contains(query) ||
            (t.Subheadline?.ToLowerInvariant().Contains(query) ?? false) ||
            (t.PayeeName?.ToLowerInvariant().Contains(query) ?? false));
    }

    // Transactions list amount is already absolute in DTO projection.
    if (_amountFrom.HasValue)
        filtered = filtered.Where(t => t.Amount >= _amountFrom.Value);
    if (_amountTo.HasValue)
        filtered = filtered.Where(t => t.Amount <= _amountTo.Value);

    return filtered.ToList();
}
```

### Example 2: Web API client query string extension

```csharp
public async Task<AccountMovementsDto> GetMovementsAsync(
    Guid accountId,
    DateOnly? fromInclusive = null,
    DateOnly? toExclusive = null,
    string? searchQuery = null,
    decimal? minAmount = null,
    decimal? maxAmount = null,
    int page = 1,
    int pageSize = 50,
    CancellationToken ct = default)
{
    var url = $"api/v1/accounts/{accountId}/movements";
    var queryParams = new List<string>();

    if (fromInclusive.HasValue)
        queryParams.Add($"from={fromInclusive.Value:yyyy-MM-dd}");
    if (toExclusive.HasValue)
        queryParams.Add($"to={toExclusive.Value:yyyy-MM-dd}");
    if (!string.IsNullOrWhiteSpace(searchQuery))
        queryParams.Add($"q={Uri.EscapeDataString(searchQuery)}");
    if (minAmount.HasValue)
        queryParams.Add($"minAmount={minAmount.Value.ToString(CultureInfo.InvariantCulture)}");
    if (maxAmount.HasValue)
        queryParams.Add($"maxAmount={maxAmount.Value.ToString(CultureInfo.InvariantCulture)}");

    // existing page/pageSize behavior unchanged...
}
```

### Example 3: Accounts controller passthrough

```csharp
[HttpGet("{id:guid}/movements")]
public async Task<ActionResult<AccountMovementsDto>> GetMovements(
    [FromRoute] Guid id,
    [FromServices] IReportingReadRepository reportingRepo,
    [FromQuery] string? from = null,
    [FromQuery] string? to = null,
    [FromQuery] string? q = null,
    [FromQuery] decimal? minAmount = null,
    [FromQuery] decimal? maxAmount = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken ct = default)
{
    // existing date/paging safety...
    var skip = (page - 1) * pageSize;

    var result = await reportingRepo.GetAccountMovementsAsync(
        id, fromDate, toDate, q, minAmount, maxAmount, skip, pageSize, ct);
    return Ok(result);
}
```

### Example 4: Repository amount predicate (absolute cents)

```csharp
var q =
    from t in _db.Transactions.AsNoTracking()
    join s in _db.TransactionSplits.AsNoTracking()
        on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
    where s.AccountId == accountIdVo
    where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
    select new
    {
        TransactionId = t.Id,
        BookedOn = t.BookedOn,
        CreatedAt = t.CreatedAt,
        Description = t.Description,
        PayeeName = payee != null ? payee.Name : null,
        SignedAmountCents = s.Amount.Cents
    };

if (minAmount.HasValue)
{
    var minCents = Money.FromEuros(minAmount.Value).Cents;
    q = q.Where(x => Math.Abs(x.SignedAmountCents) >= minCents);
}

if (maxAmount.HasValue)
{
    var maxCents = Money.FromEuros(maxAmount.Value).Cents;
    q = q.Where(x => Math.Abs(x.SignedAmountCents) <= maxCents);
}
```

### Example 5: Shared range-validation helper pattern (page-level)

```csharp
private bool TryValidateAmountRange(decimal? from, decimal? to, out string? error)
{
    if (from.HasValue && to.HasValue && from.Value > to.Value)
    {
        error = L["Filter_AmountRangeInvalid"]; // "Amount From must be less than or equal to Amount To."
        return false;
    }

    error = null;
    return true;
}
```

## CRITICAL UX BEHAVIORS

- Amount filters are optional and additive to existing date/search filters.
- Boundary values are included (exactly equal to min or max must match).
- Account movements must match by absolute signed amount, preserving existing sign rendering.
- Validation errors are explicit and localized (no silent value swaps).
- Reset/clear behavior removes amount filters with existing controls.
- Existing pagination messaging (`X-Y of Z`) remains accurate after amount filtering.
- Existing loading/error UX patterns are preserved.

## Risks / Trade-offs

- [Risk] EF query translation for absolute amount filter can regress on provider differences.
  - Mitigation: filter on cents in query and cover with API integration tests on relational provider.

- [Risk] Users may expect signed filtering on account movements.
  - Mitigation: label and docs specify amount range uses absolute values.

- [Risk] Invalid range handling can add friction during input editing.
  - Mitigation: validate on submit action only (apply/search), not every keystroke.

- [Risk] Additional query params could break client URL expectations if formatting is locale-dependent.
  - Mitigation: force invariant decimal serialization in `AccountsApi`.

- [Trade-off] Client-side transactions filtering still depends on initial capped fetch (`take: 1000`).
  - Mitigation: unchanged from existing page behavior; out of scope for this change.

## Migration Plan

1. Extend Web page filter state/UI:
   - `TransactionsListPage`: add amount range inputs and predicates.
   - `AccountMovementsPage`: add amount range inputs and page-level validation.
2. Extend Web API client contract:
   - update `IAccountsApi` and `AccountsApi.GetMovementsAsync` signatures.
   - append optional query params in request URL.
3. Extend API endpoint:
   - add optional `[FromQuery] decimal? minAmount`, `maxAmount`.
   - pass through to repository call.
4. Extend repository abstraction + implementation:
   - update `IReportingReadRepository.GetAccountMovementsAsync` signature.
   - add absolute amount predicates before count/pagination.
5. Add localization keys in:
   - `src/FamilyFinances.Web/Resources/SharedResource.resx`
   - `src/FamilyFinances.Web/Resources/SharedResource.en-US.resx`
   - `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx`
6. Add/extend tests:
   - Web page tests for both pages.
   - Web API client query-parameter test.
   - API integration tests for account movements amount filtering.
7. Run focused test suites and then full impacted solution tests.

### Rollback Strategy

1. Revert added amount fields from both Web pages.
2. Revert API client/query parameter extension.
3. Revert controller/repository signature and predicate additions.
4. Keep localization keys harmless if already merged, or revert them with feature code.
5. Re-run baseline tests to confirm previous behavior restored.

## Open Questions

- Should account movements empty-state copy explicitly mention amount criteria (current text says "search criteria")?
- Should minimum value be hard-clamped to `0` at UI input level (`min="0"`) and server-side validation reject negative params?
- Should we add a compact helper text near amount fields clarifying "uses absolute amount"?

## IMPLEMENTATION VERIFICATION CHECKLIST

### UI - Transactions page

- [ ] Amount From and Amount To inputs are present in `/transactions`.
- [ ] Inputs accept decimal values with `step="0.01"`.
- [ ] Apply Filters validates invalid range and shows localized error.
- [ ] Reset clears amount filters.
- [ ] Filtering with only Amount From works.
- [ ] Filtering with only Amount To works.
- [ ] Filtering with both bounds includes exact boundary values.
- [ ] Amount filter combines correctly with text/date filters.
- [ ] Load More works correctly when amount filters are active.

### UI - Account movements page

- [ ] Amount From and Amount To inputs are present in `/accounts/{id}/movements`.
- [ ] Search action validates invalid range and skips request on invalid input.
- [ ] Valid amount filters are sent through API call.
- [ ] Existing page reset-to-1 behavior on filter apply remains intact.
- [ ] Pagination controls still work with amount filters.
- [ ] Empty-state rendering remains stable under amount-filtered results.

### Web API client

- [ ] `IAccountsApi.GetMovementsAsync` signature includes optional amount args.
- [ ] URL contains `minAmount` when min is provided.
- [ ] URL contains `maxAmount` when max is provided.
- [ ] Numeric params are invariant-culture formatted.
- [ ] Existing auth/error handling behavior remains unchanged.

### API + repository

- [ ] Accounts controller binds optional amount query params.
- [ ] Repository abstraction signature is updated consistently.
- [ ] Repository applies absolute-value filtering before count/pagination.
- [ ] Inclusive boundary behavior is verified (`>=`, `<=`).
- [ ] Running balance values remain consistent with existing logic.
- [ ] No migration files are generated.

### Tests

- [ ] Add transactions-page test for amount range filtering.
- [ ] Add account-movements-page test ensuring min/max passed to API and page reset logic remains correct.
- [ ] Extend `AccountsApiAdditionalTests.GetMovementsAsync_BuildsExpectedQueryParameters`.
- [ ] Add API integration test: min-only filter.
- [ ] Add API integration test: max-only filter.
- [ ] Add API integration test: bounded filter with inclusive edges.
- [ ] Add API integration test: absolute matching includes both positive/negative signed rows.
- [ ] Existing account movements pagination/running-balance tests remain green.

### Localization and docs

- [ ] Add new amount labels and invalid-range key in all shared resource files.
- [ ] Confirm English/Spanish values are complete.
- [ ] Update any relevant user-facing docs if filter behavior is documented.

