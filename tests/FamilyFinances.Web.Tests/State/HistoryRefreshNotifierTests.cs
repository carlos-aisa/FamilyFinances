using FamilyFinances.Web.State;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.State;

public sealed class HistoryRefreshNotifierTests
{
    [Fact]
    public void NotifyChanged_InvokesAllSubscribers()
    {
        var sut = new HistoryRefreshNotifier();
        var firstCount = 0;
        var secondCount = 0;

        using var first = sut.Subscribe(() => firstCount++);
        using var second = sut.Subscribe(() => secondCount++);

        sut.NotifyChanged();

        firstCount.Should().Be(1);
        secondCount.Should().Be(1);
    }

    [Fact]
    public void NotifyChanged_Continues_WhenOneSubscriberThrows()
    {
        var sut = new HistoryRefreshNotifier();
        var successfulCallbackCount = 0;
        using var failing = sut.Subscribe(() => throw new InvalidOperationException("boom"));
        using var successful = sut.Subscribe(() => successfulCallbackCount++);

        var act = () => sut.NotifyChanged();

        act.Should().NotThrow();
        successfulCallbackCount.Should().Be(1);
    }

    [Fact]
    public void Subscription_Dispose_Unsubscribes_AndIsIdempotent()
    {
        var sut = new HistoryRefreshNotifier();
        var callbackCount = 0;
        var subscription = sut.Subscribe(() => callbackCount++);

        subscription.Dispose();
        subscription.Dispose();

        sut.NotifyChanged();
        callbackCount.Should().Be(0);
    }
}
