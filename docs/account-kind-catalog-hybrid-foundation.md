# Account Kind Catalog Hybrid Foundation

## Summary

This change introduces a global account-kind catalog that combines system-defined kinds and custom user-defined kinds.

Key outcomes:
- Account persistence now uses catalog identity (`KindId`) as the source of truth.
- Existing enum semantics are retained as a compatibility projection (`legacyKind`) for read surfaces that still expect enum-based grouping.
- Account create/list contracts now include catalog metadata (`kindId`, `kindKey`, `kindName`).
- Custom kinds are now explicitly bound to one account `Nature`.
- New account-kind API endpoints are available for list/create/activate-deactivate operations.
- Custom kinds can be deleted only when they are not referenced by any existing account.
- Existing accounts can update their kind through an explicit account lifecycle operation.

## Data Model and Migration Notes

- New table: `AccountKinds`
  - Columns: `Id`, `Key`, `Name`, `IsSystem`, `IsActive`, `SortOrder`, `LegacyKind`, `Nature`
  - Unique index on `Key`
- `Accounts.Kind` is replaced by `Accounts.KindId` (FK to `AccountKinds`).
- Migration backfills existing accounts with deterministic enum-to-catalog mapping.
- System kinds are seeded in migration and startup seeding flow.
- A follow-up migration backfills `AccountKinds.Nature` deterministically for system kinds and derives custom kind nature from existing account usage when available.

## API Contract Notes

- `CreateAccountRequest` adds optional `kindId`.
  - When provided, account creation validates that the selected kind exists and is active.
  - Account creation also validates that kind and account `Nature` are compatible.
  - Legacy `kind` is kept for compatibility fallback during transition.
- `AccountDto` now includes:
  - `kindId`
  - `kindKey`
  - `kindName`
- New endpoints:
  - `GET /api/v1/accounts/kinds`
  - `POST /api/v1/accounts/kinds` (requires `nature`)
  - `PATCH /api/v1/accounts/kinds/{kindId}/active`
  - `DELETE /api/v1/accounts/kinds/{kindId}` (custom kinds only; rejected when in use)
  - `PATCH /api/v1/accounts/{id}/kind`

## Web UX Notes

- Account creation pages now consume a unified kind source (system + custom) from the account-kind catalog API.
- A minimal management entry point was added in Accounts list to create and toggle custom kinds, including custom kind nature binding.
- Accounts list allows deleting custom kinds when they are not in use by any account.
- Accounts list now supports reclassifying existing accounts by changing their selected kind.
- Account-kind selectors are ordered by the visible localized label to improve discoverability.
- Kind label resolution has been centralized through a shared resolver in Web UI.
- Quick Entry account search now uses catalog-driven kind labels from account DTOs.

## Explicitly Deferred Scope

The following items are intentionally deferred to a future change:
- Bank import workflows
- Bank movement mapping rules
- Automatic classification logic by kind
- Reconciliation rules based on kind taxonomy

This foundation only prepares stable kind identity semantics (`Id`/`Key`) for future filter/logic work.
