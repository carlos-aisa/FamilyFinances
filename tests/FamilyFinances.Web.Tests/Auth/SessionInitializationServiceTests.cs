using FamilyFinances.Web.Auth;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Auth;

public sealed class SessionInitializationServiceTests
{
    [Fact]
    public void IsInitialized_IsFalse_ByDefault()
    {
        var service = new SessionInitializationService();

        service.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsInitialized_CompletesWaiters_AndSetsFlag()
    {
        var service = new SessionInitializationService();

        var waitTask = service.WaitForInitializationAsync();
        waitTask.IsCompleted.Should().BeFalse();

        service.MarkAsInitialized();
        await waitTask;

        service.IsInitialized.Should().BeTrue();
        waitTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsInitialized_IsIdempotent()
    {
        var service = new SessionInitializationService();

        service.MarkAsInitialized();
        service.MarkAsInitialized();

        service.IsInitialized.Should().BeTrue();
        await service.WaitForInitializationAsync();
    }
}
