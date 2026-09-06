using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class ListLatestExpenseMovementsHandlerTests
{
    [Fact]
    public async Task HandleAsync_RequestsExactlySixLatestExpenseTransactions()
    {
        // Arrange
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        repository
            .Setup(repo => repo.ListLatestExpensesAsync(
                ListLatestExpenseMovementsHandler.LatestExpensesCount,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new ListLatestExpenseMovementsHandler(repository.Object);

        // Act
        var result = await handler.HandleAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        repository.VerifyAll();
    }
}
