# FamilyFinances

A modular monolith in .NET for **family/personal finance management**, built with a dual purpose:

1) A **truly usable** app to manage real finances  
2) A **technical lab** to learn architecture, best practices, and modern integrations

## Core Functional Goals

FamilyFinances is a ledger-first system:

- Manage **accounts**, **income**, **expenses**, **transfers**, **loans/debts**
- Accounting-style **ledger model**:
  - A `Transaction` can contain multiple `TransactionSplits`
  - Splits must be **balanced** (sum = 0)

### Examples

- **Mortgage payment**
  - Bank account → Principal + Interest
- **Salary**
  - Income → Bank account + Withholdings/Taxes

### Additional features (planned)

- Transaction references: refunds, adjustments, reversals
- Payees/merchants:
  - Autocomplete
  - Default category suggestions
- Templates to minimize repetitive entry
- Reports:
  - Monthly summary
  - By category
  - By account

## Architecture

**Modular Monolith** in .NET:

- `Domain` — business rules, entities, value objects (no external dependencies)
- `Application` — use cases, commands/queries, validation, authorization requirements
- `Infrastructure` — EF Core, SQLite, Identity persistence, logging plumbing
- `Api` — REST endpoints, authentication, authorization, versioning (`/api/v1`)
- `Tests` — unit + integration tests

Persistence: **SQLite + EF Core**.

## API Versioning

API is versioned by route:

- `/api/v1/...`

## Infrastructure from Day 1

- Structured logging with **Serilog**
- Authentication & authorization:
  - **ASP.NET Core Identity**
  - Roles: `Admin`, `Reader`
  - Policies: `CanRead`, `CanWrite`
- Health checks
- Prepared for future observability:
  - Elastic stack integration
  - OpenTelemetry

## Repository Workflow

- GitHub repository
- Semantic Versioning (**SemVer**)
- Conventional Commits
- GitHub Releases when milestones are cut

### Conventional Commits

Allowed types:

- `feat`, `fix`, `refactor`, `test`, `chore`, `docs`

Examples:

- `feat(auth): add role-based authorization`
- `fix(ledger): prevent unbalanced splits`

## Roadmap

- `v0.1.0` Infrastructure base (auth, logging, db, CI)
- `v0.2.0` Ledger (transactions + splits + links)
- `v0.3.0` Payees + templates + import
- `v0.4.0` Reports
- `v0.5.0` Account gropus
- `v1.0.0` Stable version for real usage

## Non-Goals (for now)

- Fancy UI (will decide later: Blazor / WinForms / MAUI)
- Multi-tenant / cloud sync
- Complex budgeting features (envelopes, forecasting) — maybe later

## Code Style & Language

- **All code and comments in English**
- Focus on clarity, maintainability, and learning-by-building

---

## Getting Started (soon)

After `v0.1.0`, you should be able to:

- Run the API locally
- Create an admin user
- Call `/api/v1/...` endpoints
- Observe structured logs
- Confirm health checks
- Run tests in CI
