## Context

The account movements page already retrieves `AccountMovementsDto` from `GET /api/v1/accounts/{id}/movements`.
Each `AccountMovementDto` already includes `RunningBalance` computed in the reporting repository and returned by API.

Current UI in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` displays date, description, payee, counterparty, and movement amount, but does not display the per-row running balance.

## Goals / Non-Goals

**Goals:**
- Show running balance for every movement row in the account movements table.
- Make balance evolution visible without navigating away from the movements page.
- Keep current API contract and backend calculation logic unchanged.

**Non-Goals:**
- Rework balance-calculation logic in `ReportingReadRepository`.
- Add new endpoints, DTO fields, or query parameters.
- Change pagination, filters, or sorting behavior.

## Decisions

1. Reuse existing backend data (`AccountMovementDto.RunningBalance`) instead of introducing new API behavior.
- Rationale: running balance is already available and avoids unnecessary backend risk.
- Alternative considered: recompute running balance in frontend.
- Rejected because it duplicates business logic and risks mismatch with backend semantics.

2. Add a dedicated `Running Balance` table column in `AccountMovementsPage.razor`.
- Rationale: explicit column keeps the evolution visible and scannable per row.
- Alternative considered: tooltip or expandable details.
- Rejected because it hides key information and adds interaction cost.

3. Apply sign-based text styling (`success` for positive, `danger` for negative, `muted` for zero).
- Rationale: fast visual interpretation of account trajectory.
- Alternative considered: neutral color for all values.
- Rejected because it reduces scanability.

## Risks / Trade-offs

- [Risk] Additional table width may reduce readability on small screens.
  -> Mitigation: keep compact column width and rely on existing responsive container.

- [Trade-off] Visual emphasis can imply positive/negative semantics differ by account nature.
  -> Mitigation: retain numeric sign and currency formatting exactly as provided by current data.

## Migration Plan

- Deploy as standard web UI update.
- No schema, migration, or API rollout dependencies.
- Rollback by reverting `AccountMovementsPage.razor` changes.

## Open Questions

- None for current scope.
