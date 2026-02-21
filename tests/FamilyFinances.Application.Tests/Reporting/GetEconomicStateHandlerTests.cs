using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetEconomicStateHandlerTests
{
    [Fact]
    public async Task HandleAsync_Computes_CurrentMonthToDate_Period_And_Returns_Result()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var asOf = new DateOnly(2026, 2, 21);
        var expected = new EconomicStateDto(
            AsOf: asOf,
            AssetsTotalCents: 2_000_000,
            LiabilitiesTotalCents: 500_000,
            NetWorthCents: 1_500_000,
            IncomeTotalCents: 120_000,
            ExpenseTotalCents: -40_000,
            PeriodNetResultCents: 80_000);

        repo.Setup(r => r.GetEconomicStateAsync(
                asOf,
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 2, 22),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetEconomicStateHandler(repo.Object);

        var result = await handler.HandleAsync(new GetEconomicStateQuery(asOf), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetEconomicStateAsync(
            asOf,
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 22),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
