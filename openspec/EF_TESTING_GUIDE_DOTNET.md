# EF Core Testing Guide (.NET / xUnit) — Canonical Examples + Rules

This document complements `TESTING_STANDARDS_DOTNET.md` with **EF Core–specific testing rules** and **canonical xUnit examples**.

---

## 1. EF Core Testing Rules (Non‑Negotiable)

### 1.1 Use a Real Database for Integration Tests
- **Integration tests MUST use a real relational database.**
- Allowed:
  - SQLite (real relational engine) for fast local runs.
  - PostgreSQL for “production-equivalent” behavior.
- **Do NOT use** `Microsoft.EntityFrameworkCore.InMemory` for integration tests (it is not relational and hides issues).

### 1.2 Prefer PostgreSQL for Production-Equivalent Behavior
Use PostgreSQL integration tests when you need to validate:
- Case sensitivity / collation differences
- Concurrency behavior
- Transactions, isolation, locking
- Complex SQL translation differences
- JSONB / advanced Postgres features (if used)

### 1.3 SQLite: Use a Shared Connection for the Test Lifetime
SQLite in-memory DB is per connection. For reliable tests:
- Use `DataSource=:memory:` with a **single open connection** kept alive for the whole test (or fixture).
- Call `EnsureCreated()` (or run migrations) after opening connection.

### 1.4 Migrations Must Be Tested
- If the project uses EF Core migrations:
  - Integration tests MUST validate that migrations can be applied.
  - Prefer `context.Database.Migrate()` (not `EnsureCreated`) when testing the migration path.

### 1.5 Each Test Must Be Isolated (No Order Dependency)
Allowed isolation strategies:
- Recreate DB per test (simplest, slower).
- Reuse DB per test class + reset state (faster), via:
  - Transaction rollback (works if everything is in same connection / DbContext lifetime).
  - Truncate tables between tests (Respawn recommended for Postgres).

### 1.6 Parallelization
- If tests share a database instance, they MUST NOT run in parallel.
- Either:
  - Disable parallelization for the collection, or
  - Create isolated DBs per test.

### 1.7 Seeding Rules
- Seed only what you need for the test.
- Use explicit builders / factories for test data.
- Avoid “global seed” that grows and makes tests hard to understand.

### 1.8 Assertions
- Prefer verifying **observable behavior**:
  - Entities persisted correctly
  - Constraints enforced
  - Queries return correct results
- Avoid asserting raw SQL strings unless you are explicitly testing query translation.

---

## 2. Recommended Libraries and Patterns

### 2.1 xUnit Fixtures
Use xUnit fixtures for expensive setup:
- `IAsyncLifetime` for async init/cleanup
- `IClassFixture<T>` or `ICollectionFixture<T>` for sharing resources

### 2.2 PostgreSQL via Testcontainers (Recommended)
For reliable, isolated Postgres tests, use `testcontainers-dotnet`.

### 2.3 ASP.NET Core API Tests
For API integration tests:
- Use `WebApplicationFactory<TEntryPoint>`
- Override DI to point to test database
- Validate endpoints with real HTTP calls

---

## 3. Canonical Example: Domain/Service Unit Test (No EF)

**Goal:** Unit test business logic without database.

```csharp
using FluentAssertions;
using Xunit;

public sealed class PriceCalculatorTests
{
    [Fact]
    public void Should_Apply_Discount_When_Customer_Is_Premium()
    {
        // Arrange
        var calc = new PriceCalculator();
        var input = new PriceInput(amount: 100m, isPremiumCustomer: true);

        // Act
        var result = calc.Calculate(input);

        // Assert
        result.FinalAmount.Should().Be(90m);
    }
}

public sealed record PriceInput(decimal Amount, bool IsPremiumCustomer);

public sealed class PriceCalculator
{
    public PriceResult Calculate(PriceInput input)
    {
        if (input.Amount < 0) throw new ArgumentOutOfRangeException(nameof(input.Amount));
        var discount = input.IsPremiumCustomer ? 0.10m : 0m;
        return new PriceResult(input.Amount * (1 - discount));
    }
}

public sealed record PriceResult(decimal FinalAmount);
```

---

## 4. Canonical Example: EF Core Integration Test with SQLite In‑Memory (Shared Connection)

**Goal:** Validate repository + mapping + relational behavior quickly.

### 4.1 DbContext and Entity (example)

```csharp
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
        });
    }
}

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; }

    public User(string email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }
}
```

### 4.2 Test Fixture (shared SQLite connection)

```csharp
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class SqliteDbFixture : IAsyncLifetime
{
    public DbConnection Connection { get; private set; } = default!;
    public DbContextOptions<AppDbContext> Options { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        await Connection.OpenAsync();

        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging(false)
            .Options;

        using var ctx = new AppDbContext(Options);
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
    }

    public AppDbContext CreateContext() => new(Options);
}

[CollectionDefinition(nameof(SqliteDbCollection))]
public sealed class SqliteDbCollection : ICollectionFixture<SqliteDbFixture> { }
```

### 4.3 Integration Test

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection(nameof(SqliteDbCollection))]
public sealed class UserRepositoryTests
{
    private readonly SqliteDbFixture _db;

    public UserRepositoryTests(SqliteDbFixture db) => _db = db;

    [Fact]
    public async Task Should_Persist_And_Query_User_By_Email()
    {
        // Arrange
        await using var ctx = _db.CreateContext();
        var user = new User("alice@example.com");
        ctx.Users.Add(user);

        // Act
        await ctx.SaveChangesAsync();
        var loaded = await ctx.Users.SingleAsync(x => x.Email == "alice@example.com");

        // Assert
        loaded.Id.Should().Be(user.Id);
        loaded.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Should_Enforce_Unique_Email()
    {
        // Arrange
        await using var ctx = _db.CreateContext();
        ctx.Users.Add(new User("dup@example.com"));
        await ctx.SaveChangesAsync();

        ctx.Users.Add(new User("dup@example.com"));

        // Act
        var act = async () => await ctx.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
```

**Notes**
- This test setup validates relational constraints.
- The unique index is enforced by SQLite.

---

## 5. Canonical Example: Apply EF Migrations in Tests

If your project uses migrations, test the migration path:

```csharp
using Microsoft.EntityFrameworkCore;

await using var ctx = new AppDbContext(options);
await ctx.Database.MigrateAsync(); // validates migrations can be applied
```

**Rule**
- Use `Migrate()` for migration testing.
- Use `EnsureCreated()` only for lightweight tests when migrations are not the target.

---

## 6. Canonical Example: PostgreSQL Integration Tests with Testcontainers

**Goal:** Validate production-equivalent behavior.

### 6.1 Package
- `DotNet.Testcontainers`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

### 6.2 Fixture

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly IContainer _container;

    public string ConnectionString { get; private set; } = default!;

    public PostgresFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_DB", "testdb")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var hostPort = _container.GetMappedPublicPort(5432);
        ConnectionString = $"Host=localhost;Port={hostPort};Database=testdb;Username=postgres;Password=postgres";
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableSensitiveDataLogging(false)
            .Options;
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
```

### 6.3 Test Example

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection(nameof(PostgresCollection))]
public sealed class PostgresUserTests
{
    private readonly PostgresFixture _pg;

    public PostgresUserTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Should_Apply_Migrations_And_Persist_User()
    {
        var options = _pg.CreateOptions();

        await using (var ctx = new AppDbContext(options))
        {
            await ctx.Database.MigrateAsync();
        }

        await using (var ctx = new AppDbContext(options))
        {
            ctx.Users.Add(new User("bob@example.com"));
            await ctx.SaveChangesAsync();

            var loaded = await ctx.Users.SingleAsync(x => x.Email == "bob@example.com");
            loaded.Email.Should().Be("bob@example.com");
        }
    }
}
```

**Rule**
- If using a shared Postgres database across tests, you must isolate data per test (transaction or cleanup).

---

## 7. State Reset Strategies (EF Core)

### 7.1 Strategy A — Recreate Database per Test (Simple)
- Create a unique database name per test.
- Apply migrations once per DB.
- Slow but safest.

### 7.2 Strategy B — Transaction Rollback (Fast; requires discipline)
- Begin transaction at test start.
- Rollback at test end.
- Works best when each test uses a single connection and does not spawn background operations.

### 7.3 Strategy C — Truncate between tests (Postgres recommended)
- Use a tool like Respawn to reset tables.
- This keeps schema but clears data.

---

## 8. Canonical Example: ASP.NET Core API Integration Test with WebApplicationFactory

**Goal:** Test real HTTP endpoints + EF Core database.

```csharp
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public sealed class UsersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsersApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_200_On_Health()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**Rule**
- For DB-backed endpoints, override DI to use test DB connection string and apply migrations at startup.

---

## 9. EF Core Anti‑Patterns (Forbidden)

- Using `UseInMemoryDatabase` for integration tests.
- Sharing one DbContext instance across multiple tests.
- Not awaiting async calls (hidden race conditions).
- Relying on test execution order.
- Asserting EF internal behavior rather than outcomes.
- Seeding a large global dataset for all tests.

---

## 10. AI Execution Checklist (EF Core Changes)

When the change involves EF Core, the AI MUST:
1. Identify if migrations are required.
2. Add/update migrations (if requested in task scope).
3. Add integration tests for:
   - persistence
   - constraints
   - query correctness
4. Ensure tests use real DB providers.
5. Ensure isolation (no cross-test interference).
6. Confirm `dotnet test` passes.

If any of these steps are ambiguous, the AI must ask for clarification.
