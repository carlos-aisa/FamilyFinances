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
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new ListLatestExpenseMovementsHandler(repository.Object);

        // Act
        var result = await handler.HandleAsync(null, null, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        repository.VerifyAll();
    }

    [Fact]
    public async Task HandleAsync_PassesRequestedPeriodToRepository()
    {
        var fromInclusive = new DateOnly(2026, 2, 1);
        var toExclusive = new DateOnly(2026, 3, 1);
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        repository
            .Setup(repo => repo.ListLatestExpensesAsync(
                ListLatestExpenseMovementsHandler.LatestExpensesCount,
                fromInclusive,
                toExclusive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new ListLatestExpenseMovementsHandler(repository.Object);

        var result = await handler.HandleAsync(fromInclusive, toExclusive, CancellationToken.None);

        result.Should().BeEmpty();
        repository.VerifyAll();
    }
}
