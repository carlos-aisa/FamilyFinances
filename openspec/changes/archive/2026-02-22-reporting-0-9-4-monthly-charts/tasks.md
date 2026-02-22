## 1. Month-level data contracts

- [x] 1.1 Add month-level chart DTOs for daily balance and balance-vs-group datasets.
- [x] 1.2 Add application queries/handlers for month-level datasets with deterministic day ordering.
- [x] 1.3 Extend repository interfaces and implementations for daily bucket aggregation and carry-forward behavior.

## 2. API endpoints

- [x] 2.1 Add endpoint `GET /api/v1/reports/monthly-charts/balance` with query validation.
- [x] 2.2 Add endpoint `GET /api/v1/reports/monthly-charts/balance-vs-groups` with query validation.
- [x] 2.3 Add API tests for success, invalid input, and no-data month responses.

## 3. Web integration

- [x] 3.1 Extend web `ReportsApi` client with month-level chart methods.
- [x] 3.2 Integrate focused-month selector and monthly chart sections in `src/FamilyFinances.Web/Components/Pages/Reports/AssetTotalEvolutionPanel.razor` and `src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupStateEvolutionPanel.razor`.
- [x] 3.3 Ensure monthly chart refresh behavior remains consistent with selected year/month controls and current state-evolution tab context.

## 4. Tests and consistency checks

- [x] 4.1 Add/extend Web feature tests covering focused month selection and monthly chart data refresh in integrated tabs.
- [x] 4.2 Add/extend Application tests for day-bucket ordering and carry-forward semantics.
- [x] 4.3 Add tests ensuring compared series use aligned day buckets.

## 5. Validation and release prep

- [x] 5.1 Run Application, API integration, and Web test suites in Release configuration.
- [x] 5.2 Validate chart/table month-context consistency across key report states.
- [x] 5.3 Document monthly chart semantics and endpoint contracts for `0.9.4`.
