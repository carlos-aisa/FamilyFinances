## CRITICAL IMPLEMENTATION CONSTRAINTS

- ? Do not physically move transactions to archive tables in this change.
- ? Do not remove historical data from `Transactions` or `TransactionSplits`.
- ? Do not allow create/update/delete/reconcile operations for dates in closed fiscal years.
- ? Do not change existing API route versions.
- ? Implement reversible year close/reopen behavior with explicit status tracking.
- ? Implement a dedicated historical UI view separate from the operational transactions view.
- ? Optimize running balance computation with yearly account snapshots, not full-history recomputation.
- ? Keep historical browsing read-only.

## Why

As data grows across multiple years, running balance computation and transaction browsing become more expensive and less manageable. The product needs explicit annual close governance and a separate historical browsing experience while preserving full traceability and allowing reopen when needed.

## What Changes

- Add fiscal year close/reopen capability with persisted year state.
- Enforce write-blocking for closed years (create, edit, delete, reconcile).
- Add yearly account balance snapshots to serve as computational baseline for running-balance queries.
- Add historical read-only views for both transactions and account movements in a dedicated historical section.
- Keep current operational transaction view focused on open-period workflows.
- Add validation and integration tests for closed-year guards, reopen behavior, and historical endpoints/UI flows.

### Non-goals

- No physical archival/migration of transactions into separate storage.
- No removal of existing transaction data.
- No change to authentication/authorization model.
- No support for partial-month close; closure granularity is fiscal year.

### Rollback Plan

- Disable/rollback new close/reopen and historical UI routes.
- Roll back new closure/snapshot data model migrations if deployment must be reverted.
- Restore previous reporting/running-balance path without yearly snapshot baseline.
- Preserve transaction data integrity by keeping core ledger tables unchanged.

## Capabilities

### New Capabilities
- `fiscal-year-governance`: Reversible fiscal year close/reopen state management, write guards for closed years, and yearly account snapshots.
- `historical-ledger-views`: Dedicated read-only historical views for transactions and account movements.

### Modified Capabilities
- `system`: Transaction mutation and account reconciliation requirements are extended to enforce closed-year write restrictions and historical read behavior.

## Impact

- Backend application handlers for transaction create/update/delete and reconciliation.
- Reporting repository/account movement queries and performance path.
- Infrastructure persistence model (new closure/snapshot entities + migrations + indexes).
- API controllers/endpoints for year governance and historical retrieval.
- Web navigation and pages for historical transactions/movements and year close management.
- Tests across Application, API integration, and Web.
