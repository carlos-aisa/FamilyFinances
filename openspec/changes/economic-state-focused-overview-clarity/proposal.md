## Critical Implementation Constraints

### Forbidden

- Do not alter financial calculation formulas, account-nature aggregation, transaction data, API contracts, or persistence models.
- Do not make asset movement equal to `Income + Expense`; asset movement and period net result remain different metric families.
- Do not introduce a new report route, new frontend framework, or shared abstraction beyond the existing economic-state panels and localization resources.
- Do not modify unrelated report pages or the active reports-information-architecture change.

### Required

- Apply the global focused month consistently to the Monthly Overview table and its CSV export in the Asset, Income, and Expense Evolution tabs.
- Make the selected month, rather than the system current month, the explicit context shown by those overviews when external filters are in use.
- Clarify the Asset Evolution monthly movement label and explain its non-equivalence to the Snapshot period net result.
- Preserve existing stock-versus-flow formulas and cover the changed behavior with deterministic Web UI tests.

## Why

The Economic State page accepts a global focused month, but its three Monthly Overview tables and exports still use the system current month. This produces conflicting contexts on the same screen. In addition, the Asset Evolution monthly movement can be mistaken for the Snapshot `Income + Expense` result even though they measure different financial concepts.

## What Changes

- Make the Asset, Income, and Expense Evolution Monthly Overview tables show January through the globally focused month for the selected year, including their selected-row treatment and CSV row set.
- Replace current-month-only overview context text with explicit selected-period context when the page supplies external filters.
- Keep Asset Evolution calculations unchanged, but label its first monthly value as an asset movement and present a concise explanation that it is a stock delta, not the Snapshot period net result.
- Add Web UI regression coverage for focused-month table, badge, and export behavior, and for the asset-versus-period-net semantic clarification.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `economic-state-reporting`: The integrated Economic State page must keep its Monthly Overview and semantic context aligned with the selected focused month.
- `monthly-balance-evolution-reporting`: Integrated Asset, Income, and Expense Evolution overview tables and exports must use the focused month as their visible reporting cutoff.
- `reporting-metric-semantics`: The Asset Evolution movement and Snapshot period net result must be explicitly presented as non-comparable metric families.

## Impact

- Web components: `EconomicStatePage.razor`, `AssetTotalEvolutionPanel.razor`, `IncomeEvolutionPanel.razor`, and `ExpenseEvolutionPanel.razor`.
- Localization: the shared default, English, and Spanish resource files for selected-period and asset-movement copy.
- Tests: focused Web UI report tests in `tests/FamilyFinances.Web.Tests/Features/Reports/`.
- APIs, application handlers, database schema, transaction data, and OpenAPI contracts: no changes.

## Non-Goals

- Reconciling asset movement to period net result or exposing a new cash-flow reconciliation report.
- Changing the meaning, signs, ordering, or twelve-bucket API response of monthly evolution data.
- Changing focused-month behavior on report pages outside `/reports/economic-state`.
- Redesigning the report layout or the Reports index.

## Release Impact

Type: patch
Rationale: This is a backward-compatible correction of report filter context and metric wording with no API, data-model, or calculation change.

## Rollback Plan

- Revert the four Economic State Web component changes and their resource entries.
- Revert the focused-month and semantic-clarity test additions.
- No API, database, migration, or data rollback is required.
