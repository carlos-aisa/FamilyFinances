using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Create;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger;

public sealed class CreateTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesBalancedTransaction_AndPersistsIt()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateTransactionHandler(repo.Object, uow.Object);

        var bankId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();

        var cmd = new CreateTransactionCommand(
            BookedOn: new DateOnly(2026, 1, 2),
            Description: "Groceries",
            Splits: new List<TransactionSplitInput>
            {
                new(bankId, -5000, "Payment"),
                new(expenseId, 5000, "Expense")
            });

        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.BookedOn.Should().Be(new DateOnly(2026, 1, 2));
        result.Description.Should().Be("Groceries");
        result.Splits.Should().HaveCount(2);
        result.Splits.Sum(s => s.AmountCents).Should().Be(0);

        repo.Verify(r => r.AddAsync(It.Is<Transaction>(t =>
            t.Description == "Groceries" &&
            t.BookedOn == new DateOnly(2026, 1, 2) &&
            t.Splits.Count == 2 &&
            t.Splits.Sum(x => x.Amount.Cents) == 0), It.IsAny<CancellationToken>()), Times.Once);

        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenUnbalanced()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var handler = new CreateTransactionHandler(repo.Object, uow.Object);

        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        var cmd = new CreateTransactionCommand(
            BookedOn: new DateOnly(2026, 1, 2),
            Description: "Bad Tx",
            Splits: new List<TransactionSplitInput>
            {
                new(a, 1000, null),
                new(b, 200, null) // sum != 0
            });

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
