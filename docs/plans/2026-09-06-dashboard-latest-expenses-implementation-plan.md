# Implementation Plan: Dashboard Latest Expenses

## Scope and assumptions

This plan implements the approved Dashboard replacement only. The card limit is fixed at six movements. An expense movement is a transaction with at least one split whose account has `AccountNature.Expense`; refunds are included when they meet that criterion, and the displayed amount remains an absolute positive magnitude. The existing untracked `.superpowers/` directory remains outside scope.

## 1. Add a bounded expense-movement read contract

- Add a focused DTO in `src/FamilyFinances.Application/Ledger/Transactions/Dtos/` containing the transaction identifier, booking date, optional description, optional payee name, and amount in cents.
- Extend `ITransactionRepository` with a bounded `ListLatestExpensesAsync` method. It must not expose `IQueryable` and must receive the limit and cancellation token.
- Add a small application handler which uses a named limit constant of `6`, maps repository results to the DTO, and preserves cancellation.
- Add unit tests under `tests/FamilyFinances.Application.Tests/Ledger/Transactions/` for the fixed limit forwarding and DTO mapping. Mocks are limited to the repository boundary.

## 2. Implement a deterministic relational query and API endpoint

- Update `src/FamilyFinances.Infrastructure/Persistence/Repositories/TransactionRepository.cs` to query transactions with an Expense-nature split, include only required related data, and return no more than the supplied limit.
- Apply ordering in the database by `BookedOn` descending and transaction ID descending. Avoid split duplication by selecting distinct transactions before applying the limit.
- Add a read-authorized `GET /api/v1/transactions/latest-expenses` action to `TransactionsController`, returning the new DTO collection through the focused handler.
- Update the API source of truth (`openspec/api-spec.yaml`, if present in the current checkout) with this endpoint and response schema; no request parameters or write behavior are introduced.
- Add real-provider integration tests that seed Expense, Income, and Asset-only transactions, including equal booking dates, then verify filtering, six-item limit, exact ordering, fields, and `CanRead` authorization.

## 3. Add the reusable movement-list presentation component

- Create a focused Razor component in `src/FamilyFinances.Web/Components/Shared/` and a presentation item record in the corresponding feature/shared location.
- The component receives a collection of already-prepared items and renders a date, description when present, payee when present, and `Math.Abs` amount using `MoneyFormatter.FormatCents`.
- Use neutral/current text styling for amounts; do not apply `text-danger`, signed formatting, or result-color semantics. Reuse existing tokens and Bootstrap classes only.
- Supply localized empty-state copy through `SharedResource.resx`, `SharedResource.es-ES.resx`, and `SharedResource.en-US.resx`.
- Add bUnit coverage for rows, omitted optional fields, empty state, and an amount whose markup is positive and neutral.

## 4. Replace the Dashboard card and connect the query

- Add `GetLatestExpensesAsync` to `src/FamilyFinances.Web/Api/TransactionsApi.cs`, following existing token, authorization, cancellation, and error conventions.
- Update `DashboardPage.razor` to inject `TransactionsApi`, load the collection independently alongside the overview dependencies, map its DTOs to the reusable presentation-item type, and replace the `dashboard-monthly-summary` markup with an `Últimos gastos` card and the reusable list.
- Remove Dashboard-only monthly-summary state and rendering usage from this page. Leave existing overview/report contracts and unrelated consumers untouched; remove the obsolete builder only if repository references prove it is unused after the replacement.
- Update Dashboard page tests to mock the new endpoint, verify its card and item content, verify the absence of the monthly-summary card, and preserve existing loading/error/data-sufficiency assertions.

## 5. Documentation and verification

- Keep the approved design document aligned if implementation reveals a material change; no OpenSpec change is active for this work, so no archive documents are modified by default.
- Run `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release`, `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release`, and `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release`.
- Run `dotnet build FamilyFinances.sln -c Release`, `git diff --check`, and validate the API specification with its repository-defined check if available.

## Explicit non-goals

- No forecasting, recurrence, scheduled movement model, configurable widget, generic feed provider, pagination, or user-configurable item count.
- No change to the existing general transaction-list endpoint or to monthly-summary report behavior.
