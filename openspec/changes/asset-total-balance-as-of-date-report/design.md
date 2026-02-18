## Context

The current reporting module provides period-based reports (`monthly-summary`, `category-totals`, `account-totals`, `account-group-totals`) but does not provide a single as-of snapshot for total assets. Users currently need to infer this manually.

The solution must follow the existing layered architecture and reuse the established reporting flow:
- API controller endpoint in `ReportsController`
- Application query + handler
- Infrastructure repository method via `IReportingReadRepository`
- Blazor Web report page + `ReportsApi` client method

No data-model migration is required; the calculation can be derived from existing transactions/splits/accounts.

## Goals / Non-Goals

**Goals:**
- Provide a deterministic report for total balance of all `Asset` accounts at a user-selected as-of date.
- Expose this report via API and Web UI.
- Keep behavior consistent with existing money/sign conventions in the project.
- Ensure endpoint and UI are documented and covered by tests.

**Non-Goals:**
- Net-worth calculation (Assets minus Liabilities).
- Asset composition breakdown by account in this first iteration.
- Multi-currency support or FX conversion.
- Changes to ledger posting rules.

## Decisions

### Decision 1: Use a dedicated report endpoint with `asOf` query parameter
- **Choice:** Add `GET /api/v1/reports/asset-total-balance?asOf=YYYY-MM-DD`.
- **Rationale:** Keeps parity with current report endpoint patterns and avoids overloading existing period-based endpoints.
- **Alternative considered:** Reusing `/account-totals` with `from=min` and `to=asOf+1` and aggregating client-side.
  - **Rejected because:** less explicit contract, less efficient, and mixes concerns.

### Decision 2: Compute from ledger splits filtered by `AccountNature.Asset` and `BookedOn <= asOf`
- **Choice:** Aggregate cents over `TransactionSplits` joined to `Accounts` and `Transactions`.
- **Rationale:** This is the canonical source of truth and requires no schema changes.
- **Alternative considered:** Summing current balances then reversing deltas.
  - **Rejected because:** introduces additional complexity and baseline assumptions.

### Decision 3: Return a small dedicated DTO
- **Choice:** New DTO with `AsOf`, `TotalCents`, `AssetAccountsCount`.
- **Rationale:** Explicit and stable contract for API and Web, test-friendly, precise monetary representation.
- **Alternative considered:** return only decimal euros.
  - **Rejected because:** current reporting contracts mostly use integer cents for precision.

### Decision 4: Add a dedicated Web report page
- **Choice:** Add `/reports/asset-total-balance` page with date selector and result card.
- **Rationale:** Matches existing report UX and keeps discoverability in Reports index.
- **Alternative considered:** add value as widget inside existing Reports index.
  - **Rejected because:** loses filter controls and consistency with report pages.

## Risks / Trade-offs

- **[Risk] Date semantics ambiguity (inclusive vs exclusive)** -> **Mitigation:** explicitly define as-of behavior as `BookedOn <= asOf`.
- **[Risk] Large-data query cost on full-history datasets** -> **Mitigation:** keep query aggregate-only with server-side filtering and indexes already used by transaction date/account joins.
- **[Risk] Sign confusion in asset totals** -> **Mitigation:** document sign behavior in spec and expose a single authoritative `TotalCents` value used directly by UI.
- **[Risk] Endpoint contract drift with docs** -> **Mitigation:** update `openspec/api-spec.yaml` in same change tasks.

## Migration Plan

1. Add query/handler/DTO and repository contract + implementation.
2. Expose new API endpoint in `ReportsController`.
3. Extend `ReportsApi` and add Web report page + index navigation card.
4. Add tests (application/API integration/web API client and optionally UI smoke).
5. Update OpenAPI document.

Rollback:
- Remove endpoint/handler/query/repository additions.
- Remove Web page/card and API client method.
- Revert OpenAPI and related tests.

## Open Questions

- Should this report eventually include optional liability subtraction (`net worth`) in the same screen?
- Do we want an optional account-level breakdown under the total in a follow-up change?
