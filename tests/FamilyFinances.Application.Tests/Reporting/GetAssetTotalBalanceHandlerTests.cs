using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetAssetTotalBalanceHandlerTests
{
    [Fact]
    public async Task HandleAsync_Delegates_To_Repository_And_Returns_Result()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var asOf = new DateOnly(2026, 1, 31);
        var expected = new AssetTotalBalanceDto(asOf, 123_456, 3);

        repo.Setup(r => r.GetAssetTotalBalanceAsync(asOf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetAssetTotalBalanceHandler(repo.Object);

        var result = await handler.HandleAsync(new GetAssetTotalBalanceQuery(asOf), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetAssetTotalBalanceAsync(asOf, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
