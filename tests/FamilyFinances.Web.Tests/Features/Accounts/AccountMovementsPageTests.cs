using Bunit;
using Bunit.TestDoubles;
using AngleSharp.Dom;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Accounts;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FamilyFinances.Web.Tests.Features.Accounts;

public sealed class AccountMovementsPageTests : WebTestContext
{
    [Fact]
    public void Pagination_Controls_And_Range_Text_Update_When_Navigating_Pages()
    {
        var accountId = Guid.Parse("1d6c3cb0-25ca-4de2-8d7c-0ee85d21e072");
        var requests = new List<MovementsRequest>();
        var accountsApiMock = CreateMovementsApiMock(
            accountId,
            (_, _, _) => 120,
            requests);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountMovementsPage>(parameters => parameters.Add(x => x.Id, accountId));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("1-50 of 120 total");
            cut.Markup.Should().Contain("Page 1 of 3");
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(50m));
        });

        var previous = FindButton(cut, "Previous");
        var next = FindButton(cut, "Next");
        previous.HasAttribute("disabled").Should().BeTrue();
        next.HasAttribute("disabled").Should().BeFalse();

        next.Click();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("51-100 of 120 total");
            cut.Markup.Should().Contain("Page 2 of 3");
        });

        FindButton(cut, "Next").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("101-120 of 120 total");
            cut.Markup.Should().Contain("Page 3 of 3");
        });

        FindButton(cut, "Next").HasAttribute("disabled").Should().BeTrue();
        FindButton(cut, "Previous").HasAttribute("disabled").Should().BeFalse();

        requests.Select(x => x.Page).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void Applying_Filters_Resets_Page_To_First_Page()
    {
        var accountId = Guid.Parse("2b25fd8a-307f-4f6a-a4bb-7c149bd4b835");
        var requests = new List<MovementsRequest>();
        var accountsApiMock = CreateMovementsApiMock(
            accountId,
            (_, _, query) => string.IsNullOrWhiteSpace(query) ? 120 : 20,
            requests);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountMovementsPage>(parameters => parameters.Add(x => x.Id, accountId));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Page 1 of 3");
        });

        FindButton(cut, "Next").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Page 2 of 3");
        });

        var searchInput = cut.Find("input[placeholder='Search by description or payee...']");
        searchInput.Input("rent");
        cut.Find("button.btn.btn-primary.w-100").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("1-20 of 20 total");
            cut.Markup.Should().Contain("Page 1 of 1");
        });

        var latestRequest = requests.Last();
        latestRequest.Page.Should().Be(1);
        latestRequest.Query.Should().Be("rent");
    }

    [Fact]
    public void OutOfRange_Page_Falls_Back_To_Previous_Available_Page()
    {
        var accountId = Guid.Parse("3a5dd2d2-f4e2-4cf8-90b8-6de73f7eeb1f");
        var requests = new List<MovementsRequest>();
        var shrinkAfterNavigation = false;

        var accountsApiMock = CreateMovementsApiMock(
            accountId,
            (_, _, _) => shrinkAfterNavigation ? 60 : 130,
            requests);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountMovementsPage>(parameters => parameters.Add(x => x.Id, accountId));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Page 1 of 3");
        });

        FindButton(cut, "Next").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Page 2 of 3");
        });

        shrinkAfterNavigation = true;
        FindButton(cut, "Next").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("51-60 of 60 total");
            cut.Markup.Should().Contain("Page 2 of 2");
        });

        requests.Select(x => x.Page).Should().ContainInOrder(1, 2, 3, 2);
    }

    [Fact]
    public void Applying_Valid_AmountRange_PassesValues_ToApi()
    {
        var accountId = Guid.Parse("3f93c8c7-7df0-457d-b3ee-1621361f4112");
        var requests = new List<MovementsRequest>();
        var accountsApiMock = CreateMovementsApiMock(
            accountId,
            (_, _, _) => 20,
            requests);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountMovementsPage>(parameters => parameters.Add(x => x.Id, accountId));
        cut.WaitForAssertion(() => requests.Should().NotBeEmpty());

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs.Should().HaveCount(2);
        numericInputs[0].Change("10");
        cut.FindAll("input[type='number']")[1].Change("50");
        cut.Find("button.btn.btn-primary.w-100").Click();

        cut.WaitForAssertion(() =>
        {
            var latestRequest = requests.Last();
            latestRequest.MinAmount.Should().Be(10m);
            latestRequest.MaxAmount.Should().Be(50m);
            latestRequest.Page.Should().Be(1);
        });
    }

    [Fact]
    public void Applying_Invalid_AmountRange_ShowsError_AndSkipsApiRequest()
    {
        var accountId = Guid.Parse("8f4720fb-608f-44d8-aa4d-5457204247d4");
        var requests = new List<MovementsRequest>();
        var accountsApiMock = CreateMovementsApiMock(
            accountId,
            (_, _, _) => 20,
            requests);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountMovementsPage>(parameters => parameters.Add(x => x.Id, accountId));
        cut.WaitForAssertion(() => requests.Should().NotBeEmpty());
        var initialCalls = requests.Count;

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs.Should().HaveCount(2);
        numericInputs[0].Change("100");
        cut.FindAll("input[type='number']")[1].Change("50");
        cut.Find("button.btn.btn-primary.w-100").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Amount From must be less than or equal to Amount To.");
            requests.Count.Should().Be(initialCalls);
        });
    }

    private static Mock<IAccountsApi> CreateMovementsApiMock(
        Guid accountId,
        Func<int, int, string?, int> totalCountResolver,
        ICollection<MovementsRequest> requests)
    {
        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.GetMovementsAsync(
                accountId,
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<string?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, DateOnly? from, DateOnly? to, string? query, decimal? minAmount, decimal? maxAmount, int page, int pageSize, CancellationToken _) =>
            {
                requests.Add(new MovementsRequest(page, pageSize, query, minAmount, maxAmount));
                var totalCount = totalCountResolver(page, pageSize, query);
                return BuildPage(accountId, from, to, query, page, pageSize, totalCount);
            });

        return accountsApiMock;
    }

    private static AccountMovementsDto BuildPage(
        Guid accountId,
        DateOnly? from,
        DateOnly? to,
        string? query,
        int page,
        int pageSize,
        int totalCount)
    {
        var effectivePage = page < 1 ? 1 : page;
        var effectivePageSize = pageSize < 1 ? 50 : pageSize;
        var startIndex = ((effectivePage - 1) * effectivePageSize) + 1;
        var endIndex = Math.Min(effectivePage * effectivePageSize, totalCount);
        var items = new List<AccountMovementDto>();

        if (startIndex <= totalCount)
        {
            for (var sequence = endIndex; sequence >= startIndex; sequence--)
            {
                var descriptionPrefix = string.IsNullOrWhiteSpace(query) ? "Movement" : "Filter movement";
                items.Add(new AccountMovementDto(
                    TransactionId: CreateDeterministicGuid(sequence),
                    BookedOn: new DateOnly(2026, 1, 1).AddDays(sequence - 1),
                    Description: $"{descriptionPrefix} {sequence:D3}",
                    PayeeName: null,
                    SignedAmount: 1.00m,
                    CounterpartyAccountName: "Counterparty",
                    RunningBalance: sequence));
            }
        }

        return new AccountMovementsDto(
            accountId,
            "Main Bank",
            from ?? new DateOnly(2026, 1, 1),
            to ?? new DateOnly(2026, 12, 31),
            items,
            totalCount);
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

    private static IElement FindButton(IRenderedFragment cut, string label)
    {
        return cut.FindAll("button")
            .First(button => button.TextContent.Contains(label, StringComparison.OrdinalIgnoreCase));
    }

    private static Guid CreateDeterministicGuid(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private sealed record MovementsRequest(int Page, int PageSize, string? Query, decimal? MinAmount, decimal? MaxAmount);

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
