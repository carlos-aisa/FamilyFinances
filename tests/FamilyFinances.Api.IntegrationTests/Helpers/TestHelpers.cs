using System.Net.Http.Json;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Helpers;

public static class TestHelpers
{
    public static async Task<AccountDto> CreateAccountAsync(HttpClient client, string name, string nature, string kind)
    {
        var natureValue = ParseNature(nature);
        var kindValue = ParseKind(kind, natureValue);

        var res = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name,
            nature = (int)natureValue,
            kind = (int)kindValue,
            openedOn = "2026-01-02"
        });

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"CreateAccountAsync failed with {(int)res.StatusCode} ({res.StatusCode}). Body: {body}");
        }

        var dto = await res.Content.ReadFromJsonAsync<AccountDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private static AccountNature ParseNature(string nature)
        => nature switch
        {
            "Asset" => AccountNature.Asset,
            "Liability" => AccountNature.Liability,
            "Income" => AccountNature.Income,
            "Expense" => AccountNature.Expense,
            "Equity" => AccountNature.Equity,
            _ => throw new ArgumentException($"Unknown nature: {nature}")
        };

    private static AccountKind ParseKind(string kind, AccountNature nature)
        => kind switch
        {
            "Checking" => AccountKind.Checking,
            "Savings" => AccountKind.Savings,
            "CreditCard" => AccountKind.CreditCard,
            "Cash" => AccountKind.Cash,
            "Investment" => AccountKind.Investment,
            "Mortgage" => AccountKind.Mortgage,
            "Loan" => AccountKind.Loan,
            "ExpenseCategory" => AccountKind.ExpenseCategory,
            "IncomeSource" => AccountKind.IncomeSource,
            "Other" => nature switch
            {
                AccountNature.Expense => AccountKind.ExpenseCategory,
                AccountNature.Income => AccountKind.IncomeSource,
                _ => AccountKind.Other
            },
            _ => throw new ArgumentException($"Unknown kind: {kind}")
        };

    public sealed record AccountDto(Guid Id, string Name);
}
