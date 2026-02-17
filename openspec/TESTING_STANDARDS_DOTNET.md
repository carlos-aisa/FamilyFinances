# Testing Standards (.NET / xUnit)

## 1. Purpose and Scope

This document defines **mandatory testing standards** for all .NET projects in this repository.

Testing is not optional.  
Any code that introduces logic, behavior, or decision-making MUST be covered by tests.

These rules apply to:

- Backend code
- Shared libraries
- AI-generated code
- Human-written code

If a testing requirement is unclear, clarification MUST be requested before implementation.

---

## 2. Core Testing Stack

- Language: C#
- Runtime: .NET (LTS)
- Test framework: xUnit
- Mocking:
  - Moq or NSubstitute
- Assertions:
  - FluentAssertions
- Test runner:
  - dotnet test

No alternative testing frameworks are allowed unless explicitly approved.

---

## 3. General Testing Principles

- Tests are part of the deliverable.
- Tests must validate **behavior**, not implementation details.
- Tests must be:
  - Deterministic
  - Repeatable
  - Independent
- Flaky tests are forbidden.
- Tests must be readable and explicit.

---

## 4. Test Categories

All tests MUST clearly belong to one of the following categories:

- Unit Tests
- Integration Tests
- End-to-End Tests (if applicable)

Mixing responsibilities between test categories is forbidden.

---

## 5. Unit Testing Standards

### 5.1 Purpose

Unit tests validate **pure logic** in isolation.

### 5.2 What MUST be Unit Tested

- Domain logic
- Application services logic
- Business rules
- Validation logic
- Pure functions
- State transitions

### 5.3 What MUST NOT be Unit Tested

- Database access
- File system access
- Network calls
- EF Core behavior
- Framework internals
- Logging behavior

### 5.4 Isolation Rules

- External dependencies MUST be mocked.
- Domain logic MUST NOT be mocked.
- Static dependencies MUST be avoided or abstracted.
- Mocks must represent realistic behavior, including failures.

### 5.5 Structure and Style

- One test class per production class.
- Test class name: `<ClassName>Tests`
- Use Arrange / Act / Assert structure.
- Prefer explicit setup over shared mutable fixtures.

---

## 6. Integration Testing Standards

### 6.1 Purpose

Integration tests validate interaction between components and real infrastructure.

### 6.2 What MUST be Integration Tested

- Repository implementations
- EF Core mappings
- Database migrations
- Application services using real infrastructure
- API endpoints (controllers, minimal APIs)

### 6.3 Infrastructure Rules

- Integration tests MUST use:
  - A real database (SQLite or PostgreSQL)
- In-memory providers are NOT allowed for integration tests unless explicitly approved.
- Test databases must be isolated from production.

### 6.4 Data Management Rules

- Test data must be deterministic.
- Tests must clean up their data.
- Tests must not depend on execution order.
- Parallel execution must be supported or explicitly disabled.

---

## 7. End-to-End Testing (Optional)

### 7.1 Purpose

End-to-End tests validate **complete user or system flows**.

### 7.2 Scope Rules

- E2E tests must cover:
  - Critical workflows
  - Cross-component behavior
- E2E tests must NOT duplicate unit or integration test coverage.

---

## 8. Mocking Standards

- Use mocks only for:
  - External services
  - Infrastructure dependencies
- Do NOT mock:
  - Domain entities
  - Value objects
  - Pure functions
- Avoid over-mocking.
- Mock setups must be explicit and minimal.

---

## 9. Naming and Organization

- Test folder structure must mirror production structure.
- Test method names must describe behavior.

Examples:

```csharp
Should_Create_Order_When_Input_Is_Valid
Should_Throw_Exception_When_Rule_Is_Violated
Should_Return_NotFound_When_Entity_Does_Not_Exist
```

---

## 10. Coverage Requirements

- Minimum overall coverage: 70%
- Critical business logic: minimum 90%
- Coverage is an indicator, not the goal.
- Meaningless tests written only to increase coverage are forbidden.

---

## 11. Performance and Execution Rules

- Unit tests must be fast.
- Long-running tests must be categorized as integration or E2E.
- Unit tests must not depend on environment configuration.
- Tests must run reliably in CI environments.

---

## 12. CI Enforcement

- All tests MUST run in CI.
- Builds MUST fail if any test fails.
- Coverage thresholds MUST be enforced automatically.
- Test results must be visible and auditable.

---

## 13. AI-Specific Rules

- AI-generated code MUST include tests.
- AI MUST NOT:
  - Skip tests due to complexity
  - Write trivial or meaningless tests
  - Mock domain logic
- If the AI is unsure how to test a behavior, it MUST ask for clarification.

Testing is a first-class requirement, not an afterthought.
