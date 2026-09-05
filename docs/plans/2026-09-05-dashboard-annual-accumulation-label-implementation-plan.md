# Dashboard Annual Accumulation Labels Implementation Plan

## Scope

Replace the two compact Spanish dashboard labels that use `YTD` with `Acum. anual`.

## Steps

1. Update `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx`.
   - Change only `Dashboard_Kpi_YtdNet` and `Dashboard_PinnedGroups_Ytd` to `Acum. anual`.
   - Preserve all calculations, resource keys, English strings, API contracts, and explanatory YTD labels.

2. Update `tests/FamilyFinances.Web.Tests/Features/Dashboard/DashboardPageTests.cs`.
   - Rename the KPI test to describe annual accumulation and assert the Spanish label.
   - Add or amend the dashboard rendering assertion for the pinned-group table header when its data fixture is present.

3. Amend `openspec/changes/dashboard-kind-pinned-groups/`.
   - Record the terminology refinement and its test coverage in the existing proposal, design, and follow-up task list.

## Non-Goals

- No calculation, DTO, API, persistence, or chart change.
- No English terminology change.
- No changes to long-form accumulated-net explanatory labels.

## Validation

Run the focused dashboard component tests, the Web test project in Release configuration, `dotnet build FamilyFinances.sln -c Release --no-restore`, and `openspec validate dashboard-kind-pinned-groups --strict`.
