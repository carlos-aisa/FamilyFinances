using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class DashboardOverviewTests
{
    [Fact]
    public async Task DashboardOverview_Returns_Overview_Payload_For_Valid_YearMonth()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 10),
            "January salary",
            new Split(bank.Id, 200_000, "Asset increase"),
            new Split(salary.Id, -200_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 5),
            "February groceries",
            new Split(bank.Id, -40_000, "Asset decrease"),
            new Split(groceries.Id, 40_000, "Expense debit"));

        var response = await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}&month=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        root.TryGetProperty("income", out var income).Should().BeTrue();
        root.TryGetProperty("expense", out var expense).Should().BeTrue();
        root.TryGetProperty("netResult", out var netResult).Should().BeTrue();
        root.TryGetProperty("netWorth", out var netWorth).Should().BeTrue();
        root.TryGetProperty("dailyIncomeVsExpense", out var daily).Should().BeTrue();
        root.TryGetProperty("groupStates", out var groups).Should().BeTrue();
        root.TryGetProperty("ytdSummary", out var ytd).Should().BeTrue();
        root.TryGetProperty("compactInsights", out var insights).Should().BeTrue();
        root.TryGetProperty("dataSufficiencyState", out var dataState).Should().BeTrue();

        var incomeValue = income.GetProperty("valueCents").GetInt64();
        var expenseValue = expense.GetProperty("valueCents").GetInt64();
        var netResultValue = netResult.GetProperty("valueCents").GetInt64();

        incomeValue.Should().BeGreaterThanOrEqualTo(0);
        expenseValue.Should().BeLessThanOrEqualTo(0);
        netResultValue.Should().Be(incomeValue + expenseValue);
        netWorth.GetProperty("valueCents").GetInt64().Should().BeGreaterThan(0);
        daily.ValueKind.Should().Be(JsonValueKind.Array);
        groups.ValueKind.Should().Be(JsonValueKind.Array);
        ytd.GetProperty("monthlyNetPoints").ValueKind.Should().Be(JsonValueKind.Array);
        insights.GetArrayLength().Should().BeLessThanOrEqualTo(9);
        dataState.GetInt32().Should().BeOneOf(1, 2, 3);
    }

    [Fact]
    public async Task DashboardOverview_Returns_BadRequest_For_Invalid_Query_Combination()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var year = DateTime.UtcNow.Year;

        (await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync("/api/v1/reports/dashboard-overview?month=2"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}&month=13"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/dashboard-overview?year=1999&month=2"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task PostTransactionAsync(
        HttpClient client,
        DateOnly bookedOn,
        string description,
        params Split[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = bookedOn.ToString("yyyy-MM-dd"),
            description,
            splits = splits.Select(s => new
            {
                accountId = s.AccountId,
                amountCents = s.AmountCents,
                memo = s.Memo
            })
        });

        response.EnsureSuccessStatusCode();
    }

    private sealed record Split(Guid AccountId, long AmountCents, string Memo);
}
