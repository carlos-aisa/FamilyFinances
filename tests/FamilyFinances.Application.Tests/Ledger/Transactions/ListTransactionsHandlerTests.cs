using FamilyFinances.Application.Ledger.Transactions.Abstractions;
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

    [Fact]
    public async Task HandleAsync_Maps_PayeeName_And_Uses_Description_As_Subheadline()
    {
        // Arrange
        var expenseAccount = Account.Create("Groceries", AccountNature.Expense, AccountKind.ExpenseCategory, new DateOnly(2026, 1, 1));
        var assetAccount = Account.Create("Main Bank", AccountNature.Asset, AccountKind.Checking, new DateOnly(2026, 1, 1));
        var payee = Payee.Create("Supermarket");

        var splits = new[]
        {
            TransactionSplit.Create(assetAccount.Id, Money.FromEuros(-42.5m)),
            TransactionSplit.Create(expenseAccount.Id, Money.FromEuros(42.5m))
        };

        SetSplitAccount(splits[0], assetAccount);
        SetSplitAccount(splits[1], expenseAccount);

        var transaction = Transaction.Create(
            bookedOn: new DateOnly(2026, 2, 15),
            description: "Weekly food run",
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
        item.Type.Should().Be(FamilyFinances.Application.Ledger.Transactions.Dtos.TransactionListItemType.Expense);
        item.Headline.Should().Be("Groceries");
        item.Subheadline.Should().Be("Weekly food run");
        item.PayeeName.Should().Be("Supermarket");
        item.Amount.Should().Be(42.5m);
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
