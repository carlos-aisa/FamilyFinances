using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

/// <summary>
/// Unit tests for ListTransactionsHandler.
/// Note: The transaction classification logic (Expense/Income/Transfer/Other) is complex and depends on
/// sealed domain entities with navigation properties that cannot be easily mocked.
/// This logic is thoroughly tested via integration tests in FamilyFinances.Api.IntegrationTests.
/// These unit tests focus on verifying the handler's interaction with the repository.
/// </summary>
public sealed class ListTransactionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoTransactions()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        repo.Setup(r => r.ListAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transaction>());

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        repo.Verify(r => r.ListAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_RespectsLimitParameter()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        repo.Setup(r => r.ListAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transaction>());

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        await handler.HandleAsync(25, CancellationToken.None);

        // Assert
        repo.Verify(r => r.ListAsync(25, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task HandleAsync_PassesCorrectLimitToRepository(int take)
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        repo.Setup(r => r.ListAsync(take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transaction>());

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        await handler.HandleAsync(take, CancellationToken.None);

        // Assert
        repo.Verify(r => r.ListAsync(take, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryOnce()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        repo.Setup(r => r.ListAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Transaction>());

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        await handler.HandleAsync(10, CancellationToken.None);

        // Assert
        repo.Verify(r => r.ListAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
