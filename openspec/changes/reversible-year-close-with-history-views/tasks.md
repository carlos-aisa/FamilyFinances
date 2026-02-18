## 1. Fiscal-Year Persistence Model and Migration

- [x] 1.1 Add fiscal-year closure and account-year snapshot entities and mappings in `src/FamilyFinances.Infrastructure/Persistence` (including configuration classes under `src/FamilyFinances.Infrastructure/Persistence/Configurations`) with keys/columns defined in design (`Year`, `IsClosed`, metadata, `(Year, AccountId)`, `ClosingBalanceCents`).
- [x] 1.2 Register new DbSets in `src/FamilyFinances.Infrastructure/Persistence/LedgerDbContext.cs` and ensure model snapshot updates include the new tables and indexes (`AccountId+Year` lookup and any closure-status index used by query paths).
- [x] 1.3 Create and validate EF migration files under `src/FamilyFinances.Infrastructure/Persistence/Migrations/Ledger` for new governance tables only, without modifying ledger transaction table semantics.

## 2. Governance Application Abstractions and Services

- [x] 2.1 Introduce fiscal-year governance abstractions in `src/FamilyFinances.Application` (for example service/repository interfaces to check closed status, close year, reopen year, and manage snapshots) with explicit method signatures used by handlers.
- [x] 2.2 Implement infrastructure repositories/services in `src/FamilyFinances.Infrastructure/Persistence/Repositories` (or `Services`) that persist closure status, compute year-end account balances, and store/remove snapshots per reopen/close rules.
- [x] 2.3 Register new governance services in `src/FamilyFinances.Infrastructure/DependencyInjection.cs` and keep dependency direction compliant (Application abstractions, Infrastructure implementations).

## 3. Closed-Year Mutation Guard Integration

- [x] 3.1 Enforce closed-year guard in transaction create flow (`src/FamilyFinances.Application/Ledger/Transactions/Handlers/CreateTransactionHandler.cs`) using `BookedOn.Year` before persistence.
- [x] 3.2 Enforce closed-year guard in transaction update flows (`src/FamilyFinances.Application/Ledger/Transactions/Handlers/UpdateTransactionCommandHandler.cs`, `src/FamilyFinances.Application/Ledger/Transactions/Handlers/UpdateMultiSplitTransactionHandler.cs`) based on target transaction year policy.
- [x] 3.3 Enforce closed-year guard in delete flow (`src/FamilyFinances.Application/Ledger/Transactions/Handlers/DeleteTransactionHandler.cs`) using stored transaction booked year.
- [x] 3.4 Enforce closed-year guard in reconciliation flow (`src/FamilyFinances.Application/Ledger/Accounts/Handlers/ReconcileAccountHandler.cs`) using `AsOfDate.Year` before adjustment transaction creation.
- [x] 3.5 Ensure closed-year violations surface through existing domain error middleware path in `src/FamilyFinances.Api/Middleware/DomainExceptionMiddleware.cs` with clear error message text.

## 4. Running-Balance Performance Path with Snapshots

- [x] 4.1 Extend movement query logic in `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs` to compute running-balance baseline from year snapshots when available.
- [x] 4.2 Keep fallback behavior in `ReportingReadRepository` for accounts/years with no snapshots and validate output parity against legacy calculations.
- [x] 4.3 Refactor counterparty lookup path in movement query to avoid unnecessary per-row query amplification where practical while preserving current response contract.
- [x] 4.4 Add/adjust supporting query indexes in persistence configuration/migration for date/snapshot lookup performance used by movement calculations.

## 5. Fiscal-Year Governance API Surface

- [x] 5.1 Add API endpoints for fiscal-year status listing, close year, and reopen year in `src/FamilyFinances.Api/Controllers` (new controller or extension pattern) using existing auth policies and API version conventions.
- [x] 5.2 Add corresponding request/response DTOs in Application layer for governance operations and wire handlers through dependency injection.
- [x] 5.3 Validate idempotent-safe behavior and response semantics for repeated close/reopen commands on same year.

## 6. Historical Read-Only API and Web Views

- [x] 6.1 Add read-only historical retrieval endpoints for year-filtered transactions and year/account-filtered movements in API/Application/Infrastructure layers without exposing mutation actions.
- [x] 6.2 Add History navigation entry in `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` and create new pages under `src/FamilyFinances.Web/Components/Pages/History` for Transactions, Movements, and Year Governance.
- [x] 6.3 Implement history pages using existing table/card UI patterns and ensure all history routes are read-only (no create/edit/delete controls).
- [x] 6.4 Integrate History pages with Web API clients in `src/FamilyFinances.Web/Api` (new or extended client methods) for governance commands and historical queries.

## 7. Operational UI Guard Feedback

- [x] 7.1 Ensure mutation-capable Web pages/components (`TransactionCreatePage`, `TransactionEditPage`, `TransactionDetailPage`, dashboard quick-entry drawer and widgets, opening-balance workflow) surface closed-year backend rejection messages clearly and do not mask policy errors.
- [x] 7.2 Ensure historical views do not render links/buttons that route into mutation pages for historical entries.
- [x] 7.3 Verify operational transactions UI remains unchanged for open years except for policy-driven rejection cases.

## 8. Test Coverage Updates

- [x] 8.1 Add/adjust Application tests in `tests/FamilyFinances.Application.Tests` for close/reopen service behavior and mutation guard rejection in create/update/delete/reconcile handlers.
- [x] 8.2 Add API integration tests in `tests/FamilyFinances.Api.IntegrationTests` covering: close year success, blocked create/update/delete/reconcile in closed year, reopen year success, and post-reopen mutation allowed.
- [x] 8.3 Add integration tests validating historical transactions and historical movements read-only retrieval contracts and running-balance correctness with snapshot baseline.
- [x] 8.4 Add/adjust Web tests in `tests/FamilyFinances.Web.Tests` where applicable for history navigation/read-only rendering and closed-year error surfacing in mutation screens.

## 9. Validation and Release Readiness

- [x] 9.1 Run `dotnet build FamilyFinances.sln` and confirm all projects compile with new governance/history components.
- [x] 9.2 Run relevant test suites (`tests/FamilyFinances.Application.Tests`, `tests/FamilyFinances.Api.IntegrationTests`, `tests/FamilyFinances.Web.Tests`) and confirm deterministic pass.
- [x] 9.3 Perform manual end-to-end validation: close year -> mutation blocked -> historical browsing works -> reopen year -> mutation allowed -> re-close recomputes snapshots.
- [x] 9.4 Confirm docs/spec consistency and ensure no accidental API version, auth policy, or archival-storage model changes were introduced beyond approved scope.
