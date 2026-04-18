using FamilyFinances.Web.Auth;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Auth;

public sealed class ApiTokenStoreTests
{
    [Fact]
    public void GetAccessToken_ReturnsNull_WhenStoreIsEmpty()
    {
        var store = new ApiTokenStore();

        store.GetAccessToken().Should().BeNull();
    }

    [Fact]
    public async Task WaitForTokenAsync_ReturnsToken_WhenTokenAlreadyExists()
    {
        var store = new ApiTokenStore();
        store.SetAccessToken("existing-token");

        var token = await store.WaitForTokenAsync(TimeSpan.FromMilliseconds(200), CancellationToken.None);

        token.Should().Be("existing-token");
    }

    [Fact]
    public async Task WaitForTokenAsync_ReturnsNull_OnTimeout_WhenNoTokenIsSet()
    {
        var store = new ApiTokenStore();

        var token = await store.WaitForTokenAsync(TimeSpan.FromMilliseconds(40), CancellationToken.None);

        token.Should().BeNull();
    }

    [Fact]
    public async Task WaitForTokenAsync_Completes_WhenTokenIsSetLater()
    {
        var store = new ApiTokenStore();

        var waitTask = store.WaitForTokenAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
        await Task.Delay(30);
        store.SetAccessToken("late-token");

        var token = await waitTask;
        token.Should().Be("late-token");
    }

    [Fact]
    public async Task Clear_ResetsPendingWait_AndWaitsForNewToken()
    {
        var store = new ApiTokenStore();
        store.SetAccessToken("old-token");
        store.Clear();

        var waitTask = store.WaitForTokenAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
        await Task.Delay(30);
        store.SetAccessToken("new-token");

        var token = await waitTask;
        token.Should().Be("new-token");
    }
}
