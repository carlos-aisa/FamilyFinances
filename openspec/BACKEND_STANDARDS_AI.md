# BACKEND_STANDARDS_AI.md

## Purpose

This document defines the minimum mandatory backend rules that the AI must follow when generating or modifying code in this repository.

If something is not explicitly allowed here or in BACKEND_STANDARDS.md, the AI must ask for clarification.

---

## Technology Constraints

- Language: C#
- Runtime: .NET (LTS)
- ORM: Entity Framework Core
- Databases: SQLite and PostgreSQL only
- Dependency Injection: Microsoft.Extensions.DependencyInjection

---

## Architecture Rules

- Architecture style: Layered Architecture (mandatory).
- Layers:
  - Domain
  - Application
  - Infrastructure
  - Presentation

- Dependency direction (strict):
  - Presentation → Application
  - Application → Domain
  - Infrastructure → Application, Domain
  - Domain → nothing

- Forbidden:
  - Domain referencing frameworks, EF Core, logging, or configuration
  - Application referencing Infrastructure
  - Circular dependencies

---

## Coding Rules

- Naming:
  - Classes, records, enums: PascalCase
  - Methods: PascalCase
  - Variables and parameters: camelCase
  - Interfaces: prefix I
  - Async methods must end with Async

- Async:
  - async/await end-to-end
  - .Result and .Wait are forbidden

- Code style:
  - Explicit code over clever code
  - No magic values
  - One responsibility per class

---

## Error Handling

- Never swallow exceptions
- Business rules → custom exceptions
- Infrastructure errors → logged and wrapped
- Catch blocks must add context or rethrow

---

## Validation

- Validate all external input
- Validation occurs at application boundaries
- Domain invariants enforced in constructors or factories
- Client-side validation must never be trusted

---

## API Rules

- REST-style APIs only
- No verbs in URLs
- Use DTOs for requests and responses
- Domain entities must never be exposed
- Standard error response format:

```json
{
  "errorCode": "ERROR_CODE",
  "message": "Description"
}
```

## Database Rules

- EF Core is mandatory
- Raw SQL allowed only when explicitly justified
- One repository per aggregate root
- Repositories must not expose IQueryable
- Schema changes via EF Core migrations only

## Testing Rules

- Unit tests:
  - Business logic only
  - No database or filesystem access
- Integration tests:
  - Real database
  - Deterministic data
- Do not mock domain logic

## Logging Rules

- Use ILogger<T> only
- Never log secrets or sensitive data
- Use appropriate log levels

## Performance Rules

- Avoid N+1 queries
- Use projections
- Use async I/O
- Dispose resources correctly

## Security Rules

- Never trust external input
- Never store secrets in code
- Use environment-based configuration
- Follow least privilege principle

## Change Scope

- Modify only files required for the task
- Do not refactor unrelated code
- Do not introduce new patterns or abstractions

## Decision Rule

If any requirement is unclear, missing, or conflicts with existing code or standards, the AI must stop and ask for clarification.
Do not guess.
