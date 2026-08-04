## 1. Standalone Report Page

- [ ] 1.1 Add a dedicated authenticated Razor route for the economic-state as-of-date report, with selected-date, loading, and error state following the existing report-page conventions.
- [ ] 1.2 Default the date input to the current local date, set the input maximum to today, and load the report automatically for that date.
- [ ] 1.3 Allow the user to apply an earlier date and reload `ReportsApi.GetEconomicStateAsync` with that exact date.
- [ ] 1.4 Render assets, liabilities, net worth, income, expenses, and period net result without tabs, charts, annual evolution, or exports.
- [ ] 1.5 Render explicit balance-as-of-date and month-to-date flow-period contexts from the returned `AsOf` value.

## 2. Navigation And Localization

- [ ] 2.1 Add the new report entry to the Reports index using the existing report-card/navigation conventions.
- [ ] 2.2 Add required localized strings to default, English, and Spanish shared resources, preserving resource parity.

## 3. Test Coverage

- [ ] 3.1 Add focused Web component tests for the default date, date maximum, automatic initial load, and historical-date reload.
- [ ] 3.2 Add assertions for six metric values, both date contexts, and loading/error states.
- [ ] 3.3 Update Reports index tests to cover the new report entry and destination.

## 4. Validation And Documentation

- [ ] 4.1 Run the focused Web tests for the new page and Reports index.
- [ ] 4.2 Run `dotnet test FamilyFinances.sln --configuration Release`.
- [ ] 4.3 Run `openspec validate economic-state-as-of-date-report --strict` and update the change documentation if implementation changes scope or behavior.
