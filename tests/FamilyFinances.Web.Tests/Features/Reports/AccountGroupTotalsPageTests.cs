using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class AccountGroupTotalsPageTests : TestContext
{
    [Fact]
    public void Defaults_To_Period_Totals_Tab()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/account-groups")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(CreateGroupListPayload())
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<AccountGroupTotalsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Filters");
            cut.Markup.Should().NotContain("Account Group Overview");
        });
    }

    [Fact]
    public void State_Evolution_Tab_Loads_Stock_Data()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var groupId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var requestedUris = new List<string>();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                requestedUris.Add(uri);

                if (uri.Contains("api/v1/account-groups/") && uri.Contains(groupId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateGroupDetailsPayload(groupId))
                    };
                }

                if (uri.Contains("api/v1/account-groups"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateGroupListPayload())
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=account-groups"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateMonthlyEvolutionPayload(currentYear, groupId))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<AccountGroupTotalsPage>();

        var stateTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("State Evolution"));
        stateTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=account-groups"));
            cut.Markup.Should().Contain("Account Group Overview");
            cut.Find("[data-testid='account-group-totals-stock-evolution-chart']");
        });
    }

    [Fact]
    public void State_Evolution_Composition_Chart_Shows_Total_Percentage_Near_100()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var firstGroupId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var secondGroupId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var requestedUris = new List<string>();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                requestedUris.Add(uri);

                if (uri.Contains($"api/v1/account-groups/{firstGroupId}", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateExpenseOnlyGroupDetailsPayload(firstGroupId, "Household"))
                    };
                }

                if (uri.Contains($"api/v1/account-groups/{secondGroupId}", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateExpenseOnlyGroupDetailsPayload(secondGroupId, "Transport"))
                    };
                }

                if (uri.Contains("api/v1/account-groups"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateGroupListPayloadForComposition())
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=account-groups"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateMonthlyEvolutionPayloadForComposition(currentYear, firstGroupId, secondGroupId))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<AccountGroupTotalsPage>();

        var stateTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("State Evolution"));
        stateTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=account-groups"));
            cut.Find("[data-testid='account-group-totals-stock-evolution-chart']");
        });

        var compositionModeButton = cut.FindAll("button")
            .First(button => button.TextContent.Trim() == "Composition");
        compositionModeButton.Click();

        cut.WaitForAssertion(() =>
        {
            var chart = cut.Find("[data-testid='account-group-totals-stock-composition-chart']");
            var rawTotal = chart.GetAttribute("data-total-percentage");

            rawTotal.Should().NotBeNullOrWhiteSpace();
            decimal.Parse(rawTotal!, CultureInfo.InvariantCulture)
                .Should().BeApproximately(100m, 0.01m);
        });
    }

    private void RegisterAuthorizedServices(HttpClient httpClient)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
        Services.AddScoped<AccountGroupsApi>();
    }

    private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
    {
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    private static IReadOnlyList<AccountGroupDto> CreateGroupListPayload()
    {
        return
        [
            new AccountGroupDto(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Household",
                "Main household expenses")
        ];
    }

    private static IReadOnlyList<AccountGroupDto> CreateGroupListPayloadForComposition()
    {
        return
        [
            new AccountGroupDto(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Household",
                "Main household expenses"),
            new AccountGroupDto(
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                "Transport",
                "Transport expenses")
        ];
    }

    private static AccountGroupDetailsDto CreateGroupDetailsPayload(Guid groupId)
    {
        return new AccountGroupDetailsDto(
            groupId,
            "Household",
            "Main household expenses",
            [
                new AccountRefDto(
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    "Groceries",
                    AccountNature.Expense,
                    AccountKind.Other),
                new AccountRefDto(
                    Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    "Rent",
                    AccountNature.Expense,
                    AccountKind.Other)
            ]);
    }

    private static AccountGroupDetailsDto CreateExpenseOnlyGroupDetailsPayload(Guid groupId, string accountName)
    {
        return new AccountGroupDetailsDto(
            groupId,
            accountName,
            $"{accountName} group",
            [
                new AccountRefDto(
                    Guid.NewGuid(),
                    $"{accountName} expense",
                    AccountNature.Expense,
                    AccountKind.Other)
            ]);
    }

    private static MonthlyEvolutionReportDto CreateMonthlyEvolutionPayload(int year, Guid groupId)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.AccountGroups,
            [
                new MonthlyEvolutionSeriesDto(
                    "group:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                    "Household",
                    groupId,
                    "account-group",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 120_000, 120_000, 120_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 145_000, 25_000, 145_000)
                    ])
            ]);
    }

    private static MonthlyEvolutionReportDto CreateMonthlyEvolutionPayloadForComposition(
        int year,
        Guid firstGroupId,
        Guid secondGroupId)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.AccountGroups,
            [
                new MonthlyEvolutionSeriesDto(
                    $"group:{firstGroupId:D}",
                    "Household",
                    firstGroupId,
                    "account-group",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 300, 300, 300),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 600, 300, 600)
                    ]),
                new MonthlyEvolutionSeriesDto(
                    $"group:{secondGroupId:D}",
                    "Transport",
                    secondGroupId,
                    "account-group",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 200, 200, 200),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 400, 200, 400)
                    ])
            ]);
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
