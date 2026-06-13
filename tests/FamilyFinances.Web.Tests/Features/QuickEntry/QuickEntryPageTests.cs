using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.QuickEntry;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.QuickEntry;

public sealed class QuickEntryPageTests : WebTestContext
{
    [Fact]
    public void QuickEntry_Uses_Global_Search_And_SingleOpen_Accordion_For_Accounts()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("Main Bank", AccountNature.Asset, AccountKind.Checking),
            BuildAccount("Groceries", AccountNature.Expense, AccountKind.ExpenseCategory),
            BuildAccount("Salary", AccountNature.Income, AccountKind.IncomeSource)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts by name or type']");
            cut.Find("#quick-entry-accounts.ff-single-open-accordion");
        });

        var collapses = cut.FindAll("#quick-entry-accounts .accordion-collapse");
        collapses.Should().NotBeEmpty();
        collapses.Should().OnlyContain(c => c.GetAttribute("data-bs-parent") == "#quick-entry-accounts");
        cut.FindAll("#quick-entry-accounts .accordion-collapse.show").Should().HaveCount(1);

        var headers = cut.FindAll("#quick-entry-accounts .accordion-button");
        headers.Count.Should().BeGreaterThanOrEqualTo(2);
        headers[1].Click();

        cut.WaitForAssertion(() =>
        {
            cut.FindAll("#quick-entry-accounts .accordion-collapse.show").Should().HaveCount(1);
            cut.FindAll("#quick-entry-accounts .accordion-collapse")[1].ClassList.Should().Contain("show");
        });
    }

    [Fact]
    public void QuickEntry_GlobalSearch_Filters_By_Account_Nature_Label()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("Main Bank", AccountNature.Asset, AccountKind.Checking),
            BuildAccount("Food Budget", AccountNature.Expense, AccountKind.ExpenseCategory),
            BuildAccount("Salary", AccountNature.Income, AccountKind.IncomeSource)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts by name or type']");
            cut.FindAll(".ff-dashboard-account-item").Should().NotBeEmpty();
        });

        var search = cut.Find("input[placeholder='Search accounts by name or type']");
        search.Input("expense");

        cut.WaitForAssertion(() =>
        {
            var visibleItems = cut.FindAll(".ff-dashboard-account-item")
                .Select(item => item.TextContent)
                .ToList();

            visibleItems.Should().Contain(text => text.Contains("Food Budget", StringComparison.OrdinalIgnoreCase));
            visibleItems.Should().NotContain(text => text.Contains("Main Bank", StringComparison.OrdinalIgnoreCase));
            visibleItems.Should().NotContain(text => text.Contains("Salary", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void QuickEntry_GlobalSearch_Matches_Accented_Account_Name_With_Unaccented_Query()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("María Account", AccountNature.Asset, AccountKind.Checking),
            BuildAccount("Salary", AccountNature.Income, AccountKind.IncomeSource)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts by name or type']");
            cut.FindAll(".ff-dashboard-account-item").Should().NotBeEmpty();
        });

        var search = cut.Find("input[placeholder='Search accounts by name or type']");
        search.Input("maria");

        cut.WaitForAssertion(() =>
        {
            var visibleItems = cut.FindAll(".ff-dashboard-account-item")
                .Select(item => item.TextContent)
                .ToList();

            visibleItems.Should().Contain(text => text.Contains("María Account", StringComparison.OrdinalIgnoreCase));
            visibleItems.Should().NotContain(text => text.Contains("Salary", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void QuickEntry_GlobalSearch_Matches_NonAccented_Account_Name_With_Accented_Query()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("Jose Account", AccountNature.Asset, AccountKind.Checking),
            BuildAccount("Salary", AccountNature.Income, AccountKind.IncomeSource)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts by name or type']");
            cut.FindAll(".ff-dashboard-account-item").Should().NotBeEmpty();
        });

        var search = cut.Find("input[placeholder='Search accounts by name or type']");
        search.Input("Jos\u00E9");

        cut.WaitForAssertion(() =>
        {
            var visibleItems = cut.FindAll(".ff-dashboard-account-item")
                .Select(item => item.TextContent)
                .ToList();

            visibleItems.Should().Contain(text => text.Contains("Jose Account", StringComparison.OrdinalIgnoreCase));
            visibleItems.Should().NotContain(text => text.Contains("Salary", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void QuickEntry_GlobalSearch_Matches_Custom_Kind_Label()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("Travel Wallet", AccountNature.Asset, AccountKind.Other, kindName: "Travel"),
            BuildAccount("Main Bank", AccountNature.Asset, AccountKind.Checking)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts by name or type']");
            cut.FindAll(".ff-dashboard-account-item").Should().NotBeEmpty();
        });

        var search = cut.Find("input[placeholder='Search accounts by name or type']");
        search.Input("travel");

        cut.WaitForAssertion(() =>
        {
            var visibleItems = cut.FindAll(".ff-dashboard-account-item")
                .Select(item => item.TextContent)
                .ToList();

            visibleItems.Should().Contain(text => text.Contains("Travel Wallet", StringComparison.OrdinalIgnoreCase));
            visibleItems.Should().NotContain(text => text.Contains("Main Bank", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void QuickEntry_Shows_Mode_Guidance_And_Persists_Selected_Date_Across_Mode_Switch()
    {
        RegisterAuthorizedServices(CreateAccountsApiMock(
        [
            BuildAccount("Main Bank", AccountNature.Asset, AccountKind.Checking),
            BuildAccount("Groceries", AccountNature.Expense, AccountKind.ExpenseCategory),
            BuildAccount("Salary", AccountNature.Income, AccountKind.IncomeSource),
            BuildAccount("Credit Card", AccountNature.Liability, AccountKind.CreditCard)
        ]));

        var cut = RenderComponent<QuickEntryPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Select an Asset/Liability source account and an expense destination account.");
        });

        var dateInput = cut.Find("input[type='date']");
        dateInput.Change("2026-03-15");

        ClickCardHeader(cut, "Income");
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Select an income source account and an Asset destination account.");
            cut.Find("input[type='date']").GetAttribute("value").Should().Be("2026-03-15");
        });

        ClickCardHeader(cut, "Transfer");
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Select source and destination accounts from Asset/Liability natures.");
            cut.Find("input[type='date']").GetAttribute("value").Should().Be("2026-03-15");
        });

        ClickCardHeader(cut, "Refund");
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Select the expense category to refund and an Asset/Liability destination account.");
            cut.Find("input[type='date']").GetAttribute("value").Should().Be("2026-03-15");
        });
    }

    private void RegisterAuthorizedServices(IAccountsApi accountsApi)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();

                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(Array.Empty<PayeeDto>())
                    };
                }

                if (uri.Contains("api/v1/transactions/any", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new HasAnyResult(true))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

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
        Services.AddSingleton(accountsApi);
        Services.AddScoped<PayeesApi>();
        Services.AddScoped<TransactionsApi>();
    }

    private static IAccountsApi CreateAccountsApiMock(IReadOnlyList<AccountDto> accounts)
    {
        var mock = new Mock<IAccountsApi>(MockBehavior.Strict);
        mock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);
        return mock.Object;
    }

    private static AccountDto BuildAccount(string name, AccountNature nature, AccountKind kind, string? kindName = null)
    {
        return new AccountDto(
            Guid.NewGuid(),
            name,
            nature,
            kind,
            new DateOnly(2026, 1, 1),
            false,
            null,
            Guid.NewGuid(),
            kind.ToString().ToLowerInvariant(),
            kindName ?? kind.ToString());
    }

    private static void ClickCardHeader(IRenderedComponent<QuickEntryPage> cut, string title)
    {
        cut.FindAll(".card-header.cursor-pointer")
            .First(header => header.TextContent.Contains(title, StringComparison.OrdinalIgnoreCase))
            .Click();
    }

    private sealed record HasAnyResult(bool HasAny);

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
