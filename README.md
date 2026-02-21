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
  - `Asset Evolution` tab: annual asset-total evolution (table + chart)
- `Account Totals` (`/reports/account-totals`)
  - `Period Totals` tab
  - `State Evolution` tab: annual account evolution and composition
- `Account Group Totals` (`/reports/account-group-totals`)
  - `Period Totals` tab
  - `State Evolution` tab: annual group evolution and expense-oriented composition

### Reporting API Evolution Endpoint
- Primary endpoint: `GET /api/v1/reports/state-evolution?year=YYYY&scope=<accounts|asset-total|account-groups>`
- Backward-compatible alias: `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<accounts|asset-total|account-groups>`

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
