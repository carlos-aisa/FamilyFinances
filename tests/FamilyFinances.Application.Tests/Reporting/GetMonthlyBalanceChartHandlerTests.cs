using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetMonthlyBalanceChartHandlerTests
{
    [Fact]
    public async Task HandleAsync_Delegates_To_Repository_And_Returns_Result()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var year = DateTime.UtcNow.Year;
        const int month = 2;
        var expected = new MonthlyBalanceChartDto(
            year,
            month,
            [new MonthlyChartPointDto(1, new DateOnly(year, month, 1), 1_000)]);

        repo.Setup(r => r.GetMonthlyBalanceChartAsync(year, month, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyBalanceChartHandler(repo.Object);
        var result = await handler.HandleAsync(new GetMonthlyBalanceChartQuery(year, month), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyBalanceChartAsync(year, month, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Passes_AccountId_When_Provided()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var year = DateTime.UtcNow.Year;
        const int month = 2;
        var accountId = Guid.NewGuid();
        var expected = new MonthlyBalanceChartDto(
            year,
            month,
            [new MonthlyChartPointDto(1, new DateOnly(year, month, 1), 1_000)]);

        repo.Setup(r => r.GetMonthlyBalanceChartAsync(year, month, accountId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyBalanceChartHandler(repo.Object);
        var result = await handler.HandleAsync(new GetMonthlyBalanceChartQuery(year, month, accountId), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyBalanceChartAsync(year, month, accountId, null, null, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Passes_PayeeId_And_Nature_When_Provided()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var year = DateTime.UtcNow.Year;
        const int month = 2;
        var payeeId = Guid.NewGuid();
        var expected = new MonthlyBalanceChartDto(
            year,
            month,
            [new MonthlyChartPointDto(1, new DateOnly(year, month, 1), 1_000)]);

        repo.Setup(r => r.GetMonthlyBalanceChartAsync(year, month, null, payeeId, AccountNature.Income, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyBalanceChartHandler(repo.Object);
        var result = await handler.HandleAsync(
            new GetMonthlyBalanceChartQuery(year, month, AccountId: null, PayeeId: payeeId, Nature: AccountNature.Income),
            CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyBalanceChartAsync(year, month, null, payeeId, AccountNature.Income, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Year_Is_Invalid()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyBalanceChartHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyBalanceChartQuery(DateTime.UtcNow.Year + 1, 1),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid year*");
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Month_Is_Invalid()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyBalanceChartHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyBalanceChartQuery(DateTime.UtcNow.Year, 13),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid month*");
    }

    [Fact]
    public async Task HandleAsync_Throws_When_AccountId_And_Nature_Are_Both_Provided()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyBalanceChartHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyBalanceChartQuery(DateTime.UtcNow.Year, 2, Guid.NewGuid(), Nature: AccountNature.Asset),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Specify either 'accountId' or 'nature'*");
    }
}
