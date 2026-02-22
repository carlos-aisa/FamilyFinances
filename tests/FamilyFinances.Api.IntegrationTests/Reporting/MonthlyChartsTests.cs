using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FamilyFinances.Application.Reporting.Dtos;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class MonthlyChartsTests
{
    [Fact]
    public async Task MonthlyCharts_Balance_Returns_Daily_Points_With_CarryForward()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 3;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 15),
            "Baseline salary",
            new Split(bank.Id, 10_000, "Bank increase"),
            new Split(salary.Id, -10_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 2),
            "Monthly salary",
            new Split(bank.Id, 5_000, "Bank increase"),
            new Split(salary.Id, -5_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 5),
            "Groceries",
            new Split(bank.Id, -2_000, "Bank decrease"),
            new Split(groceries.Id, 2_000, "Expense debit"));

        var response = await client.GetAsync($"/api/v1/reports/monthly-charts/balance?year={year}&month={month}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyBalanceChartDto>();
        dto.Should().NotBeNull();

        dto!.Points.Count.Should().Be(DateTime.DaysInMonth(year, month));
        dto.Points.Select(p => p.Day).Should().Equal(Enumerable.Range(1, DateTime.DaysInMonth(year, month)));
        dto.Points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(10_000);
        dto.Points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(15_000);
        dto.Points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(15_000);
        dto.Points.Single(p => p.Day == 5).EndBalanceCents.Should().Be(13_000);
    }

    [Fact]
    public async Task MonthlyCharts_Balance_With_AccountId_Returns_Selected_Account_Daily_Evolution()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 3;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var cash = await TestHelpers.CreateAccountAsync(client, "Cash Wallet", "Asset", "Cash");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 1),
            "Salary in bank",
            new Split(bank.Id, 5_000, "Asset increase"),
            new Split(salary.Id, -5_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 2),
            "Internal transfer",
            new Split(bank.Id, -2_000, "Transfer out"),
            new Split(cash.Id, 2_000, "Transfer in"));

        var response = await client.GetAsync(
            $"/api/v1/reports/monthly-charts/balance?year={year}&month={month}&accountId={bank.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyBalanceChartDto>();
        dto.Should().NotBeNull();

        dto!.Points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(5_000);
        dto.Points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(3_000);
        dto.Points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(3_000);
    }

    [Fact]
    public async Task MonthlyCharts_Balance_With_Income_Nature_Returns_Mirrored_Positive_Series()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 3;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, month - 1, 25),
            "Previous month salary",
            new Split(bank.Id, 10_000, "Asset increase"),
            new Split(salary.Id, -10_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 1),
            "Salary #1",
            new Split(bank.Id, 5_000, "Asset increase"),
            new Split(salary.Id, -5_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 2),
            "Salary #2",
            new Split(bank.Id, 2_000, "Asset increase"),
            new Split(salary.Id, -2_000, "Income credit"));

        var response = await client.GetAsync(
            $"/api/v1/reports/monthly-charts/balance?year={year}&month={month}&nature=Income");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyBalanceChartDto>();
        dto.Should().NotBeNull();

        dto!.Points.Single(p => p.Day == 1).EndBalanceCents.Should().Be(5_000);
        dto.Points.Single(p => p.Day == 2).EndBalanceCents.Should().Be(7_000);
        dto.Points.Single(p => p.Day == 3).EndBalanceCents.Should().Be(7_000);
    }

    [Fact]
    public async Task MonthlyCharts_BalanceVsGroups_Returns_Aligned_Day_Buckets()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 4;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        var household = await CreateGroupAsync(client, "Household", "Main group");
        await AddAccountToGroupAsync(client, household.Id, groceries.Id);

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 1),
            "Salary",
            new Split(bank.Id, 4_000, "Asset increase"),
            new Split(salary.Id, -4_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, month, 2),
            "Groceries",
            new Split(bank.Id, -1_200, "Asset decrease"),
            new Split(groceries.Id, 1_200, "Expense debit"));

        var response = await client.GetAsync($"/api/v1/reports/monthly-charts/group-evolution?year={year}&month={month}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyBalanceVsGroupsChartDto>();
        dto.Should().NotBeNull();

        dto!.Series.Should().NotBeEmpty();
        dto.Series.Should().Contain(s => s.SeriesKey == "asset-total");
        dto.Series.Should().Contain(s => s.EntityId == household.Id);

        var expectedDays = Enumerable.Range(1, DateTime.DaysInMonth(year, month)).ToArray();
        foreach (var series in dto.Series)
        {
            series.Points.Select(p => p.Day).Should().Equal(expectedDays);
        }
    }

    [Fact]
    public async Task MonthlyCharts_Reject_Invalid_Query_Inputs()
    {
        var year = DateTime.UtcNow.Year;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        (await client.GetAsync("/api/v1/reports/monthly-charts/balance"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-charts/balance?year={year}"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-charts/balance?month=2"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-charts/balance?year={year}&month=13"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-charts/balance?year={year}&month=2&accountId={Guid.NewGuid()}&nature=Income"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-charts/group-evolution?year={year + 1}&month=2"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MonthlyCharts_Balance_NoDataMonth_Returns_ZeroCarryForwardSeries()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 6;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync($"/api/v1/reports/monthly-charts/balance?year={year}&month={month}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyBalanceChartDto>();
        dto.Should().NotBeNull();
        dto!.Points.Should().HaveCount(DateTime.DaysInMonth(year, month));
        dto.Points.Should().OnlyContain(point => point.EndBalanceCents == 0);
    }

    [Fact]
    public async Task MonthlyCharts_GroupEvolution_LegacyAlias_Remains_Available()
    {
        var year = DateTime.UtcNow.Year;
        const int month = 2;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var primary = await client.GetAsync($"/api/v1/reports/monthly-charts/group-evolution?year={year}&month={month}");
        var alias = await client.GetAsync($"/api/v1/reports/monthly-charts/balance-vs-groups?year={year}&month={month}");

        primary.StatusCode.Should().Be(HttpStatusCode.OK);
        alias.StatusCode.Should().Be(HttpStatusCode.OK);

        var primaryDto = await primary.Content.ReadFromJsonAsync<MonthlyBalanceVsGroupsChartDto>();
        var aliasDto = await alias.Content.ReadFromJsonAsync<MonthlyBalanceVsGroupsChartDto>();

        primaryDto.Should().NotBeNull();
        aliasDto.Should().NotBeNull();
        aliasDto.Should().BeEquivalentTo(primaryDto);
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

    private static async Task<AccountGroupDto> CreateGroupAsync(HttpClient client, string name, string? description)
    {
        var response = await client.PostAsJsonAsync("/api/v1/account-groups", new { name, description });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<AccountGroupDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private static async Task AddAccountToGroupAsync(HttpClient client, Guid groupId, Guid accountId)
    {
        var response = await client.PostAsync($"/api/v1/account-groups/{groupId}/accounts/{accountId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record Split(Guid AccountId, long AmountCents, string Memo);

    private sealed record AccountGroupDto(Guid Id, string Name, string? Description);
}
