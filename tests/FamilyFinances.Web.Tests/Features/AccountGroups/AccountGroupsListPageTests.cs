using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.AccountGroups;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.AccountGroups;

public sealed class AccountGroupsListPageTests : WebTestContext
{
    [Fact]
    public void List_Shows_DashboardBadge_Only_For_Pinned_Groups()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri!.ToString().Contains("api/v1/account-groups", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<IReadOnlyList<AccountGroupDto>>(
                [
                    new AccountGroupDto(Guid.NewGuid(), "Household", null, IsDashboardPinned: true),
                    new AccountGroupDto(Guid.NewGuid(), "Travel", null, IsDashboardPinned: false)
                ])
            });

        RegisterAuthorizedServices(new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        });

        var cut = RenderComponent<AccountGroupsListPage>();

        cut.WaitForAssertion(() =>
        {
            var badges = cut.FindAll("[data-testid='account-group-dashboard-pinned-badge']");
            badges.Should().ContainSingle();
            badges[0].TextContent.Should().Contain("Shown on dashboard");
            cut.FindAll(".card").Single(card => card.TextContent.Contains("Travel", StringComparison.Ordinal))
                .QuerySelectorAll("[data-testid='account-group-dashboard-pinned-badge']")
                .Should().BeEmpty();
        });
    }

    private void RegisterAuthorizedServices(HttpClient httpClient)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(factory => factory.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<AccountGroupsApi>();
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token)
        {
            _token = token;
        }

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
