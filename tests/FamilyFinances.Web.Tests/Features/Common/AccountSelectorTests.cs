using Bunit;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Components.Shared;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Common;

public sealed class AccountSelectorTests : WebTestContext
{
    [Fact]
    public void AccountSelector_Search_Matches_Accented_Name_With_Unaccented_Query()
    {
        var accounts = BuildAccountsForSearch();
        var cut = RenderComponent<AccountSelector>(parameters => parameters
            .Add(x => x.Accounts, accounts)
            .Add(x => x.ShowSearch, true));

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts...']");
        });

        cut.Find("input[placeholder='Search accounts...']").Input("cafe");

        cut.WaitForAssertion(() =>
        {
            var optionTexts = cut.FindAll("select option")
                .Select(option => option.TextContent)
                .ToList();

            optionTexts.Should().Contain(text => text.Contains("Café Reserve", StringComparison.OrdinalIgnoreCase));
            optionTexts.Should().NotContain(text => text.Contains("Utility Savings", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void AccountSelector_Search_Preserves_FilterByNature()
    {
        var accounts = BuildAccountsForSearch();
        var cut = RenderComponent<AccountSelector>(parameters => parameters
            .Add(x => x.Accounts, accounts)
            .Add(x => x.ShowSearch, true)
            .Add(x => x.FilterByNature, AccountNature.Asset));

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[placeholder='Search accounts...']");
        });

        cut.Find("input[placeholder='Search accounts...']").Input("cafe");

        cut.WaitForAssertion(() =>
        {
            var optionTexts = cut.FindAll("select option")
                .Select(option => option.TextContent)
                .ToList();

            optionTexts.Should().Contain(text => text.Contains("Café Reserve", StringComparison.OrdinalIgnoreCase));
            optionTexts.Should().NotContain(text => text.Contains("Café Expense", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static IReadOnlyList<AccountDto> BuildAccountsForSearch()
    {
        var accounts = new List<AccountDto>
        {
            BuildAccount("Café Reserve", AccountNature.Asset),
            BuildAccount("Café Expense", AccountNature.Expense),
            BuildAccount("Utility Savings", AccountNature.Asset)
        };

        for (var i = 0; i < 9; i++)
        {
            accounts.Add(BuildAccount($"Asset {i}", AccountNature.Asset));
        }

        return accounts;
    }

    private static AccountDto BuildAccount(string name, AccountNature nature)
    {
        return new AccountDto(
            Guid.NewGuid(),
            name,
            nature,
            nature == AccountNature.Expense ? AccountKind.ExpenseCategory : AccountKind.Checking,
            new DateOnly(2026, 1, 1),
            false,
            null);
    }
}
