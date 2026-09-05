# Implementation Plan: Dashboard Expense Kinds and Pinned Groups

## Scope and assumptions

This plan implements the approved OpenSpec change `dashboard-kind-pinned-groups` only. It keeps the current dashboard route and existing report routes/contracts compatible. `ReportingInsightDimension` is intentionally unchanged. The existing untracked `.superpowers/` mockup artifacts are outside scope and must not be added to commits.

The general account-group PATCH is additive and accepts only `isDashboardPinned` in this change. The existing rename endpoint remains operational and unchanged.

## Phase 1 — Domain, persistence, and account-group contracts

1. Update `src/FamilyFinances.Domain/Ledger/AccountGroups/AccountGroup.cs`.
   - Add `IsDashboardPinned`, defaulting to `false` for all newly created groups.
   - Add explicit `SetDashboardPinned(bool)` state transition.
   - Keep name/description validation unchanged.
   - Test in `tests/FamilyFinances.Domain.Tests/Ledger/AccountGroups/AccountGroupTests.cs`.

2. Update EF mapping and create a Ledger migration.
   - Modify `src/FamilyFinances.Infrastructure/Persistence/Configurations/AccountGroupConfiguration.cs` to map a required boolean with server/default value `false`.
   - Generate a timestamped migration in `src/FamilyFinances.Infrastructure/Persistence/Migrations/Ledger/`, its designer file, and update `LedgerDbContextModelSnapshot.cs`.
   - Existing rows receive `false`; do not backfill pins.
   - Validate using relational-provider migration coverage, not EF InMemory.

3. Add account-group partial update across Application/API/Web contracts.
   - Add `SetAccountGroupDashboardPinnedRequest` and a focused application handler using `IAccountGroupRepository` + `ILedgerUnitOfWork`.
   - Add `IsDashboardPinned` to `AccountGroupDto` and `AccountGroupDetailsDto`; update create/list/get mappings.
   - Add `PATCH /api/v1/account-groups/{id}` to `AccountGroupsController`, protected with `CanWrite`, returning `204` or `404`.
   - Keep `PATCH /{id}/rename` untouched.
   - Add `SetDashboardPinnedAsync` in `src/FamilyFinances.Web/Api/AccountGroupsApi.cs` with existing token/error conventions.
   - Amend `openspec/api-spec.yaml` with the general PATCH, request schema, and response properties.

4. Expose pinning in group management.
   - Update `src/FamilyFinances.Web/Components/Pages/AccountGroups/AccountGroupDetailPage.razor` with a simple switch/checkbox and busy/error handling consistent with existing group actions.
   - Reload group details after a successful update.
   - Add English and Spanish resource values for the label, helper text, and success/error messaging.

## Phase 2 — Dashboard reporting contract and projections

5. Extend dashboard DTOs and repository interface additively.
   - In `src/FamilyFinances.Application/Reporting/Dtos/DashboardOverviewDto.cs`, add records for expense-kind rank entries and pinned-group operational-result rows; append collections to the overview response.
   - Add explicit reporting repository methods or one supplemental dashboard method in `IReportingReadRepository`; keep the method bounded and dashboard-specific.
   - Do not add `Kind` to `ReportingInsightDimension` or change Pareto/anomaly contracts.

6. Implement current-month expense-kind aggregation in `ReportingReadRepository`.
   - Join transactions, splits, expense accounts, and `AccountKinds`; group by catalog ID/name.
   - Use the selected month through `asOf`, convert expense totals to positive display magnitudes, and return deterministic aggregates.
   - In `GetDashboardOverviewHandler`, apply named constant `ExpenseKindTopCount = 6`, deterministic tie ordering, and synthetic localized `Others` only when non-zero tail data exists.
   - Keep localization at the presentation boundary or use the existing localization-safe DTO strategy; avoid storing a locale-specific database value.

7. Implement all-pinned-groups operational-result aggregation.
   - Project all pinned groups and their memberships in a bounded query; aggregate selected-month and YTD ranges without one query per group.
   - Include only `AccountNature.Income` and `AccountNature.Expense` member accounts.
   - Apply reporting display sign convention (`-split.Amount.Cents`); exclude Asset, Liability, and Equity.
   - Preserve independent contribution to overlapping groups and sort final rows by group name.
   - Do not reuse `GetMonthlyEvolutionAsync(AccountGroups)` or balance-oriented `GroupStates` for this data.

8. Compose the extended overview.
   - Update `GetDashboardOverviewHandler` to fetch and shape both dashboard collections while retaining KPIs, daily points, YTD summary, and data-sufficiency logic.
   - Remove obsolete group-state/compact-insight population only after `DashboardPage` no longer consumes it; avoid unrelated report behavior changes.
   - Ensure `ReportsController`, `ReportsApi`, API serialization tests, and OpenAPI schema remain aligned.

## Phase 3 — Annual chart and dashboard UI

9. Add mixed-chart compatibility to existing annual bars.
   - Extend `AnnualBarChart.razor` with an optional line-series key parameter; callers without it retain current grouped-bar payloads.
   - In `wwwroot/js/reportCharts.js`, preserve existing bar dataset options and configure only the selected result dataset as a non-filled Chart.js line using the supplied semantic color.
   - Keep PNG export, tooltip, cutoff marker, accessibility attributes, and single euro-axis behavior.
   - Update unit/render tests under `tests/FamilyFinances.Web.Tests/Features/Reports/Charts`.

10. Recompose `DashboardPage.razor` without changing its visual system.
   - Retain five KPIs, daily month chart, asset-total evolution, loading/error/data-sufficiency handling, semantic palette, and responsive grid.
   - Build annual income, expense, and result series from existing `YtdSummary.MonthlyNetPoints`, render the mixed chart, then remove the redundant standalone monthly-net chart.
   - Replace group annual evolution with a compact pinned-group table and replace pie composition with horizontal Top 6 + Others kind bars.
   - Provide a localized empty state with an account-groups management link when no group is pinned.
   - Do not introduce map, future movements, forecasting, generic click filtering, or widget configuration.

11. Update dashboard styles and resources conservatively.
   - Use existing CSS tokens, Bootstrap utilities, `ChartSemanticPalette`, and panel classes; add scoped/shared styles only where required for horizontal rank bars and compact group rows.
   - Add localized titles/subtitles, column headers, aria labels, and empty-state copy in both resource files.
   - Preserve mobile order: KPI → daily/annual overview → kinds/groups → assets.

## Phase 4 — Deterministic tests and validation

12. Add/update unit tests.
   - Domain state transition tests.
   - Account-group handler tests for true/false updates and unknown group behavior.
   - Dashboard handler tests for Top 6 + Others, deterministic ties, empty input, and group flow-nature exclusion.
   - Reuse mocks only for repository boundaries; do not mock domain entities.

13. Add/update integration tests with real relational storage.
   - API account-group tests: authorization, general PATCH persistence, `404`, DTO visibility, and unchanged rename behavior.
   - Dashboard API tests: kind aggregation, fixed tail handling, flow-only pinned result, and overlapping membership behavior.
   - Migration test: apply the new migration and assert legacy groups read as unpinned.
   - Keep test data minimal, isolated, and independent of execution order.

14. Add/update Web tests.
   - `AccountGroupsApi` request/method tests for PATCH.
   - Account-group detail UI toggle behavior.
   - Annual chart payload test proving only `result` is a line.
   - Dashboard composition/order, rank rows, pinned table/empty state, existing KPI/data-sufficiency behavior, and responsive layout tests.

15. Finish documentation and quality gates.
   - Mark OpenSpec tasks only after each verified implementation item; document any material design change in proposal/design/tasks.
   - Run `openspec validate dashboard-kind-pinned-groups --strict`.
   - Run affected project tests first, then `dotnet test FamilyFinances.sln -c Release` if practical.
   - Run `dotnet build FamilyFinances.sln -c Release`.
   - Run whitespace/diff checks and review `openspec/api-spec.yaml` against implemented controller/DTO contracts.

## Explicit non-goals

- No report filtering by account kind, cross-chart filters, map, predictions, scheduled movements, or recurring movement infrastructure.
- No insight-enum expansion, report-page redesign, group layout configuration, or endpoint removal.

