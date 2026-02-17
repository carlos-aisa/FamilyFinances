# EF_TESTING_GUIDE_DOTNET_AI.md

## Purpose
Rules for EF Core tests in this repo. Follow strictly. If unclear, ask.

---

## Non‑Negotiable EF Rules
- Integration tests MUST use a real relational provider:
  - SQLite (shared connection) or PostgreSQL (preferred for production parity).
- EF Core InMemory provider is FORBIDDEN for integration tests.
- Each test MUST be isolated (no order dependencies).

---

## SQLite In‑Memory Rules
- Use `DataSource=:memory:` and keep ONE connection open via fixture.
- Create schema via:
  - `Database.Migrate()` when testing migrations, OR
  - `Database.EnsureCreated()` for lightweight mapping tests.

---

## PostgreSQL Rules
- Prefer Testcontainers for Postgres integration tests.
- Apply migrations in tests with `Database.Migrate()`.
- Reset state between tests (transaction rollback or truncate/Respawn). No shared dirty DB.

---

## xUnit Rules
- Use fixtures (`IAsyncLifetime`, `ICollectionFixture`) for DB setup.
- Disable parallelization if tests share DB resources.

---

## What to Test (EF)
- Persistence (SaveChanges)
- Constraints (unique, required, FK)
- Query correctness (filters, projections)
- Migrations apply successfully (`Migrate()`)

---

## Forbidden
- `UseInMemoryDatabase` for integration tests
- Sharing a DbContext across tests
- Relying on execution order
- Flaky timing-based tests

---

## EF Change Checklist
When EF code changes:
1. Determine if migrations are needed.
2. Add/verify migrations (if in scope).
3. Add integration tests for behavior + constraints + queries.
4. Ensure isolation and real provider.
5. `dotnet test` must pass.

If any item is ambiguous, ask. Do not guess.
