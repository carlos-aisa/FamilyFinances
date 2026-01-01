# Architecture (arch.md)

## 1. Overview

FamilyFinances is a **modular monolith** designed to balance:

- Real-world usability (finance tracking)
- A learning-focused codebase (clean architecture, modern .NET practices)

The system follows a layered approach:

- **Domain**: pure business model and rules
- **Application**: use cases and orchestration (CQRS-style if useful)
- **Infrastructure**: persistence, identity, logging, integrations
- **Api**: HTTP boundary (REST, auth, policies, versioning)
- **Tests**: unit and integration coverage

Key principles:

- Domain has **no dependency** on EF Core, ASP.NET, or other frameworks
- Application depends on Domain, but not on Infrastructure details
- Infrastructure implements interfaces defined in Application (or Domain when appropriate)

## 2. Bounded Context (Single Context, Modular)

We start with one bounded context: **Personal/Family Ledger**.
Modules are separated by project boundaries and namespaces, not by microservices.

Future extraction (if ever needed) should be possible because:

- Use cases are explicit
- Interfaces and boundaries are respected
- Persistence details are isolated

## 3. Domain Model

### 3.1 Ledger Invariants

- A Transaction consists of one or more Splits
- The sum of split amounts must equal **0** (balanced transaction)
- Splits represent the accounting legs (debit/credit-style)

### 3.2 Entities (initial)

- `Account`
  - Represents a bank account, cash, credit card, loan account, etc.
- `Transaction`
  - Header-level information: date, description, payee, metadata
- `TransactionSplit`
  - Amount (+/-), account, optional category, memo
- `Payee`
  - Merchant/person with defaults (category suggestions)
- `Category`
  - Optional, can evolve (flat vs hierarchical later)
- `TransactionLink`
  - Reference between transactions (refund, reversal, adjustment)

### 3.3 Money Representation

Use a dedicated Money type approach:

- Store amounts as integer minor units (e.g., cents) + currency code
- Avoid floating point for money

(Exact implementation choice will be decided during `v0.2.0`.)

## 4. Application Layer

Application defines:

- Use cases (commands/queries)
- Validation rules
- Authorization requirements (policies)
- Interfaces for persistence and services (repositories, unit of work, etc.)

Pattern guidelines:

- Prefer explicit use cases over “fat services”
- Keep side effects in Infrastructure
- Keep Application testable with mocks/fakes

## 5. Infrastructure Layer

Infrastructure provides:

- EF Core DbContext + SQLite mappings
- ASP.NET Core Identity stores
- Serilog sinks/enrichers configuration plumbing
- Migrations, seed data (admin user), and other environment concerns

Observability preparation:

- Add extension points for OpenTelemetry and Elastic later
- Avoid hard-coding vendors in Domain/Application

## 6. API Layer

Responsibilities:

- REST endpoints under `/api/v1`
- Authentication (Identity)
- Authorization (roles + policies)
- DTOs and mapping between API contracts and Application models
- Health checks endpoint(s)

API versioning strategy:

- Route-based: `/api/v1`
- Keep contracts stable within a major version
- Introduce breaking changes with `/api/v2` + major SemVer bump

## 7. Testing Strategy

- Domain unit tests:
  - Invariants (balanced splits)
  - Money arithmetic
  - Linking rules (reversal/refund constraints)
- Application unit tests:
  - Use case validation and policy checks
- Integration tests:
  - API + DB (SQLite)
  - Authentication/authorization flows

## 8. Security Model

- ASP.NET Core Identity for user management
- Roles:
  - `Admin`: full read/write
  - `Reader`: read-only
- Policies:
  - `CanRead`: required for GET endpoints
  - `CanWrite`: required for POST/PUT/DELETE endpoints

Design goal:

- Authorization checks should be visible at the use-case/API boundary
- Domain stays free of user concepts

## 9. Logging

- Structured logs with Serilog
- Enrich with:
  - Correlation/Trace Id
  - User Id (when authenticated)
  - Request path and response status
- Ensure logs are useful locally and in CI
- Keep sinks minimal early (Console + File), prepare for Elastic later

## 10. Development Plan (Versioned)

See the roadmap below (also referenced in README). Each milestone ends with:

- A tagged release
- Changelog notes (GitHub Release)
- CI green status

### v0.1.0 — Infrastructure Baseline

Goal: runnable API with auth, policies, logging, SQLite, health checks, CI.

Deliverables:

- Solution structure + project references
- Serilog configured (console + rolling file)
- SQLite + EF Core DbContext wired (even if schema minimal)
- ASP.NET Core Identity configured
- Roles + policies implemented and enforced
- Health checks endpoint(s)
- GitHub Actions CI (build + test)

### v0.2.0 — Ledger Core

Goal: balanced transactions with splits and transaction references.

Deliverables:

- Domain model for Transaction/Splits with invariants
- Persistence mappings + migrations
- CRUD endpoints for accounts and transactions
- Link/reference support (refund/reversal/adjustment)
- Core tests (Domain + integration)

### v0.3.0 — Payees, Templates, Import

Goal: reduce manual entry and support basic import.

Deliverables:

- Payees with autocomplete + defaults
- Templates (e.g., salary, mortgage)
- Basic import pipeline (e.g., CSV of bank statement)
- Improvements to UX of API contracts (filters, pagination as needed)

### v0.4.0 — Reports

Goal: actionable views.

Deliverables:

- Monthly summary
- Category breakdown
- Account breakdown
- Performance considerations (indexes, query tuning)

### v1.0.0 — Stable Real-Use

Goal: stable, documented, usable for day-to-day.

Deliverables:

- Backward compatible API (v1)
- Data migration story documented
- Minimum UI chosen + functional flows
- Hardening: error handling, validation polish, docs, packaging

## 11. Release & Git Workflow

- SemVer tags: `v0.1.0`, `v0.2.0`, ...
- Conventional Commits enforced (recommended with a linter later)
- GitHub Releases created for milestone versions
- Keep a CHANGELOG generated from commits (optional later)
