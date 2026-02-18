using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class CreateTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesBalancedTransaction_AndPersistsIt()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var payeeRepo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var linksRepo = new Mock<ITransactionLinkRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateTransactionHandler(
            repo.Object,
            payeeRepo.Object,
            linksRepo.Object,
            fiscalYearGuard.Object,
            uow.Object);

        var bankId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();

        var cmd = new CreateTransactionRequest(
            BookedOn: new DateOnly(2026, 1, 2),
            Description: "Groceries",
            Splits: new List<TransactionSplitInput>
            {
                new(bankId, -5000, "Payment"),
                new(expenseId, 5000, "Expense")
            },
            PayeeId: null);

        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.BookedOn.Should().Be(new DateOnly(2026, 1, 2));
        result.Description.Should().Be("Groceries");
        result.Splits.Should().HaveCount(2);
        result.Splits.Sum(s => s.Amount).Should().Be(0);

        repo.Verify(r => r.AddAsync(It.Is<Transaction>(t =>
            t.Description == "Groceries" &&
            t.BookedOn == new DateOnly(2026, 1, 2) &&
            t.Splits.Count == 2 &&
            t.Splits.Sum(x => x.Amount.Cents) == 0), It.IsAny<CancellationToken>()), Times.Once);

        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        fiscalYearGuard.Verify(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenUnbalanced()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var payeeRepo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var linksRepo = new Mock<ITransactionLinkRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateTransactionHandler(
            repo.Object,
            payeeRepo.Object,
            linksRepo.Object,
            fiscalYearGuard.Object,
            uow.Object);

        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        var cmd = new CreateTransactionRequest(
            BookedOn: new DateOnly(2026, 1, 2),
            Description: "Bad Tx",
            Splits: new List<TransactionSplitInput>
            {
                new(a, 1000, null),
                new(b, 200, null) // sum != 0
            },
            PayeeId: null);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenFiscalYearIsClosed()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var payeeRepo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var linksRepo = new Mock<ITransactionLinkRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Year 2025 is closed. Reopen the year to modify movements."));

        var handler = new CreateTransactionHandler(
            repo.Object,
            payeeRepo.Object,
            linksRepo.Object,
            fiscalYearGuard.Object,
            uow.Object);

        var cmd = new CreateTransactionRequest(
            BookedOn: new DateOnly(2025, 2, 1),
            Description: "Blocked",
            Splits: new List<TransactionSplitInput>
            {
                new(Guid.NewGuid(), -500, null),
                new(Guid.NewGuid(), 500, null)
            },
            PayeeId: null);

        var act = () => handler.HandleAsync(cmd, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");

        repo.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
