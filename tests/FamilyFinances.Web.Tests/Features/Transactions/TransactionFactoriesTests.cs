using FamilyFinances.Web.Features.Transactions;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Transactions;

public sealed class TransactionFactoriesTests
{
    [Fact]
    public void ExpenseFactory_BuildsExpectedSplits_AndPayee()
    {
        var fromAccount = Guid.NewGuid();
        var expenseAccount = Guid.NewGuid();
        var payee = Guid.NewGuid();

        var request = ExpenseTransactionFactory.Build(
            bookedOn: new DateOnly(2026, 4, 18),
            description: "Groceries",
            fromAccountId: fromAccount,
            expenseAccountId: expenseAccount,
            amount: 12.345m,
            payeeId: payee,
            memo: "weekly");

        request.PayeeId.Should().Be(payee);
        request.Splits.Should().HaveCount(2);
        request.Splits[0].AccountId.Should().Be(fromAccount);
        request.Splits[0].AmountCents.Should().Be(-1235);
        request.Splits[1].AccountId.Should().Be(expenseAccount);
        request.Splits[1].AmountCents.Should().Be(1235);
        request.Splits[1].Memo.Should().Be("weekly");
    }

    [Fact]
    public void IncomeFactory_BuildsExpectedSplits_AndPayee()
    {
        var incomeAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();
        var payee = Guid.NewGuid();

        var request = IncomeTransactionFactory.Build(
            bookedOn: new DateOnly(2026, 4, 18),
            description: "Salary",
            incomeAccountId: incomeAccount,
            destinationAccountId: destinationAccount,
            amount: 1000.005m,
            payeeId: payee,
            memo: "monthly");

        request.PayeeId.Should().Be(payee);
        request.Splits.Should().HaveCount(2);
        request.Splits[0].AccountId.Should().Be(incomeAccount);
        request.Splits[0].AmountCents.Should().Be(-100001);
        request.Splits[1].AccountId.Should().Be(destinationAccount);
        request.Splits[1].AmountCents.Should().Be(100001);
        request.Splits[1].Memo.Should().Be("monthly");
    }

    [Fact]
    public void TransferFactory_BuildsExpectedSplits_WithoutPayee()
    {
        var fromAccount = Guid.NewGuid();
        var toAccount = Guid.NewGuid();

        var request = TransferTransactionFactory.Build(
            bookedOn: new DateOnly(2026, 4, 18),
            description: "Transfer",
            fromAccountId: fromAccount,
            toAccountId: toAccount,
            amount: 250.10m,
            memo: "move funds");

        request.PayeeId.Should().BeNull();
        request.RelatedTransactionId.Should().BeNull();
        request.Splits.Should().HaveCount(2);
        request.Splits[0].AccountId.Should().Be(fromAccount);
        request.Splits[0].AmountCents.Should().Be(-25010);
        request.Splits[1].AccountId.Should().Be(toAccount);
        request.Splits[1].AmountCents.Should().Be(25010);
        request.Splits[1].Memo.Should().Be("move funds");
    }

    [Fact]
    public void RefundFactory_BuildsExpectedSplits_AndRelatedTransaction()
    {
        var expenseAccount = Guid.NewGuid();
        var destinationAccount = Guid.NewGuid();
        var payee = Guid.NewGuid();
        var related = Guid.NewGuid();

        var request = RefundTransactionFactory.Build(
            bookedOn: new DateOnly(2026, 4, 18),
            description: "Refund",
            expenseAccountId: expenseAccount,
            destinationAccountId: destinationAccount,
            amount: 9.995m,
            payeeId: payee,
            memo: "refund memo",
            relatedTransactionId: related);

        request.PayeeId.Should().Be(payee);
        request.RelatedTransactionId.Should().Be(related);
        request.Splits.Should().HaveCount(2);
        request.Splits[0].AccountId.Should().Be(expenseAccount);
        request.Splits[0].AmountCents.Should().Be(-1000);
        request.Splits[1].AccountId.Should().Be(destinationAccount);
        request.Splits[1].AmountCents.Should().Be(1000);
        request.Splits[1].Memo.Should().Be("refund memo");
    }
}
