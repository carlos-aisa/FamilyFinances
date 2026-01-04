using System.Net.Http.Json;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Helpers;

public static class TestHelpers
{
    public static async Task<AccountDto> CreateAccountAsync(HttpClient client, string name, string nature, string kind)
    {
        // Convert enum names to numeric values for JSON
        var natureValue = nature switch
        {
            "Asset" => 1,
            "Liability" => 2,
            "Income" => 3,
            "Expense" => 4,
            "Equity" => 5,
            _ => throw new ArgumentException($"Unknown nature: {nature}")
        };

        var kindValue = kind switch
        {
            "Checking" => 1,
            "Savings" => 2,
            "CreditCard" => 3,
            "Cash" => 4,
            "Investment" => 5,
            "Loan" => 6,
            "Other" => 7,
            _ => throw new ArgumentException($"Unknown kind: {kind}")
        };

        var res = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name,
            nature = natureValue,
            kind = kindValue,
            openedOn = "2026-01-02"
        });

        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<AccountDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    public sealed record AccountDto(Guid Id, string Name);
}
