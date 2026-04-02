# FamilyFinances Project Constitution

## 1. Project Purpose (human-facing)
FamilyFinances is a ledger-first personal finance application for real family/personal finance management.

The product exists to:
- Manage day-to-day finances with accounting correctness.
- Preserve financial integrity through balanced transactions and explicit adjustment workflows.
- Serve as a practical, maintainable .NET architecture baseline for disciplined backend, frontend, testing, and documentation work.

## 2. High-level Architecture (as implemented)
FamilyFinances is implemented as a layered modular monolith with separate projects:

- `FamilyFinances.Domain`
  - Entities, value objects, domain rules and invariants.
- `FamilyFinances.Application`
  - Use-case handlers, DTOs, repository/service abstractions.
- `FamilyFinances.Infrastructure`
  - EF Core persistence, repository implementations, Identity/JWT wiring, migrations, startup initialization/seeding.
- `FamilyFinances.Api`
  - REST controllers, API versioning, authentication/authorization, health checks, Swagger in Development.
- `FamilyFinances.Web`
  - Blazor Web App (Interactive Server) consuming the API through HTTP clients.
- `tests/*`
  - Domain, Application, API integration, and Web test projects.

## 3. Non-negotiable Constraints

### Language
- All code, comments, and documentation MUST be written in English.

### Layering Rules and Dependency Direction
- Required architecture style: layered architecture.
- Allowed references:
  - Presentation (`Api`) -> Application
  - Application -> Domain
  - Infrastructure -> Application, Domain
  - Domain -> none
- Forbidden:
  - Domain depending on EF Core, logging frameworks, configuration, or DI frameworks.
  - Application referencing Infrastructure.
  - Circular dependencies.
- Do not introduce new architectural patterns without explicit approval.

### Persistence: EF Core + SQLite (migrations)
- EF Core is mandatory for persistence.
- Current runtime persistence is SQLite.
- Schema changes MUST be managed via EF Core migrations (no manual schema edits).
- Migration path must remain valid and testable.
- Raw SQL is allowed only for justified performance-critical paths and must stay in Infrastructure.

### Auth Model (confirmed by implementation)
- API authentication/authorization:
  - ASP.NET Core Identity is used for users/roles.
  - JWT Bearer authentication is used by API endpoints.
  - Authorization policies are role-based (`CanRead`, `CanWrite`).
- Web session model:
  - Web host exposes `/auth/session` endpoints.
  - Login exchanges credentials with API auth endpoint and stores JWT in an HttpOnly cookie (`ff_access_token`).

## 4. Coding Standards (summary)
- Follow explicit, readable, deterministic code over clever or implicit behavior.
- Naming conventions:
  - Classes/records/enums/methods/properties: PascalCase.
  - Variables/parameters: camelCase.
  - Interfaces: `I` prefix.
  - Async methods: `Async` suffix.
- Async/await must be end-to-end; `.Result` and `.Wait` are forbidden.
- Enforce validation at application boundaries and enforce invariants in domain constructors/factories.
- Never swallow exceptions; add context or rethrow.
- Keep responsibilities cohesive (SOLID), avoid unnecessary abstractions, avoid speculative refactors.
- Repositories:
  - One repository per aggregate root.
  - Must not expose `IQueryable`.
- Do not log secrets, tokens, passwords, or sensitive personal data.

## 5. Testing Standards (summary)
- Testing is mandatory for all logic/behavior changes.
- Test categories must be clear and separated: Unit, Integration, E2E (when applicable).
- Unit tests:
  - Focus on business logic.
  - No DB/filesystem/network.
  - External dependencies mocked; domain logic not mocked.
- Integration tests:
  - Must use real relational providers (SQLite or PostgreSQL).
  - EF Core InMemory provider is forbidden for integration tests.
  - Must validate persistence, constraints, query correctness, and migration application when relevant.
  - Tests must be deterministic, isolated, and order-independent.
- EF-specific:
  - SQLite in-memory tests require a shared open connection fixture.
  - Use `Migrate()` when testing migration path; use `EnsureCreated()` only for lightweight mapping scenarios.
- Coverage targets from standards:
  - 70% overall minimum.
  - 90% minimum for critical business paths.

## 6. Documentation Rules (summary)
- Documentation is part of the deliverable, not optional.
- Any change affecting behavior, APIs, data model, migrations, or configuration MUST update documentation.
- Documentation must be clear, explicit, consistent, and implementation-aligned.
- Do not assume docs are current; verify and update proactively.
- For OpenSpec change workflows (`openspec/changes/<change-name>/`), significant implementation-time changes must be reflected in `proposal.md`, `design.md`, and `tasks.md`.

## 7. OpenAPI / API documentation rule
- Any API contract change (new endpoint, request/response changes, status-code changes, behavior changes) MUST update the OpenAPI specification.
- Follow repository API documentation rules referenced by `AGENTS.md` (`OPENAPI-DOC.md`) and keep API specification artifacts in sync (for example `api-spec.yml` when used).
- Skipping or partially updating API documentation is forbidden.

## 8. Git workflow and Conventional Commits
- Use a feature branch per change.
- Pull requests are mandatory.
- Direct commits to `main` are forbidden.
- Keep scope focused; do not mix unrelated refactors with feature/fix work.
- Conventional Commit types in use:
  - `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.

## 9. Definition of Done (pragmatic checklist)
A change is done only when all items below are true:

- Code compiles and solution builds successfully.
- Applicable tests are implemented and passing (`dotnet test`).
- Architecture boundaries and dependency rules remain valid.
- Validation and error handling follow project standards.
- If persistence changed:
  - EF Core migrations are added/updated as required.
  - Migration path and data access behavior are tested.
- If API contracts changed:
  - OpenAPI/API documentation is updated.
- Documentation impacted by the change is updated in English.
- No secrets/sensitive data are introduced in code or logs.
- Scope is limited to requested behavior; no speculative or unrelated changes remain.

## 10. Optional OpenSpec + GStack Workflow Integration
- OpenSpec remains the source of truth for artifact governance (`proposal/specs/design/tasks`).
- GStack integration is optional and policy-driven via `.codex/opsx-gstack-policy.json`.
- If policy is missing or invalid, baseline OpenSpec behavior must continue.
- Release/deploy gstack skills are forbidden inside `opsx:*` orchestration.
- See `openspec/OPSX_GSTACK_INTEGRATION.md` for activation, modes, mappings, evidence, and troubleshooting.
