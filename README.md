# FamilyFinances

[![CI Quality](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/ci-quality.yml/badge.svg?branch=main)](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/ci-quality.yml)
[![CodeQL](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/codeql.yml)
[![Dependency Review](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/carlos-aisa/FamilyFinances/actions/workflows/dependency-review.yml)
[![Coverage](docs/badges/coverage.svg)](docs/badges/coverage.svg)
[![Latest Release](https://img.shields.io/github/v/release/carlos-aisa/FamilyFinances?sort=semver)](https://github.com/carlos-aisa/FamilyFinances/releases)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Ledger-first personal finance platform built as a modular monolith in .NET, focused on accounting correctness, deterministic reporting, and production-grade engineering practices.

## Project Highlights

- Domain-driven architecture with explicit boundaries (`Domain`, `Application`, `Infrastructure`, `Api`, `Web`).
- Ledger correctness rules (balanced splits, adjustment-first behavior for historical integrity).
- Real testing strategy across unit, integration, and web component layers.
- CI quality gates, dependency risk controls, CodeQL scanning, and automated release pipeline.
- Installer-first Windows distribution flow (setup bootstrapper + MSI artifacts).

## Architecture

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

### Solution layout

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
|-- openspec/
|-- tools/
`-- .github/workflows/
```

## Technology Stack

- Runtime: .NET 9 (`net9.0`)
- Backend: ASP.NET Core Web API, Identity, JWT, EF Core 9, API Versioning
- Frontend: Blazor Web App (Interactive Server), Razor Components
- Persistence: SQLite (current runtime provider)
- Observability and docs: Serilog, Swagger/OpenAPI
- Testing: xUnit, FluentAssertions, Moq, bUnit, ASP.NET Core integration testing
- CI/CD: GitHub Actions (quality, security, releases, artifact hygiene)

## Engineering Quality Gates

| Area | Mechanism | Signal |
| --- | --- | --- |
| Build + tests | `ci-quality.yml` | Required on `main` |
| Coverage visibility | Auto-generated badge + run summary | Continuous trend tracking |
| Dependency risk | `dependency-review.yml` | PR-level dependency diff checks |
| Security analysis | `codeql.yml` | Code scanning alerts |
| Release control | `release-windows.yml` | Versioned, reproducible installer artifacts |

## Product Surface (Current)

- Dashboard: KPI strip, monthly trends, group-state charts, compact insights.
- Quick Entry: dedicated workspace for expense/income/transfer/refund flows.
- Reporting Suite: Economic State, Period Summary, Account Totals, Account Group Totals.
- Backup & Restore: admin-only deterministic backup package with restore precheck/apply flow.

Detailed implementation notes are in the [docs](docs) folder, including reporting evolution and accessibility closeout notes.

## Local Run (Fast Path)

### Prerequisites

- .NET 9 SDK
- Git
- Optional: Docker Desktop (for local PostgreSQL experiments)
- Optional: Node.js 20+ (if you run Cypress locally)

### 1) Clone

```bash
git clone https://github.com/carlos-aisa/FamilyFinances.git
cd FamilyFinances
```

### 2) Start API

```bash
dotnet restore
dotnet run --project src/FamilyFinances.Api
```

API endpoints:

- `http://localhost:5084/health`
- `http://localhost:5084/swagger` (Development)

### 3) Start Web

In a second terminal:

```bash
dotnet run --project src/FamilyFinances.Web
```

Web app:

- `http://localhost:5019`

### 4) Default seeded admin

- Email: `admin@familyfinances.local`
- Password: `Admin123!`

## Testing Matrix

Run all tests:

```bash
dotnet test
```

Run key suites:

```bash
dotnet test tests/FamilyFinances.Domain.Tests
dotnet test tests/FamilyFinances.Application.Tests
dotnet test tests/FamilyFinances.Api.IntegrationTests
dotnet test tests/FamilyFinances.Web.Tests
```

## Releases and Distribution

### Automated Windows release

Pushes to `main` trigger `.github/workflows/release-windows.yml`, which:

- computes the next SemVer tag (`vX.Y.Z`) if needed,
- runs critical reporting test gates,
- builds installer assets,
- publishes GitHub Release artifacts.

Primary assets:

- `FamilyFinances-v<version>-win-x64-setup.exe` (bootstrapper)
- `FamilyFinances-v<version>-win-x64.msi`

### Manual local installer build

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\installer\windows\build-installer.ps1 -Version 0.9.7 -Configuration Release
```

## Documentation Map

- Architecture and standards: [openspec](openspec)
- Feature and sprint notes: [docs](docs)
- Release and operational details: [docs/windows-installer-lan-operations.md](docs/windows-installer-lan-operations.md)
- Reporting regression checklist: [docs/v0.9-reporting-regression-checklist.md](docs/v0.9-reporting-regression-checklist.md)

## Contributing and Governance

- Contribution guide: [CONTRIBUTING.md](CONTRIBUTING.md)
- Security policy: [SECURITY.md](SECURITY.md)
- Pull request template: [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md)
- Issue templates: [.github/ISSUE_TEMPLATE](.github/ISSUE_TEMPLATE)
- Release notes categories: [.github/release.yml](.github/release.yml)

## Implementation Notes

This repository emphasizes clarity across architecture, delivery, and operations:

- business-domain modeling with long-term maintainability,
- quality-first CI with measurable gates,
- security and dependency governance,
- release automation and installer distribution,
- documentation discipline for team scalability.

## License

This project is licensed under the MIT License.
See [LICENSE](LICENSE) for details.
