using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class SearchExpensesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenQueryTooShort()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync("a", null, 20, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenQueryIsWhitespace()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync("   ", null, 20, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CallsRepository_WhenQueryIsValid()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        
        var expectedResults = new List<ExpenseSearchResultDto>
        {
            new(Guid.NewGuid(), "Test Expense", new DateOnly(2026, 1, 15), "Amazon", 50.00m, "Groceries")
        };

        repo.Setup(r => r.SearchExpensesAsync("Amazon", null, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync("Amazon", null, 20, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResults);
        repo.Verify(r => r.SearchExpensesAsync("Amazon", null, 20, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CapsLimitAt50()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        
        repo.Setup(r => r.SearchExpensesAsync("test", null, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpenseSearchResultDto>());

        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        await handler.HandleAsync("test", null, 100, CancellationToken.None);

        // Assert
        repo.Verify(r => r.SearchExpensesAsync("test", null, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TrimsQuery()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        
        repo.Setup(r => r.SearchExpensesAsync("Amazon", null, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpenseSearchResultDto>());

        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        await handler.HandleAsync("  Amazon  ", null, 20, CancellationToken.None);

        // Assert
        repo.Verify(r => r.SearchExpensesAsync("Amazon", null, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesExpenseAccountId_WhenProvided()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var accountId = Guid.NewGuid();
        
        repo.Setup(r => r.SearchExpensesAsync("test", accountId, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExpenseSearchResultDto>());

        var handler = new SearchExpensesHandler(repo.Object);

        // Act
        await handler.HandleAsync("test", accountId, 20, CancellationToken.None);

        // Assert
        repo.Verify(r => r.SearchExpensesAsync("test", accountId, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}