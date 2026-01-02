using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger;
using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger;

public sealed class GetTransactionByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsNull_WhenNotFound()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var handler = new GetTransactionByIdHandler(repo.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();

        repo.Verify(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ReturnsDto_WhenFound()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var bank = AccountId.New();
        var expense = AccountId.New();

        var tx = Transaction.Create(
            new DateOnly(2026, 1, 2),
            "Groceries",
            new[]
            {
                TransactionSplit.Create(bank, new Money(-5000), "Payment"),
                TransactionSplit.Create(expense, new Money(5000), "Expense")
            });

        repo.Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tx);

        var handler = new GetTransactionByIdHandler(repo.Object);

        var result = await handler.HandleAsync(tx.Id.Value, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(tx.Id.Value);
        result.Splits.Should().HaveCount(2);
        result.Splits.Sum(s => s.AmountCents).Should().Be(0);

        repo.Verify(r => r.GetByIdAsync(It.Is<TransactionId>(x => x.Value == tx.Id.Value), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
