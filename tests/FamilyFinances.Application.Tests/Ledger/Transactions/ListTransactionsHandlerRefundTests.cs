using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

/// <summary>
/// Tests for refund classification in ListTransactionsHandler.
/// Note: Full classification matrix is integration-tested.
/// These unit tests verify DTO mapping and key behavior contracts.
/// </summary>
public sealed class ListTransactionsHandlerRefundTests
{
    [Fact]
    public async Task HandleAsync_MapsRefund_WithPayeeName_AndCleanSubheadline()
    {
        // Arrange
        var expenseAccount = Account.Create("Groceries", AccountNature.Expense, AccountKind.ExpenseCategory, new DateOnly(2026, 1, 1));
        var assetAccount = Account.Create("Main Bank", AccountNature.Asset, AccountKind.Checking, new DateOnly(2026, 1, 1));
        var payee = Payee.Create("Supermarket");

        var splits = new[]
        {
            TransactionSplit.Create(expenseAccount.Id, Money.FromEuros(-15.75m)),
            TransactionSplit.Create(assetAccount.Id, Money.FromEuros(15.75m))
        };

        SetSplitAccount(splits[0], expenseAccount);
        SetSplitAccount(splits[1], assetAccount);

        var transaction = Transaction.Create(
            bookedOn: new DateOnly(2026, 3, 1),
            description: "Refund of overcharge",
            splits: splits,
            payeeId: payee.Id);

        SetTransactionPayee(transaction, payee);

        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ListAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([transaction]);

        var handler = new ListTransactionsHandler(repo.Object);

        // Act
        var result = await handler.HandleAsync(10, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        var item = result.Single();

        item.Type.Should().Be(TransactionListItemType.Refund);
        item.Headline.Should().Be("Groceries");
        item.Subheadline.Should().Be("Refund of overcharge");
        item.PayeeName.Should().Be("Supermarket");
        item.Amount.Should().Be(15.75m);

        repo.Verify(r => r.ListAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    private static void SetSplitAccount(TransactionSplit split, Account account)
    {
        var field = typeof(TransactionSplit).GetField("<Account>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to find backing field for TransactionSplit.Account.");
        field.SetValue(split, account);
    }

    private static void SetTransactionPayee(Transaction transaction, Payee payee)
    {
        var field = typeof(Transaction).GetField("<Payee>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to find backing field for Transaction.Payee.");
        field.SetValue(transaction, payee);
    }
}
