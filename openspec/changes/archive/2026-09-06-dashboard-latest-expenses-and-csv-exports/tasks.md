## 1. Latest Expense Data Contract

- [x] 1.1 Add a latest-expense DTO, application handler, repository contract, and dependency-injection registration.
- [x] 1.2 Implement a read-only transaction query filtering to Expense-nature splits, ordered by booked date descending and transaction identifier descending, with a six-row limit.
- [x] 1.3 Add the authorized transactions endpoint, Web API client method, and OpenAPI path/schema documentation.

## 2. Dashboard Presentation

- [x] 2.1 Replace the Dashboard monthly textual summary with a latest-expenses card and load it through the concrete transactions API client.
- [x] 2.2 Add a reusable movement-list component for display-ready movement items.
- [x] 2.3 Display ISO-style date, description, and positive neutral amount without unfavorable-result color semantics.
- [x] 2.4 Render the expense-kind ranking as a compact visual list and preserve its deterministic percentage behavior.
- [x] 2.5 Add Expense metric-kind handling and monthly-result ordering for highlighted groups.

## 3. CSV Exports

- [x] 3.1 Add an Export CSV button and period badge to highlighted groups, expense-kind ranking, and latest expenses.
- [x] 3.2 Build each card export from its visible columns using the existing CSV builder and browser download interop.
- [x] 3.3 Localize the CSV period label and report export errors in the owning card.

## 4. Tests And Validation

- [x] 4.1 Add unit and integration coverage for latest Expense movement filtering, order, response shape, and client behavior.
- [x] 4.2 Add shared-component and Dashboard rendering coverage for neutral positive amounts, controls, and card layout.
- [x] 4.3 Add Dashboard CSV download coverage for all three third-row cards.
- [x] 4.4 Add integration coverage for highlighted-group monthly-result ordering.
- [x] 4.5 Run `dotnet build FamilyFinances.sln -c Release --no-restore` successfully.
- [x] 4.6 Run `dotnet test FamilyFinances.sln -c Release --no-build` successfully: 778 tests passed.
- [x] 4.7 Run `openspec validate --specs --strict --no-interactive` successfully: 29 specs passed before this archived change was added.
