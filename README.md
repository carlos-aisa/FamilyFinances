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
- `v0.6.7` Windows ZIP distribution ✅
  - Self-contained win-x64 executable (no .NET installation required)
  - Portable SQLite database & logs
  - One-click launcher with health checks
  - Automated builds via GitHub Actions

### In progress / planned
- `v0.7.0` Multi-split transactions ✅
  - Support for transactions with 3+ splits
  - Mortgage payment preset widget on dashboard
  - Multi-split editor with live validation
  - Exclusive widget collapse behavior
  - Date picker for backdating payments
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

### For End Users (Windows)

**Download the latest Windows ZIP distribution:**
1. Go to the [Releases page](https://github.com/carlos-aisa/FamilyFinances/releases)
2. Download `FamilyFinances-vX.X.X-win-x64.zip`
3. Extract the ZIP folder
4. Double-click `Start FamilyFinances.bat`
5. Use the app in your browser at `http://localhost:5019`

**No .NET installation required!** Everything is self-contained.

See the included `README.txt` for troubleshooting and detailed instructions.

### For Developers

At the current stage, you can:

- Clone the repository
- Run API and Web locally (requires .NET 9.0 SDK)
- Build the Windows distribution using `build-windows-dist.ps1`
- Manage real accounts and transactions
- Inspect balances and movements
- Reconcile accounts safely
- Run unit and integration tests
- Follow a clean, documented architecture

**API:** `http://localhost:5084`  
**Web:** `http://localhost:5019`

See [Windows Distribution Build Guide](docs/windows-distribution-build.md) for packaging details.
