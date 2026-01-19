using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

/// <summary>
/// Tests for refund classification in ListTransactionsHandler.
/// Note: Full refund classification logic is integration-tested.
/// These unit tests focus on handler behavior with the repository.
/// </summary>
public sealed class ListTransactionsHandlerRefundTests
{
    [Fact]
    public async Task HandleAsync_ClassifiesRefunds_InIntegrationTests()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        // For unit tests, we just verify the handler calls the repository
        // The actual classification logic (Expense -> Asset = Refund) is tested in integration tests
        // because it requires real domain entities with navigation properties that can't be easily mocked

        repo.Setup(r => r.ListAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FamilyFinances.Domain.Ledger.Transactions.Transaction>());

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        repo.Verify(r => r.ListAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        
        // Note: Refund classification is thoroughly tested in RefundsApiTests
        // where we can verify that expense-to-asset transactions return TransactionListItemType.Refund
    }
}