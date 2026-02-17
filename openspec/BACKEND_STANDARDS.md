# Backend Development Standards (.NET / C#)

## 1. Purpose and Scope

This document defines mandatory backend development standards for projects built with .NET and C#, targeting desktop and mobile applications, using SQLite and/or PostgreSQL.

This document exists to:

- Prevent architectural and implementation improvisation.
- Ensure consistency across all backend components.
- Serve as a single source of truth for human developers and AI assistants.
- Define clear boundaries for responsibilities, patterns, and decisions.

Any AI-assisted development must strictly follow this document.  
If a requirement is not explicitly defined here, the AI must ask for clarification and must not assume or invent behavior.

---

## 2. Technology Stack

### 2.1 Core Technologies

- Programming Language: C# (latest compatible LTS version).
- Runtime: .NET (LTS versions preferred).
- Application type: backend services supporting desktop and mobile applications.
- Dependency Injection: Microsoft.Extensions.DependencyInjection.
- Configuration: Microsoft.Extensions.Configuration.

### 2.2 Database & ORM

- Supported databases:
  - SQLite (embedded or local scenarios).
  - PostgreSQL (client-server scenarios).
- ORM:
  - Entity Framework Core.
- Raw SQL:
  - Allowed only for performance-critical paths.
  - Must be encapsulated inside Infrastructure layer.
  - Must be documented and justified.

### 2.3 Testing Frameworks

- Unit testing framework: xUnit.
- Mocking framework: Moq or NSubstitute.
- Assertion library: FluentAssertions.

### 2.4 Development Tools

- IDE: Visual Studio or JetBrains Rider.
- Code formatting: .editorconfig (mandatory).
- Static analysis: built-in .NET analyzers.
- CI compatibility is mandatory.

---

## 3. Architectural Guidelines

### 3.1 Architectural Style

The backend must follow a layered architecture.  
Alternative architectures (Clean Architecture, Hexagonal) are not allowed unless explicitly stated in the project documentation.

### 3.2 Layered Architecture Rules

The following layers are mandatory:

- Domain
  - Entities.
  - Value Objects.
  - Domain rules and invariants.
  - No framework or infrastructure dependencies.

- Application
  - Use cases.
  - Application services.
  - DTOs.
  - Interfaces for repositories and external services.

- Infrastructure
  - Entity Framework Core.
  - Database context.
  - Repository implementations.
  - External service integrations.

- Presentation
  - API controllers or service endpoints.
  - Input/output mapping.
  - No business logic.

### 3.3 Project Structure

Each layer must be implemented as a separate project.

Allowed references:

- Presentation → Application
- Application → Domain
- Infrastructure → Application, Domain

Forbidden references:

- Domain → any other layer
- Application → Infrastructure
- Presentation → Infrastructure

### 3.4 Dependency Rules

- Domain must not reference:
  - EF Core
  - Logging frameworks
  - Configuration
  - Dependency Injection
- Infrastructure must implement interfaces defined in Application.
- Dependency direction must always point inward.

---

## 4. Design Principles

### 4.1 SOLID Principles

All code must comply with SOLID principles:

- One responsibility per class.
- Interfaces must be small and cohesive.
- Dependencies must be injected, never instantiated directly.

### 4.2 DRY Principle

- Code duplication is allowed only if it improves clarity.
- Shared logic must be extracted to a single location.
- Copy-paste reuse is forbidden.

### 4.3 Explicitness over Cleverness

- Code must be readable and explicit.
- Avoid implicit behavior, magic values, and overly compact logic.
- Favor clarity over brevity.

---

## 5. Coding Standards

### 5.1 Language and Naming Conventions

- Classes, records, enums: PascalCase.
- Methods: PascalCase.
- Properties: PascalCase.
- Local variables and parameters: camelCase.
- Async methods must end with the suffix Async.
- Interfaces must start with the prefix I.

### 5.2 Error Handling Strategy

- Exceptions must never be swallowed.
- Business rule violations:
  - Use custom domain or application exceptions.
- Infrastructure errors:
  - Must be wrapped and logged.
- Catch blocks must always add context or rethrow.

### 5.3 Validation Patterns

- All external input must be validated at application boundaries.
- Use FluentValidation or equivalent validation libraries.
- Domain invariants must be enforced in constructors or factory methods.
- Validation must not rely on UI or client behavior.

### 5.4 Logging Standards

- Logging must use ILogger<T>.
- Log levels:
  - Information: application flow.
  - Warning: recoverable or unexpected situations.
  - Error: failures that require attention.
- Logs must be meaningful and structured.

---

## 6. API Design Standards

### 6.1 API Style and Conventions

- APIs must follow REST-style conventions.
- URLs must represent resources, not actions.
- HTTP verbs must express intent.
- Versioning must be explicit if required.

### 6.2 Request / Response Models

- Use DTOs exclusively.
- Domain entities must never be exposed.
- Requests and responses must be immutable where possible.

### 6.3 Error Response Format

All error responses must follow this format:

```json
{
  "errorCode": "ERROR_CODE",
  "message": "Human-readable description"
}

### 6.4 CQRS Usage Rules

- CQRS is optional.
- CQRS must be used only when read and write models differ significantly.
- Commands must not return domain entities.
- Queries must not modify state.

## 7. Database Standards

### 7.1 Supported Databases

SQLite:
- Assume limited concurrency.
- Avoid long-running transactions.

PostgreSQL:
- Full transactional support.
- Use connection pooling.

### 7.2 Schema Design and Normalization

- Third Normal Form is the default.
- Denormalization requires justification and documentation.
- Schema changes must be backward compatible when possible.

### 7.3 Indexing Strategies

- Index foreign keys by default.
- Composite indexes must be justified.
- Measure performance before adding indexes.

### 7.4 Migration Management

- EF Core migrations are mandatory.
- No manual schema changes are allowed.
- Migrations must be reviewed and versioned.

### 7.5 Repository and Data Access Patterns

- One repository per aggregate root.    
- Repositories must not expose IQueryable.
- Query logic must remain inside Infrastructure.

## 8. Testing Standards

### 8.1 Unit Testing Rules

- Unit tests must test business logic only.
- No database access is allowed.
- No file system access is allowed.

### 8.2 Integration Testing Rules

- Integration tests must use a real database.
- Test data must be controlled and isolated.
- Tests must be repeatable and deterministic.

### 8.3 Test Coverage Requirements

- Minimum overall coverage: 70%.
- Critical business paths: minimum 90%.
- Coverage must be enforced in CI.

### 8.4 Mocking and Test Isolation

- Infrastructure dependencies must be mocked.
- Domain logic must not be mocked.
- Mocks must reflect real behavior.

## 9. Performance Guidelines

### 9.1 Database Performance

- Avoid N+1 query patterns.
- Use projections instead of loading full entities.
- Prefer server-side filtering.

### 9.2 Async / Await Usage

- Use async/await end-to-end.
- Blocking calls (.Result, .Wait) are forbidden.
- Async methods must not mix synchronous I/O.

### 9.3 Resource Management

- Use await using when applicable.
- Dispose unmanaged resources explicitly.
- Avoid unnecessary allocations.

## 10. Security Guidelines

### 10.1 Input Validation and Sanitization

- Validate all external input.
- Never trust client-provided data.
- Protect against injection attacks.

### 10.2 Secrets and Configuration

- Secrets must never be stored in source code.
- Use environment variables or secure stores.
- Configuration must be environment-specific.

### 10.3 Dependency Injection and Lifetime Management

- Transient: lightweight stateless services.
- Scoped: application services.
- Singleton: only for stateless, thread-safe components.

### 10.4 Logging and Sensitive Data

- Never log:
  - passwords
  - tokens
  - secrets
  - personal or sensitive data
- Logs must comply with data protection rules.

## 11. Development Workflow

### 11.1 Git Workflow

- Feature branch per change.
- Pull requests are mandatory.
- Direct commits to main branch are forbidden.

### 11.2 Build and Development Scripts

- dotnet build
- dotnet test
- dotnet format 

### 11.3 Code Quality Gates

- All tests must pass.
- No compiler warnings allowed.
- Static analysis issues must be addressed.

11.4 AI Usage Rules

- AI must not:
  - invent architecture
  - change layer boundaries
  - bypass validation rules
  - introduce undocumented patterns

- AI-generated code must always be reviewed by a human.
- When in doubt, the AI must ask for clarification.
