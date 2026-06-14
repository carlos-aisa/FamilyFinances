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
        accountsApiMock
            .Setup(x => x.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new AccountKindCatalogDto(Guid.NewGuid(), "checking", "Checking", true, true, 10, AccountKind.Checking)
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
        accountsApiMock.Verify(x => x.ListKindsAsync(true, It.IsAny<CancellationToken>()), Times.Once);
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
        accountsApiMock
            .Setup(x => x.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new AccountKindCatalogDto(Guid.NewGuid(), "cash", "Cash", true, true, 10, AccountKind.Cash)
            ]);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Accounts updated as of");
            cut.Markup.Should().Contain(DateTime.Today.ToString("yyyy-MM-dd"));
        });
    }

    [Fact]
    public void Accounts_List_KindSelector_IsOrderedByVisibleLabel()
    {
        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);

        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
                    "Groceries",
                    AccountNature.Expense,
                    AccountKind.ExpenseCategory,
                    new DateOnly(2026, 1, 1),
                    false,
                    null)
            ]);

        accountsApiMock
            .Setup(x => x.GetBalancesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        accountsApiMock
            .Setup(x => x.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000001"), "zoo", "Zoo", false, true, 1000, AccountKind.Other, AccountNature.Expense),
                new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000002"), "expense-category", "Expense Category", true, true, 60, AccountKind.ExpenseCategory, AccountNature.Expense),
                new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000003"), "alpha", "Alpha", false, true, 1010, AccountKind.Other, AccountNature.Expense),
                new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000004"), "other", "Other", true, true, 100, AccountKind.Other, AccountNature.Equity)
            ]);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("New Account");
        });

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Account", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var optionTexts = cut.FindAll("select.ff-account-kind-select option")
                .Select(option => option.TextContent.Trim())
                .ToList();

            optionTexts.Should().ContainInOrder("Alpha", "Expense category", "Other", "Zoo");
        });
    }

    [Fact]
    public void Accounts_List_CreateForm_Hides_FullKindManagement_ByDefault()
    {
        var accountsApiMock = CreateAccountsApiMock(
            CreateAccounts(AccountNature.Expense),
            CreateKinds());

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Account", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ff-kind-management-panel").Should().BeEmpty();
            cut.Markup.Should().NotContain("Custom kinds");
            cut.FindAll(".ff-account-kind-select").Should().HaveCount(1);
        });
    }

    [Fact]
    public void Accounts_List_KindSelector_FiltersAndFallsBack_WhenNatureChanges()
    {
        var accountsApiMock = CreateAccountsApiMock(
            CreateAccounts(AccountNature.Expense),
            CreateKinds());

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Account", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var initialOptionTexts = cut.FindAll("select.ff-account-kind-select option")
                .Select(option => option.TextContent.Trim())
                .ToList();

            initialOptionTexts.Should().ContainInOrder("Expense category", "Food", "Other");
        });

        cut.Find("select.ff-account-nature-select").Change(AccountNature.Asset.ToString());

        cut.WaitForAssertion(() =>
        {
            var optionTexts = cut.FindAll("select.ff-account-kind-select option")
                .Select(option => option.TextContent.Trim())
                .ToList();

            optionTexts.Should().ContainInOrder("Checking", "Other", "Savings", "Travel Wallet");
            cut.Find("select.ff-account-kind-select option[selected]").TextContent.Trim().Should().Be("Checking");
        });
    }

    [Fact]
    public void Accounts_List_InlineKindCreation_CreatesCompatibleKind_AndSelectsIt()
    {
        var existingKinds = CreateKinds();
        var createdKind = new AccountKindCatalogDto(
            Guid.Parse("30000000-0000-0000-0000-000000000001"),
            "travel",
            "Travel",
            false,
            true,
            1100,
            AccountKind.Other,
            AccountNature.Expense);

        var accountsApiMock = CreateAccountsApiMock(
            CreateAccounts(AccountNature.Expense),
            existingKinds);
        accountsApiMock
            .Setup(x => x.CreateKindAsync("Travel", AccountNature.Expense, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdKind);

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Account", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.Find(".ff-account-kind-create-toggle").Click();
        cut.Find("input.ff-account-kind-create-input").Change("Travel");
        cut.Find("button.ff-account-kind-create-submit").Click();

        cut.WaitForAssertion(() =>
        {
            accountsApiMock.Verify(x => x.CreateKindAsync("Travel", AccountNature.Expense, It.IsAny<CancellationToken>()), Times.Once);

            var optionTexts = cut.FindAll("select.ff-account-kind-select option")
                .Select(option => option.TextContent.Trim())
                .ToList();

            optionTexts.Should().Contain("Travel");
            cut.Find("select.ff-account-kind-select option[selected]").TextContent.Trim().Should().Be("Travel");
        });
    }

    [Fact]
    public void Accounts_List_InlineKindCreation_ShowsLocalError_AndPreservesFormState()
    {
        var accountsApiMock = CreateAccountsApiMock(
            CreateAccounts(AccountNature.Expense),
            CreateKinds());
        accountsApiMock
            .Setup(x => x.CreateKindAsync("Duplicate", AccountNature.Expense, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Kind already exists"));

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Account", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.Find("input.ff-account-name-input").Change("Groceries Envelope");
        cut.Find(".ff-account-kind-create-toggle").Click();
        cut.Find("input.ff-account-kind-create-input").Change("Duplicate");
        cut.Find("button.ff-account-kind-create-submit").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Find(".ff-account-kind-create-error").TextContent.Should().Contain("Kind already exists");
            cut.Find("input.ff-account-name-input").GetAttribute("value").Should().Be("Groceries Envelope");
        });
    }

    [Fact]
    public void Accounts_List_ManageKinds_OpensSecondaryManagementSurface()
    {
        var accountsApiMock = CreateAccountsApiMock(
            CreateAccounts(AccountNature.Expense),
            CreateKinds());

        RegisterAuthorizedServices(accountsApiMock.Object);

        var cut = RenderComponent<AccountsListPage>();

        cut.FindAll(".ff-kind-management-panel").Should().BeEmpty();

        cut.Find("button.ff-manage-kinds-toggle").Click();

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".ff-kind-management-panel").Should().HaveCount(1);
            cut.Markup.Should().Contain("Custom kinds");
            cut.FindAll(".ff-kind-management-create-name").Should().HaveCount(1);
            cut.Markup.Should().Contain("Enable");
            cut.Markup.Should().Contain("Delete");
        });
    }

    private static Mock<IAccountsApi> CreateAccountsApiMock(
        IReadOnlyList<AccountDto> accounts,
        IReadOnlyList<AccountKindCatalogDto> kinds)
    {
        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);
        accountsApiMock
            .Setup(x => x.GetBalancesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AccountBalanceDto>());
        accountsApiMock
            .Setup(x => x.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(kinds);

        return accountsApiMock;
    }

    private static IReadOnlyList<AccountDto> CreateAccounts(AccountNature nature)
    {
        return
        [
            new AccountDto(
                Guid.Parse("40000000-0000-0000-0000-000000000001"),
                "Reference account",
                nature,
                nature == AccountNature.Asset ? AccountKind.Checking : AccountKind.ExpenseCategory,
                new DateOnly(2026, 1, 1),
                false,
                null)
        ];
    }

    private static IReadOnlyList<AccountKindCatalogDto> CreateKinds()
    {
        return
        [
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000010"), "checking", "Checking", true, true, 10, AccountKind.Checking, AccountNature.Asset),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000011"), "savings", "Savings", true, true, 20, AccountKind.Savings, AccountNature.Asset),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000012"), "expense-category", "Expense Category", true, true, 60, AccountKind.ExpenseCategory, AccountNature.Expense),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000013"), "other", "Other", true, true, 100, AccountKind.Other, AccountNature.Equity),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000014"), "food", "Food", false, true, 1000, AccountKind.Other, AccountNature.Expense),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000015"), "travel-wallet", "Travel Wallet", false, true, 1010, AccountKind.Other, AccountNature.Asset),
            new AccountKindCatalogDto(Guid.Parse("10000000-0000-0000-0000-000000000016"), "seasonal", "Seasonal", false, false, 1020, AccountKind.Other, AccountNature.Expense)
        ];
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
