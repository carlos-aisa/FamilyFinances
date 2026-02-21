## 1. Backend economic-state read model

- [x] 1.1 Add `GetEconomicStateQuery` and handler in Application reporting layer with canonical stock/flow KPI formulas.
- [x] 1.2 Extend reporting repository abstraction and infrastructure implementation to provide required aggregates for as-of date and current period.
- [x] 1.3 Add API endpoint `GET /api/v1/reports/economic-state` in reports controller with input validation and DTO mapping.

## 2. Web API client and report page

- [x] 2.1 Extend `ReportsApi` web client with `GetEconomicStateAsync(asOfDate)` method.
- [x] 2.2 Create `/reports/economic-state` page with as-of date filter (default today), load action, and KPI cards.
- [x] 2.3 Add explicit stock/flow semantic labeling and disclaimer text in the new page.

## 3. Navigation and dashboard entry

- [x] 3.1 Add `Economic State` card in `/reports` index and wire navigation.
- [x] 3.2 Add navigation-menu shortcut entry/CTA to `/reports/economic-state`.
- [x] 3.3 Ensure navigation preview includes an explicit as-of reference when showing Asset Balance preview values.

## 4. Test updates and validation

- [x] 4.1 Add/extend Application tests for economic-state query formula correctness.
- [x] 4.2 Add/extend API integration tests for success and validation failure scenarios of `/reports/economic-state`.
- [x] 4.3 Add/extend Web page tests for route availability, default date behavior, KPI rendering, and dashboard/report navigation.

## 5. Release readiness checks

- [x] 5.1 Run `dotnet test` for impacted projects (Application, API integration, Web tests) in Release configuration.
- [x] 5.2 Review report copy to ensure canonical metric naming consistency with `0.9.1`.
- [x] 5.3 Document `0.9.2` release notes with endpoint, UI changes, and known limitations.
