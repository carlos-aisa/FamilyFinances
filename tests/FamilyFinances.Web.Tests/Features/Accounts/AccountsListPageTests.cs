using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Accounts;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FamilyFinances.Web.Tests.Features.Accounts;

public sealed class AccountsListPageTests : WebTestContext
{
    [Fact]
    public void Accounts_List_Shows_Accumulated_And_CurrentMonth_Balance_Columns()
    {
        var accountId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    accountId,
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(2026, 1, 1),
                    false,
                    null)
            ]);
        accountsApiMock
            .Setup(x => x.GetBalancesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountBalanceDto(accountId, 1234.56m, 345.67m)
            ]);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Accumulated balance");
            cut.Markup.Should().Contain("Current month balance");
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(1234.56m));
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(345.67m));

            var headers = cut.FindAll("thead th").Select(x => x.TextContent).ToList();
            var currentMonthHeaderIndex = headers.FindIndex(text => text.Contains("Current month balance", StringComparison.OrdinalIgnoreCase));
            var accumulatedHeaderIndex = headers.FindIndex(text => text.Contains("Accumulated balance", StringComparison.OrdinalIgnoreCase));
            currentMonthHeaderIndex.Should().BeGreaterThanOrEqualTo(0);
            accumulatedHeaderIndex.Should().BeGreaterThanOrEqualTo(0);
            currentMonthHeaderIndex.Should().BeLessThan(accumulatedHeaderIndex);
        });

        accountsApiMock.Verify(x => x.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
        accountsApiMock.Verify(x => x.GetBalancesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Accounts_List_Shows_CurrentMonth_Period_Basis_Label()
    {
        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Cash",
                    AccountNature.Asset,
                    AccountKind.Cash,
                    new DateOnly(2026, 1, 1),
                    false,
                    null)
            ]);
        accountsApiMock
            .Setup(x => x.GetBalancesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountBalanceDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    50m,
                    10m)
            ]);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Accounts updated as of");
            cut.Markup.Should().Contain(DateTime.Today.ToString("yyyy-MM-dd"));
        });
    }

    private void RegisterAuthorizedServices(IAccountsApi accountsApi)
    {
        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(accountsApi);
        Services.AddSingleton<IHttpClientFactory>(new EmptyHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
    }

    private sealed class EmptyHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri("http://localhost")
        };

        public HttpClient CreateClient(string name) => _client;
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
