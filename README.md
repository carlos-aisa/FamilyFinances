# FamilyFinances

## Overview

### Purpose
FamilyFinances is a ledger-first personal finance application built as a modular monolith in .NET.

Its goals are:
- Manage real family/personal finances with double-entry style transactions.
- Keep accounting correctness (balanced splits, immutable historical behavior through adjustment transactions).
- Provide a practical architecture playground for clean layering, testing, and operational practices.

### Architecture
The solution follows a layered modular monolith:

```text
[FamilyFinances.Web (Blazor UI)]
             |
             v
     [FamilyFinances.Api]
             |
             v
      [Application Layer]
             |
             v
        [Domain Layer]
             ^
             |
   [Infrastructure Layer]
    (EF Core, Identity, SQLite)
```

Main architectural characteristics:
- `Domain`: entities, value objects, and business rules.
- `Application`: use-case handlers and application orchestration.
- `Infrastructure`: repositories, EF Core contexts/configuration/migrations, Identity, JWT wiring.
- `Api`: REST controllers, auth/authz, versioning, health checks, Swagger.
- `Web`: Blazor Web App (Interactive Server) that consumes the API.

## Technologies

### Backend
- .NET 9 (`net9.0`)
- ASP.NET Core Web API
- ASP.NET Core Identity
- JWT Bearer authentication
- EF Core 9
- SQLite (current runtime persistence)
- Serilog
- Swagger / OpenAPI (Swashbuckle)
- API Versioning (`Asp.Versioning.Mvc`)

### Frontend
- ASP.NET Core Blazor Web App (Interactive Server)
- Razor Components
- Bootstrap (static assets)

### DevOps & Testing
- xUnit
- FluentAssertions
- Moq
- bUnit (web component tests)
- `Microsoft.AspNetCore.Mvc.Testing` (integration tests)
- Coverlet collector
- GitHub Actions (CI + Windows ZIP distribution build)
- Docker/PostgreSQL: not wired as default runtime in current codebase (see setup notes)
- Cypress: not committed in the repository as of now (setup path provided below)

## Folder Structure

```text
FamilyFinances/
|-- src/
|   |-- FamilyFinances.Domain/
|   |-- FamilyFinances.Application/
|   |-- FamilyFinances.Infrastructure/
|   |-- FamilyFinances.Api/
|   `-- FamilyFinances.Web/
|-- tests/
|   |-- FamilyFinances.Domain.Tests/
|   |-- FamilyFinances.Application.Tests/
|   |-- FamilyFinances.Api.IntegrationTests/
|   `-- FamilyFinances.Web.Tests/
|-- docs/
|-- dist/
|-- openspec/
|-- FamilyFinances.sln
`-- README.md
```

## Setup Instructions

### Prerequisites
- .NET 9 SDK
- Git
- Docker Desktop (for PostgreSQL container workflow)
- Node.js 20+ (only if you want Cypress E2E locally)

1. Clone the Repository

```bash
git clone https://github.com/carlos-aisa/FamilyFinances.git
cd FamilyFinances
```

2. Environment Configuration

Backend and frontend run with default local settings:
- API defaults to `http://localhost:5084`
- Web defaults to `http://localhost:5019`

Relevant files:
- `src/FamilyFinances.Api/appsettings.json`
- `src/FamilyFinances.Api/appsettings.Development.json`
- `src/FamilyFinances.Web/appsettings.json`

Default seeded admin account (created on startup):
- Email: `admin@familyfinances.local`
- Password: `Admin123!`

3. Database Setup

Current implementation:
- The app uses SQLite through EF Core (`UseSqlite`) and creates/migrates DB on startup.
- Default development connection string: `Data Source=familyfinances.db`.

PostgreSQL via Docker (optional environment bootstrap):

```bash
docker run --name familyfinances-postgres \
  -e POSTGRES_DB=familyfinances \
  -e POSTGRES_USER=familyfinances \
  -e POSTGRES_PASSWORD=familyfinances \
  -p 5432:5432 \
  -d postgres:16-alpine
```

Important:
- PostgreSQL is not the active runtime provider in current code.
- To run the app on PostgreSQL, code changes are required (EF provider/wiring currently targets SQLite).

4. Backend Setup

```bash
dotnet restore
dotnet run --project src/FamilyFinances.Api
```

Backend endpoints:
- API base URL: `http://localhost:5084`
- Health check: `http://localhost:5084/health`
- Swagger UI (Development): `http://localhost:5084/swagger`

5. Frontend Setup

In a second terminal:

```bash
dotnet run --project src/FamilyFinances.Web
```

Frontend URL:
- `http://localhost:5019`

6. Testing Setup

Backend and web test projects are included in the solution and run with `dotnet test`.

Cypress testing suite setup (optional, not committed in repo yet):

```bash
npm init -y
npm install --save-dev cypress
npx cypress open
```

Recommended Cypress base URL:
- `http://localhost:5019`

## Testing

### Backend Tests
Run all tests:
```bash
dotnet test
```

Run specific suites:
```bash
dotnet test tests/FamilyFinances.Domain.Tests
dotnet test tests/FamilyFinances.Application.Tests
dotnet test tests/FamilyFinances.Api.IntegrationTests
```

### Frontend Tests
Web/component tests:
```bash
dotnet test tests/FamilyFinances.Web.Tests
```

E2E tests:
- Cypress suite is not currently versioned in this repository.
- If initialized locally, run with your Cypress commands (`npx cypress open` / `npx cypress run`).

## Database schema
The system uses two EF Core contexts:

- `AppIdentityDbContext` (Identity)
  - ASP.NET Core Identity tables for users, roles, claims, logins, tokens.

- `LedgerDbContext` (finance domain)
  - `Accounts`
  - `Payees`
  - `Transactions`
  - `TransactionSplits`
  - `TransactionLinks`
  - `AccountGroups`
  - `AccountGroupMembers`

Schema source:
- EF Core migrations under `src/FamilyFinances.Infrastructure/Persistence/Migrations/Ledger`
- Identity migrations under `src/FamilyFinances.Infrastructure/Migrations`

## API Documentation
- Route versioning format: `/api/v1/...`
- Swagger/OpenAPI is enabled in Development mode.
- Primary endpoint groups include:
  - Auth
  - Accounts
  - Payees
  - Transactions
  - Account Groups
  - Reports
  - Health

### Reporting Map (Current UI)
- `Economic State` (`/reports/economic-state`)
  - `Snapshot` tab: current stock + period flow KPIs
  - `Asset Evolution` tab: annual asset-total evolution (table + chart) + focused-month daily asset chart + CSV/PNG export actions
  - `Income Evolution` tab: annual income-total evolution (table + chart) + focused-month daily income chart + CSV/PNG export actions
- `Period Summary` (`/reports/monthly-summary`)
  - period flow KPIs (`Income`, `Expense`, `Period Net Result`, `Transactions Count`)
  - account-focused month chart (when an account is selected)
  - insight panel with `Groups/Payees` toggle:
    - expense and income Pareto rankings
    - top-N concentration percentages with explicit denominator
    - monthly anomaly badges (`Anomaly` / `Normal` / `Insufficient history`) with explanation
- `Account Totals` (`/reports/account-totals`)
  - `Period Totals` tab + CSV export
  - `State Evolution` tab: annual account evolution and composition + overview CSV export + chart PNG export
- `Account Group Totals` (`/reports/account-group-totals`)
  - `Period Totals` tab + CSV export
  - `State Evolution` tab: annual group evolution, expense-oriented composition, focused-month daily group-evolution chart + overview CSV export + chart PNG export

### Reporting Exports & Accessibility (`0.9.6`)
- CSV exports are available on table-based report views and include active filter context.
- Chart cards provide `Export PNG` actions for current visible chart state.
- Reporting controls/charts include accessibility baseline improvements (explicit labels, focusable chart surfaces).
- See detailed notes:
  - `docs/reporting-export-accessibility.md`
  - `docs/v0.9-reporting-regression-checklist.md`
  - `docs/v0.9.6-closeout-notes.md`

### Reporting API Evolution Endpoint
- Primary endpoint: `GET /api/v1/reports/state-evolution?year=YYYY&scope=<accounts|asset-total|account-groups>`
- Backward-compatible alias: `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<accounts|asset-total|account-groups>`

### Reporting API Monthly Chart Endpoints (`0.9.4`)
- `GET /api/v1/reports/monthly-charts/balance?year=YYYY&month=MM`
  - Returns day-bucket `Asset Total` end-balance points for the selected month.
  - Optional: `accountId=<GUID>` returns day-bucket end-balance points for the selected account (used by `Period Summary` account-focused chart).
  - No-activity days use deterministic carry-forward values.
- `GET /api/v1/reports/monthly-charts/group-evolution?year=YYYY&month=MM`
  - Returns one `Asset Total` series plus one series per account group (liability accounts excluded from group aggregation).
  - All returned series are aligned on the same day buckets (`1..daysInMonth`).
  - Legacy alias still available: `GET /api/v1/reports/monthly-charts/balance-vs-groups?year=YYYY&month=MM`.

### Reporting API Insight Endpoints (`0.9.5`)
- `GET /api/v1/reports/insights/pareto?from=YYYY-MM-DD&to=YYYY-MM-DD&dimension=<group|payee>&topN=<1..20>`
  - Returns deterministic Pareto and concentration payload for both `Expense` and `Income` in one response.
  - Optional filters: `accountId=<GUID>`, `payeeId=<GUID>` (except `payeeId` is rejected when `dimension=payee`).
- `GET /api/v1/reports/insights/anomalies?year=YYYY&month=MM&nature=<Expense|Income>&dimension=<group|payee>&lookbackMonths=<3..36>&requiredHistoryMonths=<2..12>`
  - Returns deterministic anomaly evaluation for the requested month and dimension.
  - Contributor rows include baseline mean, threshold, z-score (when available), and explanation text.
  - Contributors with sparse history are returned as `Insufficient history` and are never flagged as anomaly.

## Contributing
Suggested workflow:
- Create a feature branch from `main`.
- Keep changes aligned with layered architecture boundaries.
- Add/adjust tests in the corresponding test project.
- Use conventional commit messages (`feat`, `fix`, `refactor`, `test`, `docs`, `chore`).
- Open a pull request with scope, rationale, and testing evidence.

## License
No `LICENSE` file is currently present in the repository.

## Support
- Open an issue in the GitHub repository for bugs or feature requests.
- Include environment details, reproduction steps, and logs when reporting runtime problems.
