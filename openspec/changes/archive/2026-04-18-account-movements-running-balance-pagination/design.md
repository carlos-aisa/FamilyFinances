## Context

The account movements page (`/accounts/{id}/movements`) already calls a paginated API (`page`, `pageSize`) and receives `TotalCount`, but the UI currently hardcodes `page=1` and `pageSize=50` with no navigation controls. As a result, users cannot reach older rows when a date range returns more than 50 movements.

Running balance is provided by backend read models (`AccountMovementDto.RunningBalance`) and should remain the single source of truth. This change focuses on exposing complete navigation for the filtered dataset and making balance verification reliable from the same screen.

## Goals / Non-Goals

**Goals:**
- Allow users to navigate account movements beyond the first 50 rows without changing route/API contracts.
- Keep running-balance semantics ledger-correct for every rendered row, regardless of current page.
- Provide explicit visual context of visible range and total count.
- Add regression tests that validate running-balance correctness with multi-page datasets.

**Non-Goals:**
- Adding a user-selectable page size.
- Rewriting movement retrieval to client-side full fetch.
- Changing reconciliation workflows or historical movements behavior.
- Adding new database structures, migrations, or new API endpoints.

## Decisions

1. Reuse existing server pagination contract (`page`, `pageSize`, `TotalCount`) for account movements.
- Rationale: API already supports deterministic paging and avoids large payload regressions.
- Alternative considered: fetch all filtered rows and paginate in browser.
- Rejected because it increases payload size and hides server-side ordering semantics.

2. Add explicit page state in `AccountMovementsPage` and load by page.
- Expected state model:
  - `_currentPage` (1-based)
  - `_pageSize = 50`
  - derived `CanGoPrevious`, `CanGoNext`, `VisibleFromOrdinal`, `VisibleToOrdinal`
- Filter interactions (`preset`, manual dates, search submit) reset `_currentPage = 1` before loading.
- Alternative considered: infinite "Load more" append behavior.
- Rejected because running-balance verification is easier with deterministic page boundaries and totals.

3. Preserve backend running-balance ownership and add stronger tests instead of moving logic to Web.
- Rationale: balance is accounting-critical and already centralized in repository read logic.
- Required test expansion:
  - multi-page account movement scenario (`> 50` rows) asserting `RunningBalance` values on page 1 and page 2.
  - filtered/paginated scenario confirming balance correctness does not depend on visible page size.
- Alternative considered: recompute running balance in UI based on visible rows.
- Rejected because it can diverge from ledger semantics and fails when partial pages are loaded.

4. Keep API contracts unchanged for this iteration.
- Rationale: no additional metadata is required because page and page-size are known client-side and `TotalCount` is already returned.
- Alternative considered: add extra response fields (`CurrentPage`, `TotalPages`, `HasNext`).
- Rejected to avoid contract churn for behavior that can be derived client-side.

5. Make out-of-range page handling deterministic after data changes.
- If a page load returns zero rows with `TotalCount > 0` and `_currentPage > 1`, step back one page and reload.
- Rationale: avoids dead-end page states after deletions/reconciliations.

## Risks / Trade-offs

- [Risk] More page-navigation clicks and API calls for large datasets.
  -> Mitigation: keep controls lightweight and surface visible range/total to reduce unnecessary navigation.

- [Risk] Users may still assume first page is "entire range" if count messaging is weak.
  -> Mitigation: show explicit `X-Y of Z` text in header/footer near pagination controls.

- [Trade-off] Fixed page size 50 may still require many clicks in very large datasets.
  -> Mitigation: defer page-size selector to a future UX iteration to keep scope focused.

## Migration Plan

1. Implement UI page state and pagination controls in `AccountMovementsPage`.
2. Wire API calls to pass `_currentPage` and `_pageSize`.
3. Add localized strings for pagination labels/buttons.
4. Add/extend API integration tests for running-balance correctness with paginated datasets.
5. Add Web UI tests for pagination control visibility and page transitions.
6. Run focused test suites, then full affected-project test run.

Rollback strategy:
- Revert account-movements UI paging state and controls while preserving existing API behavior.
- Keep backend running-balance path unchanged.

## Open Questions

- Should a user-selectable page size be introduced later (for example 50/100/200) once baseline pagination behavior is stable?
