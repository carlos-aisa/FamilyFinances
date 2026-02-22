using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetMonthlyBalanceVsGroupsChartHandlerTests
{
    [Fact]
    public async Task HandleAsync_Delegates_To_Repository_And_Returns_Result()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var year = DateTime.UtcNow.Year;
        const int month = 4;
        var expected = new MonthlyBalanceVsGroupsChartDto(
            year,
            month,
            [
                new MonthlyChartSeriesDto(
                    "asset-total",
                    "Asset Total",
                    null,
                    "scope",
                    [new MonthlyChartPointDto(1, new DateOnly(year, month, 1), 1_000)])
            ]);

        repo.Setup(r => r.GetMonthlyBalanceVsGroupsChartAsync(year, month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyBalanceVsGroupsChartHandler(repo.Object);
        var result = await handler.HandleAsync(new GetMonthlyBalanceVsGroupsChartQuery(year, month), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyBalanceVsGroupsChartAsync(year, month, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Year_Is_Invalid()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyBalanceVsGroupsChartHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyBalanceVsGroupsChartQuery(1999, 1),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid year*");
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Month_Is_Invalid()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyBalanceVsGroupsChartHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyBalanceVsGroupsChartQuery(DateTime.UtcNow.Year, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid month*");
    }
}
