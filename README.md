# FamilyFinances

A modular monolith in .NET for **family / personal finance management**, built with a dual purpose:

1) A **truly usable** app to manage real finances  
2) A **technical lab** to learn architecture, best practices, and modern integrations

FamilyFinances is actively used to manage real finances, and its roadmap is driven by **real usage needs**, not just planned features.

---

## Core Functional Goals

FamilyFinances is a **ledger-first system**:

- Manage **accounts**, **income**, **expenses**, **transfers**, **refunds/reimbursements**, **adjustments**
- Accounting-style **ledger model**:
  - A `Transaction` can contain multiple `TransactionSplits`
  - Splits must always be **balanced** (sum = 0)
- All balance corrections are done via **new transactions**, never by editing the past

### Examples

- **Mortgage payment**
  - Bank account → Principal + Interest (Liability)
- **Salary**
  - Income → Bank account + Withholdings/Taxes
- **Refund / reimbursement**
  - Expense → Asset or Liability
- **Balance adjustment**
  - Asset → Adjustments (Expense / Income)

---

## Key Features Implemented

- Ledger with transactions and balanced splits
- Accounts:
  - Assets
  - Liabilities
  - Expenses
  - Income
- Transfers, refunds, reimbursements
- Account reconciliation via adjustment transactions
- Payees with autocomplete
- Reports & visibility:
  - Account balances
  - Account movements (ledger per account)
  - Monthly summaries
  - Totals by account and account group
- Authentication & authorization
- Real-time usage feedback driving UX improvements

---

## Architecture

**Modular Monolith** in .NET:

- `Domain` — business rules, entities, value objects (no external dependencies)
- `Application` — use cases, commands/queries, validation, authorization requirements
- `Infrastructure` — EF Core, SQLite, Identity persistence, logging plumbing
- `Api` — REST endpoints, authentication, authorization, versioning (`/api/v1`)
- `Web` — Blazor Web App (Interactive Server), consuming the API as an external client
- `Tests` — unit + integration tests

Persistence: **SQLite + EF Core**.

---

## API Versioning

API is versioned by route:

- `/api/v1/...`

---

## Infrastructure from Day One

- Structured logging with **Serilog**
- Authentication & authorization:
  - **ASP.NET Core Identity**
  - Roles: `Admin`, `Reader`
  - Policies: `CanRead`, `CanWrite`
- Health checks
- Prepared for future observability:
  - OpenTelemetry
  - Elastic stack integration

---

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

---

## Roadmap (realistic & usage-driven)

### Completed
- `v0.1.x` Infrastructure base (auth, logging, db, CI)
- `v0.2.x` Ledger core (transactions, splits, balancing rules)
- `v0.3.x` Payees and basic entry flows
- `v0.4.x` Reporting foundations
- `v0.5.x` Account groups and categorization
- `v0.6.2` Opening balance onboarding
- `v0.6.3` Refunds / reimbursements
- `v0.6.4` Reports & visibility (balances, account movements)
- `v0.6.5` Account adjustments & reconciliation
- `v0.6.6` Polish & bugfix sprint ✅
  - Sign convention fixes (income positive, expenses negative)
  - Transaction timestamps & stable ordering
  - Running balance calculation
  - Date range presets & filters
  - Account/payee search functionality
  - Dark mode polish & visibility improvements
  - UX consistency across reports

### In progress / planned
- `v0.6.7` Distributable Windows build (ZIP)
- `v0.6.8` Internationalization (i18n)

### Future (v0.7+)
- Advanced reports and visualizations
- Templates for repetitive transactions
- Grouped reports (by payee, category)
- Optional automation / import
- Optional observability integrations

---

## Non-Goals (for now)

- Cloud sync / multi-tenant
- Complex budgeting systems (envelopes, forecasting)
- Over-engineered UI frameworks
- Mobile-first focus

---

## Code Style & Language

- **All code, comments, and documentation in English**
- Focus on:
  - correctness
  - auditability
  - clarity
  - learning by building real features

---

## Getting Started

At the current stage, you can:

- Run API and Web locally
- Manage real accounts and transactions
- Inspect balances and movements
- Reconcile accounts safely
- Run unit and integration tests
- Follow a clean, documented architecture

Distribution and end-user packaging will come after the polish phase.
