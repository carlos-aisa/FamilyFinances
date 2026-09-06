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

        var groupResponse = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Household",
            description = (string?)null
        });
        groupResponse.EnsureSuccessStatusCode();

        using var groupJson = JsonDocument.Parse(await groupResponse.Content.ReadAsStringAsync());
        groupJson.RootElement.GetProperty("isDashboardPinned").GetBoolean().Should().BeFalse();
        var groupId = groupJson.RootElement.GetProperty("id").GetGuid();
        foreach (var accountId in new[] { bank.Id, salary.Id, groceries.Id })
        {
            (await client.PostAsync($"/api/v1/account-groups/{groupId}/accounts/{accountId}", null))
                .EnsureSuccessStatusCode();
        }

        (await client.PatchAsync(
            $"/api/v1/account-groups/{groupId}",
            JsonContent.Create(new { isDashboardPinned = true })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var sharedGroupResponse = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Shared household",
            description = (string?)null
        });
        sharedGroupResponse.EnsureSuccessStatusCode();

        using var sharedGroupJson = JsonDocument.Parse(await sharedGroupResponse.Content.ReadAsStringAsync());
        var sharedGroupId = sharedGroupJson.RootElement.GetProperty("id").GetGuid();
        foreach (var accountId in new[] { salary.Id, groceries.Id })
        {
            (await client.PostAsync($"/api/v1/account-groups/{sharedGroupId}/accounts/{accountId}", null))
                .EnsureSuccessStatusCode();
        }

        (await client.PatchAsync(
            $"/api/v1/account-groups/{sharedGroupId}",
            JsonContent.Create(new { isDashboardPinned = true })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

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
        root.TryGetProperty("expenseKindRanking", out var expenseKindRanking).Should().BeTrue();
        root.TryGetProperty("pinnedGroups", out var pinnedGroups).Should().BeTrue();

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
        expenseKindRanking.ValueKind.Should().Be(JsonValueKind.Array);
        expenseKindRanking[0].GetProperty("amountCents").GetInt64().Should().Be(40_000);
        pinnedGroups.ValueKind.Should().Be(JsonValueKind.Array);
        pinnedGroups.GetArrayLength().Should().Be(2);
        pinnedGroups.EnumerateArray()
            .Select(group => group.GetProperty("monthOperationalResultCents").GetInt64())
            .Should().AllBeEquivalentTo(-40_000);
        pinnedGroups.EnumerateArray()
            .Select(group => group.GetProperty("ytdOperationalResultCents").GetInt64())
            .Should().AllBeEquivalentTo(160_000);
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

    [Fact]
    public async Task DashboardOverview_Orders_PinnedGroups_By_MonthlyOperationalResult()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var lowerExpense = await TestHelpers.CreateAccountAsync(client, "Lower expense", "Expense", "Other");
        var higherExpense = await TestHelpers.CreateAccountAsync(client, "Higher expense", "Expense", "Other");

        var lowerGroupId = await CreatePinnedGroupAsync(client, "Lower group", lowerExpense.Id);
        var higherGroupId = await CreatePinnedGroupAsync(client, "Higher group", higherExpense.Id);

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 5),
            "Lower expense",
            new Split(bank.Id, -10_000, "Asset decrease"),
            new Split(lowerExpense.Id, 10_000, "Expense debit"));
        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 6),
            "Higher expense",
            new Split(bank.Id, -40_000, "Asset decrease"),
            new Split(higherExpense.Id, 40_000, "Expense debit"));

        var response = await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}&month=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var pinnedGroups = json.RootElement.GetProperty("pinnedGroups");

        pinnedGroups.EnumerateArray()
            .Select(group => group.GetProperty("groupId").GetGuid())
            .Should().ContainInOrder(higherGroupId, lowerGroupId);
    }

    [Fact]
    public async Task DashboardOverview_Returns_YTD_Summary_WithMonthlyPoints()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 15),
            "January salary",
            new Split(bank.Id, 100_000, "Asset increase"),
            new Split(salary.Id, -100_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 15),
            "February salary",
            new Split(bank.Id, 120_000, "Asset increase"),
            new Split(salary.Id, -120_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 3, 15),
            "March salary",
            new Split(bank.Id, 150_000, "Asset increase"),
            new Split(salary.Id, -150_000, "Income credit"));

        var response = await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}&month=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        root.TryGetProperty("ytdSummary", out var ytdSummary).Should().BeTrue("response should contain ytdSummary");
        ytdSummary.TryGetProperty("accumulatedNetCents", out var accumulated).Should().BeTrue("ytdSummary should contain accumulatedNetCents");
        ytdSummary.TryGetProperty("monthlyNetPoints", out var monthlyPoints).Should().BeTrue("ytdSummary should contain monthlyNetPoints");

        monthlyPoints.ValueKind.Should().Be(JsonValueKind.Array, "monthlyNetPoints should be an array");
        monthlyPoints.GetArrayLength().Should().BeGreaterThan(0, "monthlyNetPoints should contain data");

        accumulated.GetInt64().Should().BeGreaterThan(0, "accumulatedNetCents should be calculated");
    }

    [Fact]
    public async Task DashboardOverview_YTD_Calculation_Matches_Expected()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // +200k in January
        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 15),
            "January salary",
            new Split(bank.Id, 200_000, "Asset increase"),
            new Split(salary.Id, -200_000, "Income credit"));

        // +150k in February
        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 15),
            "February salary",
            new Split(bank.Id, 150_000, "Asset increase"),
            new Split(salary.Id, -150_000, "Income credit"));

        // -50k in March
        await PostTransactionAsync(
            client,
            new DateOnly(year, 3, 10),
            "March groceries",
            new Split(bank.Id, -50_000, "Asset decrease"),
            new Split(groceries.Id, 50_000, "Expense debit"));

        var response = await client.GetAsync($"/api/v1/reports/dashboard-overview?year={year}&month=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        root.TryGetProperty("ytdSummary", out var ytdSummary).Should().BeTrue();
        ytdSummary.TryGetProperty("accumulatedNetCents", out var accumulated).Should().BeTrue();

        // Expected: +200k (Jan) +150k (Feb) -50k (Mar) = 300k
        accumulated.GetInt64().Should().Be(300_000, "YTD accumulated net should be 200k + 150k - 50k = 300k");
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

    private static async Task<Guid> CreatePinnedGroupAsync(HttpClient client, string name, Guid accountId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name,
            description = (string?)null
        });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var groupId = json.RootElement.GetProperty("id").GetGuid();
        (await client.PostAsync($"/api/v1/account-groups/{groupId}/accounts/{accountId}", null)).EnsureSuccessStatusCode();
        (await client.PatchAsync(
            $"/api/v1/account-groups/{groupId}",
            JsonContent.Create(new { isDashboardPinned = true }))).EnsureSuccessStatusCode();

        return groupId;
    }

    private sealed record Split(Guid AccountId, long AmountCents, string Memo);
}
