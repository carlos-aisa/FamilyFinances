# Testing Guide - FamilyFinances

## Test Structure

This project follows Clean Architecture with tests organized into three distinct levels:

### 1. Domain Tests (`FamilyFinances.Domain.Tests`)

**Purpose**: Pure unit tests that validate business logic in domain entities.

**Characteristics**:

- No external dependencies (no mocks, no database)
- Test business rules, validations, and entity behavior
- Very fast to execute

**Structure**:

```
FamilyFinances.Domain.Tests/
├── Common/
│   └── MoneyTests.cs              # Tests for Money value object
└── Ledger/
    ├── Accounts/
    │   └── AccountTests.cs         # Tests for Account entity
    ├── AccountGroups/
    │   └── AccountGroupTests.cs    # Tests for AccountGroup entity
    ├── Payees/
    │   └── PayeeTests.cs           # Tests for Payee entity
    └── Transactions/
        ├── TransactionTests.cs     # Tests for Transaction entity
        └── TransactionLinkTests.cs # Tests for TransactionLink
```

**Domain test example**:

```csharp
[Fact]
public void Create_RejectsEmptyName()
{
    var openedOn = new DateOnly(2026, 1, 2);
    
    Assert.Throws<DomainException>(() =>
        Account.Create("   ", AccountNature.Asset, AccountKind.Checking, openedOn));
}
```

### 2. Application Tests (`FamilyFinances.Application.Tests`)

**Purpose**: Unit tests for handlers and application logic using mocks.

**Characteristics**:

- Use Moq to simulate repositories and dependencies
- Verify handler behavior, validations, and orchestration
- No database required

**Structure**:

```
FamilyFinances.Application.Tests/
└── Ledger/
    ├── Accounts/
    │   ├── CreateAccountHandlerTests.cs
    │   └── ListAccountsHandlerTests.cs
    ├── AccountGroups/
    │   ├── CreateAccountGroupHandlerTests.cs
    │   └── ListAccountGroupsHandlerTests.cs
    ├── Payees/
    │   ├── CreatePayeeHandlerTests.cs
    │   └── ListPayeesHandlerTests.cs
    └── Transactions/
        ├── CreateTransactionHandlerTests.cs
        └── GetTransactionByIdHandlerTests.cs
```

**Application test example**:

```csharp
[Fact]
public async Task HandleAsync_CreatesAccount_AndPersistsIt()
{
    var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
    var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

    repo.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var handler = new CreateAccountHandler(repo.Object, uow.Object);
    
    // ... rest of test
}
```

### 3. Integration Tests (`FamilyFinances.Api.IntegrationTests`)

**Purpose**: End-to-end tests that validate the entire application including API, handlers, repositories, and database.

**Characteristics**:

- Use WebApplicationFactory to spin up the complete API
- In-memory SQLite database (clean for each test)
- Test complete system behavior
- Slower but with greater coverage

**Structure**:

```
FamilyFinances.Api.IntegrationTests/
├── AuthenticationTests.cs         # Authentication/authorization tests
├── CustomWebApplicationFactory.cs  # Test factory
├── TestAuth.cs                    # Authentication helpers
├── TestClient.cs                  # HTTP client helper
├── Helpers/
│   └── TestHelpers.cs             # Utility functions (create accounts, etc.)
├── Ledger/
│   ├── Accounts/
│   │   ├── AccountsApiTests.cs         # Accounts CRUD
│   │   └── AccountLifecycleApiTests.cs # Close/Reopen/Rename
│   ├── AccountGroups/
│   │   └── AccountGroupsApiTests.cs    # CRUD and memberships
│   ├── Payees/
│   │   ├── PayeesApiTests.cs           # Payees CRUD
│   │   └── PayeeManagementApiTests.cs  # Rename, category, delete
│   └── Transactions/
│       ├── TransactionsApiTests.cs          # Basic CRUD
│       └── TransactionsPayeesApiTests.cs    # Transactions with payees
└── Reporting/
    ├── AccountTotalsTests.cs
    ├── AccountGroupTotalsTests.cs
    ├── AccountGroupTotalsApiTests.cs
    ├── CategoryTotalsTests.cs
    └── MonthlySummaryTests.cs
```

```csharp
[Fact]
public async Task CreateAccount_ReturnsCreatedAccount()
{
    var factory = TestClient.CreateFactoryWithFreshDb();
    var client = await TestClient.CreateAuthorizedClientAsync(factory);

    var command = new CreateAccountCommand(
        Name: "Test Checking Account",
        Nature: AccountNature.Asset,
        Kind: AccountKind.Checking,
        OpenedOn: new DateOnly(2026, 1, 2)
    );

    var response = await client.PostAsJsonAsync("/api/v1/accounts", command);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

---

## Naming Conventions

### Test Classes

- **Domain**: `{EntityName}Tests.cs`
  - Example: `AccountTests.cs`, `PayeeTests.cs`
  
- **Application**: `{HandlerName}Tests.cs`
  - Example: `CreateAccountHandlerTests.cs`, `ListPayeesHandlerTests.cs`
  
- **Integration**: `{Feature}ApiTests.cs`
  - Example: `AccountsApiTests.cs`, `TransactionsPayeesApiTests.cs`

### Test Methods

Use a descriptive format that explains what is being tested:

```csharp
// Domain
[Fact]
public void Create_RejectsNullName() { }

[Fact]
public void Rename_UpdatesAccountName() { }

// Application
[Fact]
public async Task HandleAsync_CreatesAccount_AndPersistsIt() { }

[Fact]
public async Task HandleAsync_RejectsInvalidAccountNature() { }

// Integration
[Fact]
public async Task CreateAccount_ReturnsCreatedAccount() { }

[Fact]
public async Task GetAccount_Returns404_WhenAccountDoesNotExist() { }
```

---

## Running Tests

### From CLI

```bash
# Run all tests
dotnet test

# Run tests from a specific project
dotnet test tests/FamilyFinances.Domain.Tests

# Run only fast tests (Domain + Application)
dotnet test --filter "FullyQualifiedName~Domain|FullyQualifiedName~Application"

# Run only integration tests
dotnet test tests/FamilyFinances.Api.IntegrationTests

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

### From Visual Studio / Rider

- Use the Test Explorer (Ctrl+E, T in VS)
- Right-click on a test/class/project and select "Run Tests"

---

## Writing New Tests

### 1. Domain Tests

When adding new domain logic:

1. Create test class in `Domain.Tests` following folder structure of `Domain` project
2. Test all business rules and validations
3. **Don't use mocks** - just pure entity logic

**Guidelines**:

- One test class per entity
- Test both valid and invalid cases
- Use descriptive test names
- Use `Assert.Throws<DomainException>()` for validation errors
- Use FluentAssertions for complex assertions

### 2. Application Tests

When creating new handlers:

1. Create test class in `Application.Tests` following folder structure
2. Mock all dependencies (repositories, UoW, etc.)
3. Verify interactions with dependencies

**Guidelines**:

- Use `MockBehavior.Strict` to catch unexpected calls
- Verify that `SaveChangesAsync` is called when needed
- Test both success and failure cases
- Mock only interfaces, not concrete classes

### 3. Integration Tests

When implementing new API endpoints:

1. Create/update test class in `Api.IntegrationTests/Ledger/{Feature}/`
2. Use `TestClient` for factory and client creation
3. Always start with a fresh database (`CreateFactoryWithFreshDb()`)

**Guidelines**:

- Test complete flows (create, read, update, delete)
- Verify HTTP status codes
- Validate response content
- Use `TestHelpers` for common setups (creating accounts, etc.)
- Group related tests in the same class

---

## Common Helpers

### TestClient

```csharp
// Create factory with fresh database
var factory = TestClient.CreateFactoryWithFreshDb();

// Create authenticated HTTP client
var client = await TestClient.CreateAuthorizedClientAsync(factory);

// Create factory from existing database
var factory = TestClient.CreateFactoryFromDb("Data Source=test.db");
```

### TestHelpers

```csharp
// Create an account for testing
var accountId = await TestHelpers.CreateAccountAsync(
    client,
    name: "Test Account",
    nature: AccountNature.Asset,
    kind: AccountKind.Checking,
    openedOn: new DateOnly(2026, 1, 1)
);
```

### TestAuth

```csharp
// Get authorization token
var token = TestAuth.GetTestToken();
```

---

## Test Coverage Status

### ✅ Domain Tests - Complete

- [x] Account
- [x] AccountGroup
- [x] Payee
- [x] Transaction
- [x] TransactionLink
- [x] Money

### ✅ Application Tests - Handlers

**Account**:

- [x] CreateAccountHandler
- [x] ListAccountsHandler

**AccountGroup**:

- [x] CreateAccountGroupHandler
- [x] ListAccountGroupsHandler

**Payee**:

- [x] CreatePayeeHandler
- [x] ListPayeesHandler

**Transaction**:

- [x] CreateTransactionHandler
- [x] GetTransactionByIdHandler

### ✅ Integration Tests

**Authentication**:

- [x] Basic authentication

**Accounts**:

- [x] CRUD operations
- [x] Validation errors

**AccountGroups**:

- [x] CRUD operations
- [x] Memberships (add/remove accounts)

**Payees**:

- [x] CRUD operations
- [x] Validation errors

**Transactions**:

- [x] Create transaction
- [x] Get transaction by ID
- [x] Create transaction with payee
- [x] Link transactions
- [x] Validation errors

**Reporting**:

- [x] Account totals
- [x] AccountGroup totals  
- [x] Category totals
- [x] Monthly summary

---

## Best Practices

### 1. Isolation

- Each test should be independent
- Don't share state between tests
- In integration tests, always use a fresh database

### 2. Clarity

- Descriptive test names that explain what is tested
- Use Arrange-Act-Assert pattern
- One assertion per test when possible (one logical concept)

### 3. Speed

- Keep domain and application tests fast
- Reserve integration tests for complete flows
- Don't test same thing at multiple levels

### 4. Maintainability

- Use helpers to avoid duplication
- Keep tests simple and easy to understand
- Update tests when changing business logic

### 5. Coverage

- Test happy path and edge cases
- Verify validations and error handling
- Don't test framework code (EF, ASP.NET, etc.)

---

## Troubleshooting

### "The ConnectionString property has not been initialized"

- Make sure you're using `TestClient.CreateFactoryWithFreshDb()` for integration tests
- Don't reuse factories between tests

### Tests pass individually but fail in bulk

- Check for shared state between tests
- Verify that each test cleans up after itself
- In integration tests, ensure fresh database per test

### Moq exceptions in application tests

- Verify that all expected calls are set up with `Setup()`
- Use `MockBehavior.Strict` to identify unexpected calls
- Don't forget to set up `SaveChangesAsync` when modifying data

### Integration test returns 401 Unauthorized

- Make sure you're using `CreateAuthorizedClientAsync()`
- Verify that test token is being correctly generated

---

## Contributing

When adding new features:

1. **Write domain tests first** for new entities/value objects
2. **Add application tests** for new handlers
3. **Create integration tests** for new endpoints
4. **Update this README** if you add new patterns or helpers

