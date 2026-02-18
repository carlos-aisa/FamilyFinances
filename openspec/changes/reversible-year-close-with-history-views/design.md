## Context

The current ledger behavior has no concept of fiscal-year closure. Any transaction date can be created, edited, or deleted regardless of period governance. Running-balance computation in account movement queries currently recalculates over broad history ranges, which becomes increasingly expensive as years accumulate.

Current state constraints observed in code:

- Transaction mutation is available through multiple paths (transaction pages, dashboard quick-entry, widgets, opening-balance workflow, reconciliation-generated adjustments).
- `GetAccountMovementsAsync` in `ReportingReadRepository` computes balances from historical splits and performs per-row counterparty lookups.
- There is no persistence model for fiscal closure state or year-level account snapshots.
- There is no dedicated historical area for read-only browsing separated from operational transaction workflows.

Stakeholders:

- End users who require annual close governance and safer historical periods.
- Maintainers who need scalable movement/running-balance behavior as data grows.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- Do not move transactions into archive tables/files.
- Do not delete or rewrite historical transaction rows during close/reopen.
- Enforce closed-year write guards in backend handlers/services, not only UI.
- Apply guard consistently to create, edit (both two-split and multi-split), delete, and reconcile flows.
- Keep closure granularity at calendar/fiscal year integer (YYYY).
- Reopen must be supported and must remove write blocking for reopened year.
- Historical views are read-only; no mutation actions from historical pages.
- Running-balance optimization must use yearly snapshots as baseline, not complete full-history scans.
- Maintain layered architecture boundaries (Presentation -> Application -> Domain, Infrastructure implementation only).
- Keep API versioning route style unchanged.

## Goals / Non-Goals

**Goals:**

- Introduce reversible fiscal year close/reopen governance.
- Persist closure state and account year-end snapshots.
- Block mutation operations for closed years.
- Provide dedicated historical views for transactions and account movements.
- Improve running-balance performance via snapshot baseline strategy.
- Cover behavior with application and integration tests.

**Non-Goals:**

- Physical archival of transaction records.
- Multi-tenant/year calendars beyond current single-ledger assumptions.
- Partial period close semantics (month/quarter).
- Localization/theming work.

## Decisions

### Decision 1: Introduce dedicated persistence entities for closure state and snapshots

- Decision: Add tables/entities:
  - `FiscalYearClosures` (year state and audit metadata)
  - `AccountYearSnapshots` (per-account closing balance for year end)
- Rationale: Explicit state model is required for reversible close/reopen and deterministic query baseline.
- Alternative considered: derive closed years from synthetic transactions.
- Rejected because: conflates governance metadata with business ledger events and complicates reversibility.

Proposed schema shape:

- `FiscalYearClosures`
  - `Year` (int, PK)
  - `IsClosed` (bool, required)
  - `ClosedAtUtc` (DateTime?, nullable)
  - `ClosedByUserId` (string?, nullable)
  - `ReopenedAtUtc` (DateTime?, nullable)
  - `ReopenedByUserId` (string?, nullable)

- `AccountYearSnapshots`
  - `Year` (int, part of PK)
  - `AccountId` (Guid, part of PK, FK -> Accounts)
  - `ClosingBalanceCents` (long, required)
  - `ComputedAtUtc` (DateTime, required)

Indexes:

- `IX_AccountYearSnapshots_AccountId_Year`
- Optional: `IX_FiscalYearClosures_IsClosed_Year`

### Decision 2: Add a single Year-Governance guard service in Application layer

- Decision: Introduce abstraction used by transaction/reconcile handlers to validate if a given booked date or existing transaction date is in closed year.
- Rationale: Prevent guard logic duplication and guarantee consistency across mutation paths.
- Alternative considered: duplicate checks in each handler/repository.
- Rejected because: high drift risk and inconsistent blocking behavior.

Service contract (target):

- `Task EnsureYearOpenAsync(int year, CancellationToken ct)`
- `Task<bool> IsYearClosedAsync(int year, CancellationToken ct)`
- `Task CloseYearAsync(int year, string? actorUserId, CancellationToken ct)`
- `Task ReopenYearAsync(int year, string? actorUserId, CancellationToken ct)`

### Decision 3: Close operation computes and stores deterministic year-end snapshots

- Decision: Closing year `Y` computes account balances as of `Y-12-31` and stores one snapshot row per account.
- Rationale: Snapshot baseline removes need to recompute from origin for future movement calculations.
- Alternative considered: lazy snapshots at first query.
- Rejected because: query-path complexity and non-deterministic first-hit latency.

### Decision 4: Reopen operation invalidates and recomputes snapshots when re-closed

- Decision: Reopening sets `IsClosed = false` and marks reopen metadata; previous snapshots for that year are removed or flagged stale and replaced upon next close.
- Rationale: Reopened years can accept edits; old snapshots become invalid.
- Alternative considered: retain stale snapshot and apply deltas.
- Rejected because: risk of silent drift and accounting inconsistency.

### Decision 5: Running-balance query uses snapshot baseline + in-range accumulation

- Decision: For `GetAccountMovementsAsync`, compute baseline:
  - If snapshot for previous closed year exists: start from snapshot cents.
  - Else fallback to current full-history strategy.
  Then add only required movement deltas up to the requested window/order.
- Rationale: Reduces expensive historical scans for mature ledgers.
- Alternative considered: full materialized running-balance table.
- Rejected because: heavier write-side maintenance for every mutation.

### Decision 6: Provide separate historical routes and API endpoints (read-only)

- Decision: Add dedicated History navigation entry and routes for:
  - Historical transactions list by year.
  - Historical account movements by year/account.
  - Year close/reopen management panel.
- Rationale: Clean separation between operational day-to-day transactions and immutable historical browsing context.
- Alternative considered: merge history into existing `/transactions` page filters only.
- Rejected because: weaker mental model and easier accidental mutation attempts.

### Decision 7: Preserve mutation API routes but enforce closed-year rejection with clear domain error

- Decision: Keep existing mutation endpoints; return domain/validation error when target year is closed.
- Rationale: Backward-compatible API surface with explicit policy enforcement.
- Alternative considered: introduce parallel mutation endpoints for open years.
- Rejected because: unnecessary API complexity.

## DETAILED UI FLOWS AND COMPONENT REUSE

### Flow 1: Close year from History management

1. User navigates to `History` from nav menu.
2. Year governance panel lists years with status badge (`Open`/`Closed`).
3. User clicks `Close Year` for year `Y`.
4. Confirmation modal explains write lock + snapshot creation.
5. User confirms.
6. API closes year and computes snapshots.
7. UI refreshes status to `Closed` and displays close timestamp.

### Flow 2: Reopen year

1. User opens `History` panel.
2. For a closed year `Y`, user clicks `Reopen Year`.
3. Confirmation modal explains that writes will be allowed again and snapshots will be regenerated at next close.
4. User confirms.
5. API reopens year.
6. UI updates badge to `Open`.

### Flow 3: Blocked create/edit/delete in closed year

1. User attempts create/edit/delete/reconcile with booked date in closed year.
2. API rejects request with domain error.
3. UI shows blocking alert with actionable message: `Year YYYY is closed. Reopen the year to modify movements.`
4. No data mutation occurs.

### Flow 4: Browse historical transactions (read-only)

1. User opens `History > Transactions`.
2. User selects year `Y`.
3. List loads only transactions booked in `Y`.
4. Row detail navigation is read-only (edit/delete controls hidden/disabled).

### Flow 5: Browse historical account movements (read-only)

1. User opens `History > Movements`.
2. User selects year + account.
3. Movement list renders with running balance computed from snapshot baseline + in-range deltas.
4. Page exposes no mutation actions.

### Flow 6: Operational transactions remain for active/open workflows

1. User uses existing `/transactions` and dashboard entry flows for open years.
2. Existing UX remains intact except for closed-year rejection messages when applicable.

## DETAILED PAGE WIREFRAMES

### History index with governance panel

```text
+--------------------------------------------------------------------------------+
| History                                                                        |
| [Transactions] [Movements] [Year Governance]                                   |
+--------------------------------------------------------------------------------+
| Year Governance                                                                 |
| ------------------------------------------------------------------------------ |
| Year | Status  | Closed At           | Actions                                 |
| 2026 | Open    | -                   | [Close Year]                            |
| 2025 | Closed  | 2026-01-02 09:14Z   | [Reopen Year]                           |
+--------------------------------------------------------------------------------+
```

### Historical transactions view

```text
+--------------------------------------------------------------------------------+
| History / Transactions                                                         |
| Year: [2025 v]  Search: [.................]                                   |
+--------------------------------------------------------------------------------+
| Date       | Type     | Description                 | Amount | Status(read-only)|
| 2025-12-14 | Expense  | Grocery weekly             | 95.30  | Locked            |
+--------------------------------------------------------------------------------+
```

### Historical movements view

```text
+--------------------------------------------------------------------------------+
| History / Movements                                                            |
| Year: [2025 v] Account: [Main Bank v]                                          |
+--------------------------------------------------------------------------------+
| Date       | Description            | Amount     | Running Balance             |
| 2025-01-03 | Salary                 | +2,000.00  | 2,000.00                    |
| 2025-01-04 | Rent                   | -900.00    | 1,100.00                    |
+--------------------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Existing File/Component | Action | Notes |
|---|---|---|---|
| Nav link structure | `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` | Modify | Add History entry |
| Transactions operational UI | `src/FamilyFinances.Web/Components/Pages/Transactions/*` | Modify | Add closed-year error handling/messages only |
| Account movements query | `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` | Modify | Add snapshot-based baseline path |
| Reconciliation flow | `src/FamilyFinances.Application/Ledger/Accounts/Handlers/ReconcileAccountHandler.cs` | Modify | Enforce closed-year policy before creating adjustment |
| Transaction create handler | `src/FamilyFinances.Application/Ledger/Transactions/Handlers/CreateTransactionHandler.cs` | Modify | Guard by booked year |
| Transaction update handlers | `UpdateTransactionHandler`, `UpdateMultiSplitTransactionHandler` | Modify | Guard by target year and existing tx date policy |
| Transaction delete handler | `DeleteTransactionHandler` | Modify | Guard by existing tx booked year |
| History pages | `src/FamilyFinances.Web/Components/Pages/History/*` | New | Separate read-only historical UI |
| Governance API endpoints | new or extended controller in API layer | New | Close/reopen/list year statuses |
| Persistence model | `LedgerDbContext` + new configurations + migrations | New/Modify | Add closure/snapshot entities and indexes |

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Closure and snapshot entities

```csharp
public sealed class FiscalYearClosure
{
    public int Year { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedByUserId { get; set; }
    public DateTime? ReopenedAtUtc { get; set; }
    public string? ReopenedByUserId { get; set; }
}

public sealed class AccountYearSnapshot
{
    public int Year { get; set; }
    public Guid AccountId { get; set; }
    public long ClosingBalanceCents { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}
```

### Example 2: Guard usage in create handler

```csharp
await _yearGovernance.EnsureYearOpenAsync(cmd.BookedOn.Year, ct);

var tx = Transaction.Create(cmd.BookedOn, cmd.Description, splits, payeeId);
await _transactions.AddAsync(tx, ct);
await _uow.SaveChangesAsync(ct);
```

### Example 3: Guard usage in delete handler

```csharp
var tx = await _transactions.GetByIdAsync(id, ct);
if (tx is null) return false;

await _yearGovernance.EnsureYearOpenAsync(tx.BookedOn.Year, ct);
await _transactions.RemoveAsync(id, ct);
await _uow.SaveChangesAsync(ct);
```

### Example 4: Snapshot baseline in movements query

```csharp
var baselineCents = await _snapshotRepo.GetBaselineCentsAsync(accountId, fromInclusive, ct);
var runningCents = baselineCents;

foreach (var m in orderedMovements)
{
    runningCents += m.AmountCents;
    // map running balance
}
```

### Example 5: Year governance API contract

```csharp
[HttpPost("api/v1/fiscal-years/{year:int}/close")]
public Task<IActionResult> CloseYear(int year, CancellationToken ct);

[HttpPost("api/v1/fiscal-years/{year:int}/reopen")]
public Task<IActionResult> ReopenYear(int year, CancellationToken ct);

[HttpGet("api/v1/fiscal-years")]
public Task<ActionResult<IReadOnlyList<FiscalYearStatusDto>>> ListYears(CancellationToken ct);
```

### Example 6: History route shell

```razor
@page "/history"

<TabNav>
  <Tab Link="/history/transactions">Transactions</Tab>
  <Tab Link="/history/movements">Movements</Tab>
  <Tab Link="/history/years">Year Governance</Tab>
</TabNav>
```

## CRITICAL UX BEHAVIORS

- Close and reopen actions always require explicit confirmation.
- Closed year status is visually explicit in governance list and historical filters.
- Historical views never render actionable create/edit/delete controls.
- Error message for blocked write must name the closed year.
- Existing operational pages remain familiar; only blocked cases introduce new alert.
- Read-only history pages must still allow drilldown detail viewing.

## Risks / Trade-offs

- [Risk] Reopen + edits can desynchronize old snapshots.
  -> Mitigation: delete/recompute snapshots for reopened year on next close and avoid reusing stale rows.

- [Risk] Multiple write entry points might miss guard integration.
  -> Mitigation: centralize guard in application services and enforce in all mutation handlers.

- [Risk] Added data model and query complexity may introduce regressions.
  -> Mitigation: integration tests for close/reopen, blocked writes, and movement calculations.

- [Risk] Large historical pages might still be slow without proper indexes.
  -> Mitigation: add indexes on transaction date and snapshot lookup keys.

- [Trade-off] Separate history views increase navigation surface area.
  -> Mitigation: keep history IA simple with three clear tabs and existing table patterns.

## Migration Plan

1. Add new domain/application abstractions for fiscal year governance and snapshot retrieval.
2. Add infrastructure entities/configurations/migration for closure and snapshot tables.
3. Add/extend repositories and services for close/reopen operations and snapshot compute.
4. Integrate guard checks into create/update/delete/reconcile handlers.
5. Add snapshot-based optimization path into account movement query logic.
6. Add API endpoints for year governance and historical retrieval.
7. Add web pages/routes/nav links for historical transactions/movements and governance UI.
8. Add/adjust tests (application + integration + web where applicable).
9. Run build/tests and perform manual end-to-end checks.

Rollback strategy:

- Disable new governance/history routes and endpoints.
- Revert guard checks and snapshot query path.
- Roll back migration for closure/snapshot tables.
- Keep core transaction tables untouched.

## Open Questions

- No blocking open questions for this scope. Reopen behavior and historical coverage are explicitly decided.

## IMPLEMENTATION VERIFICATION CHECKLIST

### A) Data model and persistence

- ? `FiscalYearClosures` table created with year state and audit metadata.
- ? `AccountYearSnapshots` table created with composite key `(Year, AccountId)`.
- ? Snapshot table has index optimized for account/year lookups.
- ? Optional closure state index added if query path requires it.
- ? `LedgerDbContext` includes both new DbSets.
- ? EF migration compiles and applies on clean database.
- ? EF migration applies on existing database.
- ? No schema changes to `Transactions` and `TransactionSplits` required for archival.

### B) Governance service and business rules

- ? Year close operation marks year as closed.
- ? Year reopen operation marks year as open.
- ? Close operation computes snapshots for all relevant accounts.
- ? Reopen invalidates stale snapshots policy is implemented.
- ? Re-closing recomputes snapshots deterministically.
- ? Guard service exposes `IsYearClosed` and `EnsureYearOpen` semantics.
- ? Guard errors are domain/business errors with clear message.

### C) Mutation guard coverage

- ? Create transaction path checks year-open before save.
- ? Update two-split path checks year-open before save.
- ? Update multi-split path checks year-open before save.
- ? Delete transaction path checks existing transaction year-open before delete.
- ? Reconcile path checks target date year-open before adjustment creation.
- ? Opening-balance flow fails when date falls in closed year.
- ? Dashboard quick-entry flow fails with clear closed-year error.
- ? Multi-split widget flow fails with clear closed-year error.

### D) History API and read-only behavior

- ? Fiscal-year list endpoint returns year status metadata.
- ? Close endpoint enforces idempotent/consistent behavior.
- ? Reopen endpoint enforces idempotent/consistent behavior.
- ? Historical transactions endpoint filters by selected year.
- ? Historical movements endpoint supports year/account filtering.
- ? Historical endpoints remain read-only.
- ? Existing transaction list endpoints remain compatible.

### E) UI and navigation

- ? Nav menu contains separate History entry.
- ? History section provides Transactions and Movements views.
- ? History section provides Year Governance view.
- ? Governance table shows explicit Open/Closed status badge.
- ? Close and reopen actions require confirmation modal.
- ? Historical transaction pages hide edit/delete controls.
- ? Historical movement page exposes no mutation actions.
- ? Operational transaction detail page surfaces closed-year errors clearly.

### F) Running balance performance path

- ? Movement query uses snapshot baseline when available.
- ? Snapshot-missing path falls back safely.
- ? Counterparty lookup path avoids avoidable N+1 patterns where practical.
- ? Query ordering remains deterministic.
- ? Results remain numerically consistent with prior logic for same dataset.
- ? Baseline and delta arithmetic is done in cents-safe logic where applicable.

### G) Testing

- ? Application tests added/updated for close/reopen policies.
- ? Application tests added/updated for mutation guard rejection.
- ? API integration tests cover close year and blocked writes.
- ? API integration tests cover reopen then allowed writes.
- ? Integration tests verify historical transactions retrieval.
- ? Integration tests verify historical movements retrieval and running balances.
- ? Web tests (if applicable) cover history read-only UI state.
- ? Tests remain deterministic and isolated.

### H) Validation and documentation

- ? `dotnet build` succeeds for API and Web projects after changes.
- ? Relevant test projects pass in CI-safe mode.
- ? OpenSpec artifacts stay aligned (proposal/design/spec/tasks).
- ? No architecture boundary violations introduced.
- ? No unapproved external dependency introduced.
- ? Rollback plan steps are executable.
