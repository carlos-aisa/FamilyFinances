using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FluentAssertions;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class MonthlyChartBucketBuilderTests
{
    [Fact]
    public void BuildDailyEndBalancePoints_Returns_Ordered_Days_With_CarryForward()
    {
        var points = MonthlyChartBucketBuilder.BuildDailyEndBalancePoints(
            year: 2026,
            month: 2,
            openingBalanceCents: 1_000,
            movementByDayCents: new Dictionary<int, long>
            {
                [3] = 200,
                [1] = -100,
                [5] = -50
            });

        points.Should().HaveCount(28);
        points.Select(p => p.Day).Should().Equal(Enumerable.Range(1, 28));
        points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(900);
        points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(900);
        points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(1_100);
        points.Single(p => p.Day == 4).EndBalanceCents.Should().Be(1_100);
        points.Single(p => p.Day == 5).EndBalanceCents.Should().Be(1_050);
    }

    [Fact]
    public void AlignSeriesDayBuckets_Aligns_Compared_Series_Using_CarryForward()
    {
        var source = new List<MonthlyChartSeriesDto>
        {
            new(
                SeriesKey: "asset-total",
                DisplayName: "Asset Total",
                EntityId: null,
                EntityType: "scope",
                Points:
                [
                    new MonthlyChartPointDto(2, new DateOnly(2026, 2, 2), 200),
                    new MonthlyChartPointDto(4, new DateOnly(2026, 2, 4), 180)
                ]),
            new(
                SeriesKey: "group:living",
                DisplayName: "Living",
                EntityId: Guid.NewGuid(),
                EntityType: "account-group",
                Points:
                [
                    new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 50),
                    new MonthlyChartPointDto(3, new DateOnly(2026, 2, 3), 70)
                ])
        };

        var aligned = MonthlyChartBucketBuilder.AlignSeriesDayBuckets(2026, 2, source);

        aligned.Should().HaveCount(2);
        aligned.All(series => series.Points.Count == 28).Should().BeTrue();

        var expectedDays = Enumerable.Range(1, 28).ToArray();
        aligned[0].Points.Select(p => p.Day).Should().Equal(expectedDays);
        aligned[1].Points.Select(p => p.Day).Should().Equal(expectedDays);

        aligned[0].Points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(0);
        aligned[0].Points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(200);
        aligned[0].Points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(200);
        aligned[0].Points.Single(p => p.Day == 4).EndBalanceCents.Should().Be(180);

        aligned[1].Points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(50);
        aligned[1].Points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(50);
        aligned[1].Points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(70);
        aligned[1].Points.Single(p => p.Day == 4).EndBalanceCents.Should().Be(70);
    }
}
