## Why

Several UI listings currently render monetary amounts with the "$" symbol, which conflicts with the product's single-currency EUR model and causes user confusion. This change is needed now to align all user-facing amount rendering with euro semantics and avoid inconsistent financial interpretation across screens.

## What Changes

- Standardize money rendering in web listings and list-like report tables so the visible currency symbol is always "€".
- Define a single formatting rule for UI amounts: keep culture-specific number separators while forcing EUR currency identity.
- Replace any hardcoded or culture-default "$" output paths in shared formatting helpers and listing components.
- Add deterministic fallback behavior so null/empty/unavailable monetary values do not render any foreign currency symbol.
- Add automated regression tests covering list pages and representative reporting list/table surfaces to prevent symbol regressions.
- Update affected functional documentation to record EUR-only presentation semantics.

## Release Impact

Type: minor
Rationale: Backward-compatible behavior hardening that standardizes existing UI monetary presentation semantics across the application.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `system`: Tighten single-currency presentation requirements so all user-facing amount rendering uses EUR symbol semantics consistently.
- `web-localization`: Refine localization requirements so active culture controls numeric/date formatting, while currency symbol identity remains EUR for financial amounts.
- `transaction-list-filtering`: Ensure transaction list rows never display "$" and always follow standardized EUR rendering.
- `account-movements-filtering`: Ensure account movements list rows never display "$" and always follow standardized EUR rendering.

## Non-Goals

- Introduce multi-currency support, currency conversion, or per-user currency preferences.
- Redesign list layouts, sorting, filtering logic, or reporting calculations unrelated to symbol rendering.
- Change persisted monetary storage, domain value objects, or accounting sign semantics.
- Modify API monetary payload shape unless required only for formatting consistency at UI boundary.

## Rollback Plan

- Revert the formatting helper and component-level rendering changes in a single patch.
- Keep tests added by this change and mark expected behavior back to baseline only if rollback is approved.
- Validate rollback by running focused web and integration test suites for impacted list/report views.
- If rollback is partial, gate release until all affected listing surfaces render a single agreed symbol.

## Impact

- Frontend: Shared amount-formatting helpers, listing/table Razor components, and localized UI resources.
- Backend/API: No contract expansion expected; only touched if an API-provided preformatted display string currently injects "$".
- Tests: Web component tests and API integration regression tests for amount rendering in list/report contexts.
- Documentation: OpenSpec delta specs and implementation notes referencing EUR-only currency presentation behavior.
