## Context

This change improves search usability when users type text without accents while stored data includes accented characters.

Current behavior is inconsistent across surfaces:

- Client-side filters are accent-sensitive:
  - `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor`
  - `src/FamilyFinances.Web/Components/Shared/AccountSelector.razor`
  - `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`
- Server-side filters are accent-sensitive and are used by:
  - `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` via `AccountsApi.GetMovementsAsync(..., q, ...)`
  - `src/FamilyFinances.Web/Components/Pages/History/HistoryMovementsPage.razor` via `HistoryApi.GetHistoricalMovementsAsync(..., q, ...)`
  - Expense transaction search endpoint `GET /api/v1/transactions/search-expenses`

The proposal states no backend changes are needed, but code inspection shows backend filtering is part of the user-visible search path for account/history movements and expense search.

Architecture constraints to preserve:

- Keep layering (Presentation -> Application -> Domain; Infrastructure may depend on Application/Domain).
- No schema/migration changes for this patch.
- Keep current API contracts backward-compatible.
- Keep deterministic tests on current stack (xUnit + FluentAssertions; integration tests on relational provider).

## Goals / Non-Goals

**Goals:**

- Make search accent-insensitive and case-insensitive everywhere covered by this change.
- Use one normalization algorithm across Web and Infrastructure to prevent behavioral drift.
- Keep current UX patterns, routes, and API shapes unchanged.
- Keep existing pagination semantics for account/history movements (`TotalCount`, page slicing, running balance behavior).
- Add tests that prove accent-insensitive matching end-to-end.

**Non-Goals:**

- No new endpoints.
- No database collation changes, virtual columns, or migrations.
- No redesign of filter UIs.
- No broad search-system refactor outside affected surfaces.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Use one shared normalization helper for all affected filters.
- [MUST] Normalize both user query and candidate text before `Contains`.
- [MUST] Keep behavior case-insensitive after normalization.
- [MUST] Preserve existing query params and endpoint signatures.
- [MUST] Preserve existing default date/page behavior in movements pages.
- [MUST] Preserve `TotalCount` correctness for filtered account/history movement responses.
- [MUST] Keep `runningBalance` semantics unchanged.
- [MUST NOT] introduce DB migrations or new persistence fields.
- [MUST NOT] duplicate normalization logic in multiple files with divergent implementations.
- [MUST NOT] reduce search behavior to a partial replacement map (for example, only `a/e/i/o/u`).

## DETAILED UI FLOWS

### Flow 1: Quick Entry account search

1. User opens Quick Entry and types a query in `Search accounts by name or type`.
2. Query is normalized (trim, Unicode decomposition, remove combining marks, case fold).
3. Each candidate account name/nature/kind label is normalized the same way.
4. Matches are shown when normalized candidate contains normalized query.
5. Existing accordion behavior and section auto-expansion remain unchanged.

### Flow 2: Shared AccountSelector search

1. User types in account selector search input.
2. Query normalization is applied once per keystroke.
3. Account name and nature label are normalized and compared.
4. Matching list updates without changing component API.

### Flow 3: Transactions page search

1. User types search text and clicks `Apply Filters`.
2. Filter pipeline keeps existing order (date -> text -> amount).
3. Text filter now uses normalized `Headline`, `Subheadline`, and `PayeeName`.
4. Amount/date logic stays unchanged.

### Flow 4: Account movements page search (server-side)

1. User types search text and clicks `Search`.
2. Web layer still sends `q` unchanged (trimmed).
3. Repository applies existing account/date/amount constraints first.
4. If `q` is provided, text matching is performed accent-insensitively before pagination.
5. Response `TotalCount` and paged `Items` reflect the normalized filter.

### Flow 5: History movements page search (server-side)

1. User selects year/account and types query.
2. Request goes to `GET /api/v1/history/movements?...&q=...`.
3. Shared account-movements repository path applies normalized filtering.
4. UI receives accent-insensitive results with unchanged paging contract.

### Flow 6: Expense transaction search endpoint (server-side)

1. User flow that relies on `search-expenses` sends query `q`.
2. Repository normalizes query and candidate fields (description/payee/expense account name).
3. Endpoint returns the same DTO shape, but matching is accent-insensitive.

## DETAILED PAGE WIREFRAMES

No layout redesign is required. Existing controls remain in place; only filter semantics change.

### Quick Entry (`/quick-entry`)

```text
+--------------------------------------------------------------------------------+
| Search accounts by name or type [.............................]               |
| Accordion sections by account nature with account chips                        |
+--------------------------------------------------------------------------------+
```

### Transactions (`/transactions`)

```text
+--------------------------------------------------------------------------------+
| From [date]  To [date]  Search [Description or payee...]                      |
| Amount From [number]  Amount To [number]                                      |
| [Apply Filters] [Reset]                                                        |
| Table of transactions                                                          |
+--------------------------------------------------------------------------------+
```

### Account Movements (`/accounts/{id}/movements`)

```text
+--------------------------------------------------------------------------------+
| From [date]  To [date]  Search [description or payee...]  [Search]            |
| Amount From [number]  Amount To [number]                                      |
| Movements table + pagination                                                   |
+--------------------------------------------------------------------------------+
```

### History Movements (`/history/movements`)

```text
+--------------------------------------------------------------------------------+
| Account [select]  Year [select]  Search [description or payee...] [Search]    |
| Movements table + paging                                                       |
+--------------------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Search normalization primitive | Existing project utility pattern (`NameNormalizer`) | N/A | `src/FamilyFinances.Application/Common/SearchTextNormalizer.cs` |
| Quick Entry filter | Existing `GetFilteredAccounts` flow | Use shared normalizer for query and candidate text | None |
| AccountSelector filter | Existing `FilteredAccounts` query logic | Use shared normalizer instead of raw `ToLowerInvariant()` | None |
| Transactions list text filter | Existing `FilterTransactions(...)` | Normalize text fields before `Contains` | None |
| Account/history movements API contract | Existing `q` query param | No contract change | None |
| Reporting repository account movement query | Existing date/account/amount query and paging shape | Add accent-insensitive text filtering before page slicing | None |
| Expense search repository | Existing `SearchExpensesAsync` path | Apply shared normalization in matching logic | None |
| Test suites | Existing Web/API integration coverage | Add accent-insensitive assertions | New focused tests where needed |

## Decisions

### Decision 1: Introduce one shared normalization helper in Application layer

- **Choice:** Add `SearchTextNormalizer.NormalizeForSearch(string?)` in `FamilyFinances.Application`.
- **Rationale:** Both Web and Infrastructure already reference Application, so one helper can be reused without layer violations.
- **Alternative considered:** duplicate helper in Web and Infrastructure.
  - **Rejected because:** high risk of drift and inconsistent matching over time.

### Decision 2: Keep API contracts unchanged (`q` remains the input)

- **Choice:** Keep endpoint/query parameter shapes as-is; only matching semantics change.
- **Rationale:** Backward compatibility and minimal rollout risk.
- **Alternative considered:** introduce explicit flags like `accentInsensitive=true`.
  - **Rejected because:** unnecessary API complexity for a default UX bug fix.

### Decision 3: Preserve current query constraints and only change text matching

- **Choice:** Keep date/account/amount filtering behavior and ordering unchanged; swap text compare semantics.
- **Rationale:** Isolates behavior change to search normalization.
- **Alternative considered:** broader query refactor of account movement retrieval.
  - **Rejected because:** too large for a patch-level change.

### Decision 4: Favor correctness over DB-collation tricks for diacritics

- **Choice:** Use Unicode normalization-based matching (`FormD` + combining-mark removal) rather than provider-specific collation hacks.
- **Rationale:** Predictable behavior independent of SQLite collation quirks.
- **Alternative considered:** SQL collation/replacement chains.
  - **Rejected because:** incomplete diacritic coverage, provider coupling, and difficult long-term maintenance.

### Decision 5: Add explicit regression tests for accent-insensitive cases

- **Choice:** Cover Web component filters and API integration scenarios with accented test data.
- **Rationale:** Prevent silent reintroduction of accent-sensitive comparisons.
- **Alternative considered:** rely on existing search tests only.
  - **Rejected because:** current tests validate case-insensitivity and query wiring, not diacritic behavior.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Shared normalizer

```csharp
using System.Globalization;
using System.Text;

namespace FamilyFinances.Application.Common;

public static class SearchTextNormalizer
{
    public static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
```

### Example 2: Client-side filter usage

```csharp
var query = SearchTextNormalizer.NormalizeForSearch(_searchQuery);
if (!string.IsNullOrEmpty(query))
{
    filtered = filtered.Where(t =>
        SearchTextNormalizer.NormalizeForSearch(t.Headline).Contains(query) ||
        SearchTextNormalizer.NormalizeForSearch(t.Subheadline).Contains(query) ||
        SearchTextNormalizer.NormalizeForSearch(t.PayeeName).Contains(query));
}
```

### Example 3: Account movement text match predicate (normalized)

```csharp
var normalizedQuery = SearchTextNormalizer.NormalizeForSearch(searchQuery);
if (!string.IsNullOrEmpty(normalizedQuery))
{
    candidates = candidates
        .Where(x =>
            SearchTextNormalizer.NormalizeForSearch(x.Transaction.Description).Contains(normalizedQuery) ||
            SearchTextNormalizer.NormalizeForSearch(x.PayeeName).Contains(normalizedQuery))
        .ToList();
}
```

### Example 4: Expense search normalized matching

```csharp
var normalizedQuery = SearchTextNormalizer.NormalizeForSearch(query);
var filtered = candidates
    .Where(t =>
        SearchTextNormalizer.NormalizeForSearch(t.Description).Contains(normalizedQuery) ||
        SearchTextNormalizer.NormalizeForSearch(t.PayeeName).Contains(normalizedQuery) ||
        SearchTextNormalizer.NormalizeForSearch(t.ExpenseAccountName).Contains(normalizedQuery))
    .Take(limit)
    .ToList();
```

## CRITICAL UX BEHAVIORS

- Typing `maria` must match records rendered as `maria` and `maria with accent`.
- Existing case-insensitive behavior must remain.
- Empty/whitespace query must keep current no-filter behavior.
- Search semantics must be consistent between Quick Entry, AccountSelector, Transactions, Account Movements, and History Movements.
- Pagination labels (`X-Y of Z`) must still reflect filtered totals on server-backed pages.
- No new user-visible settings are introduced for this feature.

## Risks / Trade-offs

- [Risk] Repository-side normalized matching can be more expensive than pure SQL `LIKE`.
  - Mitigation: keep current restrictive predicates (account/date/amount) before normalized filtering and maintain cancellation support.

- [Risk] Unicode edge cases (for example ligatures or locale-specific folding) may not behave as transliteration.
  - Mitigation: document behavior as "diacritic-insensitive, not full transliteration" and add targeted tests for expected Spanish/Latin cases.

- [Risk] If normalization is applied inconsistently across fields, users will see partial behavior.
  - Mitigation: enforce shared helper usage and include explicit test coverage per surface.

- [Trade-off] Keeping DB schema unchanged avoids migrations but limits opportunities for indexed normalized search.
  - Mitigation: track a future optimization change only if performance evidence appears.

## Migration Plan

1. Add shared normalizer:
   - `src/FamilyFinances.Application/Common/SearchTextNormalizer.cs`
2. Update client-side filters to use shared normalizer:
   - `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor`
   - `src/FamilyFinances.Web/Components/Shared/AccountSelector.razor`
   - `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor`
3. Update backend search matching:
   - `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs`
   - `src/FamilyFinances.Infrastructure/Persistence/Repositories/TransactionRepository.cs`
4. Keep API contracts unchanged but verify path behavior through existing API layers:
   - `src/FamilyFinances.Web/Api/AccountsApi.cs`
   - `src/FamilyFinances.Web/Api/HistoryApi.cs`
   - `src/FamilyFinances.Api/Controllers/V1/AccountsController.cs`
   - `src/FamilyFinances.Api/Controllers/V1/HistoryController.cs`
5. Add/extend tests:
   - `tests/FamilyFinances.Application.Tests` for normalizer behavior.
   - `tests/FamilyFinances.Web.Tests/Features/QuickEntry/QuickEntryPageTests.cs`
   - `tests/FamilyFinances.Web.Tests/Features/Transactions/TransactionsListPageTests.cs`
   - `tests/FamilyFinances.Api.IntegrationTests/Ledger/Accounts/AccountMovementsApiTests.cs`
   - `tests/FamilyFinances.Api.IntegrationTests/Ledger/Transactions/RefundsApiTests.cs`
6. Execute impacted test suites and verify no regression in existing search/filter flows.

### Rollback Strategy

1. Revert shared normalizer usage in Web and Infrastructure to prior comparison logic.
2. Remove/disable new accent-insensitive tests.
3. Re-run baseline tests to confirm previous behavior.

## Open Questions

- Should we include `PayeeSelectorSimple` in the same change for search consistency, even though this proposal is account-search focused?
- For very large datasets, should we introduce a follow-up optimization with indexed normalized fields?
- Do we want a short tooltip/help text clarifying that search ignores accents?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Shared normalizer

- [ ] `SearchTextNormalizer` exists in Application layer.
- [ ] Helper trims input.
- [ ] Helper removes combining marks via Unicode category checks.
- [ ] Helper performs case-folding.
- [ ] Null/empty/whitespace input returns empty string.
- [ ] Unit tests include accented and non-accented pairs.

### Quick Entry

- [ ] Searching `maria` matches account name with accent.
- [ ] Nature-label matching still works.
- [ ] Kind-label matching still works.
- [ ] Empty search keeps all accounts.
- [ ] Accordion open-state behavior remains unchanged.

### AccountSelector

- [ ] Name search is accent-insensitive.
- [ ] Nature text search remains functional.
- [ ] Existing `FilterByNature` behavior is preserved.
- [ ] Existing `AllowedNatures` behavior is preserved.

### Transactions page

- [ ] `Headline` matching is accent-insensitive.
- [ ] `Subheadline` matching is accent-insensitive.
- [ ] `PayeeName` matching is accent-insensitive.
- [ ] Date filters still apply correctly.
- [ ] Amount range filters still apply correctly.
- [ ] Invalid amount-range validation behavior remains unchanged.

### Account Movements API path

- [ ] Query `q` is still accepted unchanged by API.
- [ ] Search matching is accent-insensitive in description.
- [ ] Search matching is accent-insensitive in payee name.
- [ ] `TotalCount` reflects normalized filter.
- [ ] Pagination still returns deterministic page slices.
- [ ] Running balance values remain correct.

### History Movements API path

- [ ] `GET /history/movements` uses same normalized search semantics.
- [ ] Year/account filters still constrain results correctly.
- [ ] Page and pageSize handling remains unchanged.

### Expense search endpoint

- [ ] Description matching is accent-insensitive.
- [ ] Payee matching is accent-insensitive.
- [ ] Expense account-name matching is accent-insensitive.
- [ ] Existing `limit` cap behavior remains unchanged.
- [ ] Existing minimum query-length behavior remains unchanged.

### Regression safety

- [ ] No endpoint contracts were changed.
- [ ] No migration files were generated.
- [ ] Existing non-search account movement tests still pass.
- [ ] Existing amount-range tests still pass.
- [ ] Existing Web API query-building tests still pass.

