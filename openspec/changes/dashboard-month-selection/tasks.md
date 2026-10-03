## 1. Period Selection And Dashboard State

- [ ] 1.1 Add Dashboard year and month controls with current-period defaults and valid-month constraints.
- [ ] 1.2 Read and update Dashboard `year` and `month` query parameters when the selected period changes.
- [ ] 1.3 Reload the dashboard-overview request, charts, badges, exports, and year-to-date KPI for the selected period.

## 2. Selected-Month Latest Expenses

- [ ] 2.1 Extend the latest-expenses API contract with optional, jointly validated year and month query parameters.
- [ ] 2.2 Scope the application handler and repository query to the requested calendar month while preserving no-parameter behavior.
- [ ] 2.3 Update the Dashboard transaction API client and latest-expenses loading to use the selected period.
- [ ] 2.4 Update the OpenAPI specification and localization resources for the period selection behavior.

## 3. Period-Aware Navigation And Export

- [ ] 3.1 Ensure every Dashboard analytical report link propagates the selected year and month.
- [ ] 3.2 Ensure selected-period CSV exports contain only their visible selected-period data and period metadata.

## 4. Tests And Validation

- [ ] 4.1 Add application and API integration tests for latest-expenses period validation, filtering, ordering, and default compatibility.
- [ ] 4.2 Add Dashboard component and API-client tests for period selection, URL restoration, data reload, exports, and account-group navigation.
- [ ] 4.3 Run `dotnet test FamilyFinances.sln --no-restore` and `openspec validate dashboard-month-selection --strict`.
