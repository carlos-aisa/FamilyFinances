## 1. Identify and centralize money symbol rendering paths

- [x] 1.1 Audit Web UI list/table amount renderers to locate all code paths that can emit "$" (transactions, account movements, reporting tables, and shared amount-format helpers).
- [x] 1.2 Consolidate monetary display into shared EUR-aware formatting helpers so list/table components do not format currency independently.
- [x] 1.3 Add deterministic formatting guardrails in shared helpers to preserve sign semantics while forcing EUR symbol identity.

## 2. Apply EUR symbol consistency in targeted listing surfaces

- [x] 2.1 Update Transactions list amount rendering on `/transactions` to consume the standardized EUR formatter.
- [x] 2.2 Update Account Movements list amount and running-balance rendering on `/accounts/{id}/movements` to consume the standardized EUR formatter.
- [x] 2.3 Update reporting list/table monetary cells in representative report surfaces to consume the same standardized EUR formatter.
- [x] 2.4 Ensure null/empty/unavailable money display states never fallback to foreign symbols.

## 3. Align localization behavior with fixed EUR currency identity

- [x] 3.1 Update localization/formatting integration so active culture still controls numeric/date conventions while monetary symbol identity remains EUR.
- [x] 3.2 Remove or adapt any culture-default currency formatting calls that can emit "$" for domain monetary values.
- [x] 3.3 Keep language-switch behavior unchanged (runtime refresh and persisted culture) after currency rendering adjustments.

## 4. Automated test updates for regression safety

- [x] 4.1 Add or update Web component tests for Transactions list verifying rendered monetary amounts use `€` and never `$`.
- [x] 4.2 Add or update Web component tests for Account Movements list verifying both amount and running-balance columns use `€` and never `$`.
- [x] 4.3 Add or update representative reporting UI tests for table/list monetary cells to assert EUR symbol consistency.
- [x] 4.4 Update localization-related tests to assert culture-specific separators can change while EUR symbol identity stays fixed.

## 5. Documentation and OpenSpec consistency

- [x] 5.1 Update any impacted implementation notes under `docs/` that reference currency formatting behavior.
- [x] 5.2 Confirm OpenSpec delta specs in this change match implemented behavior for `system`, `web-localization`, `transaction-list-filtering`, and `account-movements-filtering`.

## 6. Validation and readiness checks

- [x] 6.1 Run focused Web tests covering transactions, account movements, and reporting list/table rendering behaviors.
- [x] 6.2 Run broader solution regression tests (`dotnet test FamilyFinances.sln -c Release`) and resolve failures caused by updated currency rendering expectations.
- [x] 6.3 Validate OpenSpec artifacts strictly (`openspec validate currency-symbol-euro-consistency --strict`).
- [x] 6.4 Confirm change status reports all artifacts complete and tasks ready for implementation apply flow.
