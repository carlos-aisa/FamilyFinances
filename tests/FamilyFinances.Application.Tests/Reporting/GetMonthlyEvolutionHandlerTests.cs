using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetMonthlyEvolutionHandlerTests
{
    [Fact]
    public async Task HandleAsync_Delegates_To_Repository_And_Returns_Result()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var year = 2026;
        var scope = MonthlyEvolutionScope.AssetTotal;
        var expected = new MonthlyEvolutionReportDto(
            year,
            scope,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "asset-total",
                    "Asset Total",
                    null,
                    "scope",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 50_000, 50_000, 50_000)
                    })
            });

        repo.Setup(r => r.GetMonthlyEvolutionAsync(year, scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyEvolutionHandler(repo.Object);

        var result = await handler.HandleAsync(new GetMonthlyEvolutionQuery(year, scope), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyEvolutionAsync(year, scope, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Delegates_To_Repository_For_ExpenseTotal_Scope()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        const int year = 2026;
        const MonthlyEvolutionScope scope = MonthlyEvolutionScope.ExpenseTotal;
        var expected = new MonthlyEvolutionReportDto(
            year,
            scope,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "expense-total",
                    "Expense Total",
                    null,
                    "scope",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 20_000, 20_000, 20_000)
                    })
            });

        repo.Setup(r => r.GetMonthlyEvolutionAsync(year, scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetMonthlyEvolutionHandler(repo.Object);

        var result = await handler.HandleAsync(new GetMonthlyEvolutionQuery(year, scope), CancellationToken.None);

        result.Should().Be(expected);
        repo.Verify(r => r.GetMonthlyEvolutionAsync(year, scope, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Year_Is_Before_2000()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyEvolutionHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyEvolutionQuery(1999, MonthlyEvolutionScope.Accounts),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid year*");
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Year_Is_After_CurrentYear()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyEvolutionHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyEvolutionQuery(DateTime.UtcNow.Year + 1, MonthlyEvolutionScope.Accounts),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid year*");
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Scope_Is_Invalid()
    {
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var handler = new GetMonthlyEvolutionHandler(repo.Object);

        var act = () => handler.HandleAsync(
            new GetMonthlyEvolutionQuery(DateTime.UtcNow.Year, (MonthlyEvolutionScope)999),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid scope.");
    }
}
