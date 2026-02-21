using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class MonthlyEvolutionTests
{
    [Fact]
    public async Task StateEvolution_New_Route_Returns_Ok_For_Valid_Query()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync($"/api/v1/reports/state-evolution?year={year}&scope=asset-total");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StateEvolution_Returns_BadRequest_For_Missing_Or_Invalid_Query_Parameters()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        (await client.GetAsync("/api/v1/reports/state-evolution"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/state-evolution?year={DateTime.UtcNow.Year}"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync("/api/v1/reports/state-evolution?scope=asset-total"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/state-evolution?year={DateTime.UtcNow.Year}&scope=invalid"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/state-evolution?year={DateTime.UtcNow.Year + 1}&scope=asset-total"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StateEvolution_Primary_And_Legacy_Alias_Return_Equivalent_Payload()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var primaryResponse = await client.GetAsync($"/api/v1/reports/state-evolution?year={year}&scope=asset-total");
        var aliasResponse = await client.GetAsync($"/api/v1/reports/monthly-evolution?year={year}&scope=asset-total");

        primaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        aliasResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var primary = await primaryResponse.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        var alias = await aliasResponse.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();

        primary.Should().NotBeNull();
        alias.Should().NotBeNull();
        alias.Should().BeEquivalentTo(primary);
    }

    [Fact]
    public async Task MonthlyEvolution_AccountsScope_Returns_AccountSeries_With_Correct_Deltas()
    {
        var year = DateTime.UtcNow.Year - 1;
        var baselineYear = year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(baselineYear, 12, 15),
            "Baseline salary",
            new Split(bank.Id, 20_000, "Bank increase"),
            new Split(salary.Id, -20_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 10),
            "January salary",
            new Split(bank.Id, 10_000, "Bank increase"),
            new Split(salary.Id, -10_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 5),
            "Groceries",
            new Split(bank.Id, -3_000, "Bank decrease"),
            new Split(groceries.Id, 3_000, "Expense"));

        var response = await client.GetAsync($"/api/v1/reports/monthly-evolution?year={year}&scope=accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        dto.Should().NotBeNull();
        dto!.Year.Should().Be(year);

        var bankSeries = dto.Series.Single(s => s.EntityId == bank.Id);
        bankSeries.SeriesKey.Should().Be($"account:{bank.Id:D}");
        bankSeries.Points.Select(p => p.Month).Should().Equal(Enumerable.Range(1, 12));

        var jan = bankSeries.Points.Single(p => p.Month == 1);
        jan.EndBalanceCents.Should().Be(30_000);
        jan.DeltaVsPreviousMonthCents.Should().Be(10_000);
        jan.DeltaVsYearStartCents.Should().Be(10_000);

        var feb = bankSeries.Points.Single(p => p.Month == 2);
        feb.EndBalanceCents.Should().Be(27_000);
        feb.DeltaVsPreviousMonthCents.Should().Be(-3_000);
        feb.DeltaVsYearStartCents.Should().Be(7_000);
    }

    [Fact]
    public async Task MonthlyEvolution_AssetTotalScope_Returns_Single_AssetOnly_Aggregation()
    {
        var year = DateTime.UtcNow.Year - 1;
        var baselineYear = year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var cash = await TestHelpers.CreateAccountAsync(client, "Cash", "Asset", "Cash");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(baselineYear, 12, 5),
            "Baseline",
            new Split(bank.Id, 10_000, "Asset increase"),
            new Split(salary.Id, -10_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 7),
            "January salary",
            new Split(bank.Id, 5_000, "Asset increase"),
            new Split(salary.Id, -5_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 1),
            "Internal transfer",
            new Split(bank.Id, -2_000, "Transfer out"),
            new Split(cash.Id, 2_000, "Transfer in"));

        var response = await client.GetAsync($"/api/v1/reports/monthly-evolution?year={year}&scope=asset-total");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        dto.Should().NotBeNull();
        dto!.Series.Should().HaveCount(1);

        var series = dto.Series.Single();
        series.SeriesKey.Should().Be("asset-total");
        series.Points.Should().HaveCount(12);

        var jan = series.Points.Single(p => p.Month == 1);
        jan.EndBalanceCents.Should().Be(15_000);
        jan.DeltaVsPreviousMonthCents.Should().Be(5_000);
        jan.DeltaVsYearStartCents.Should().Be(5_000);

        var feb = series.Points.Single(p => p.Month == 2);
        feb.EndBalanceCents.Should().Be(15_000);
        feb.DeltaVsPreviousMonthCents.Should().Be(0);
        feb.DeltaVsYearStartCents.Should().Be(5_000);
    }

    [Fact]
    public async Task MonthlyEvolution_AccountGroupsScope_Returns_Group_Aggregates_With_Deterministic_Points()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var bills = await TestHelpers.CreateAccountAsync(client, "Bills", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        var living = await CreateGroupAsync(client, "Living", "Home costs");
        await AddAccountToGroupAsync(client, living.Id, groceries.Id);
        await AddAccountToGroupAsync(client, living.Id, bills.Id);

        var income = await CreateGroupAsync(client, "Income", "Income sources");
        await AddAccountToGroupAsync(client, income.Id, salary.Id);

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 10),
            "Groceries",
            new Split(bank.Id, -2_000, "Payment"),
            new Split(groceries.Id, 2_000, "Expense"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 2, 12),
            "Bills",
            new Split(bank.Id, -3_000, "Payment"),
            new Split(bills.Id, 3_000, "Expense"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 3, 5),
            "Salary",
            new Split(bank.Id, 10_000, "Deposit"),
            new Split(salary.Id, -10_000, "Income credit"));

        var response = await client.GetAsync($"/api/v1/reports/monthly-evolution?year={year}&scope=account-groups");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        dto.Should().NotBeNull();

        var livingSeries = dto!.Series.Single(s => s.EntityId == living.Id);
        livingSeries.Points.Should().HaveCount(12);
        livingSeries.Points.Select(p => p.Month).Should().Equal(Enumerable.Range(1, 12));

        livingSeries.Points.Single(p => p.Month == 1).Should().BeEquivalentTo(
            new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 2_000, 2_000, 2_000));
        livingSeries.Points.Single(p => p.Month == 2).Should().BeEquivalentTo(
            new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 5_000, 3_000, 5_000));
        livingSeries.Points.Single(p => p.Month == 3).Should().BeEquivalentTo(
            new MonthlyEvolutionPointDto(3, new DateOnly(year, 3, 31), 5_000, 0, 5_000));

        var incomeSeries = dto.Series.Single(s => s.EntityId == income.Id);
        incomeSeries.Points.Single(p => p.Month == 3).EndBalanceCents.Should().Be(-10_000);
        incomeSeries.Points.Single(p => p.Month == 3).DeltaVsPreviousMonthCents.Should().Be(-10_000);
        incomeSeries.Points.Single(p => p.Month == 3).DeltaVsYearStartCents.Should().Be(-10_000);
    }

    [Fact]
    public async Task MonthlyEvolution_Uses_Historical_And_Current_Month_Windows_And_CarryForward()
    {
        var currentYear = DateTime.UtcNow.Year;
        var historicalYear = currentYear - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(historicalYear, 1, 8),
            "Historical salary",
            new Split(bank.Id, 12_000, "Deposit"),
            new Split(salary.Id, -12_000, "Income credit"));

        var historicalResponse = await client.GetAsync(
            $"/api/v1/reports/monthly-evolution?year={historicalYear}&scope=asset-total");
        historicalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var historical = await historicalResponse.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        historical.Should().NotBeNull();

        var historicalSeries = historical!.Series.Single();
        historicalSeries.Points.Should().HaveCount(12);
        historicalSeries.Points.Single(p => p.Month == 1).EndBalanceCents.Should().Be(12_000);
        historicalSeries.Points.Single(p => p.Month == 2).EndBalanceCents.Should().Be(12_000);
        historicalSeries.Points.Single(p => p.Month == 2).DeltaVsPreviousMonthCents.Should().Be(0);

        var currentResponse = await client.GetAsync(
            $"/api/v1/reports/monthly-evolution?year={currentYear}&scope=asset-total");
        currentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var current = await currentResponse.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        current.Should().NotBeNull();
        current!.Series.Single().Points.Should().HaveCount(12);
    }

    [Fact]
    public async Task MonthlyEvolution_Returns_BadRequest_For_Missing_Or_Invalid_Query_Parameters()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        (await client.GetAsync("/api/v1/reports/monthly-evolution"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-evolution?year={DateTime.UtcNow.Year}"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync("/api/v1/reports/monthly-evolution?scope=asset-total"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-evolution?year={DateTime.UtcNow.Year}&scope=invalid"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.GetAsync($"/api/v1/reports/monthly-evolution?year={DateTime.UtcNow.Year + 1}&scope=asset-total"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MonthlyEvolution_Uses_Snapshot_Baseline_And_Falls_Back_When_Snapshot_Is_Missing()
    {
        var selectedYear = DateTime.UtcNow.Year;
        var baselineYear = selectedYear - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var cash = await TestHelpers.CreateAccountAsync(client, "Cash Wallet", "Asset", "Cash");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(baselineYear, 6, 10),
            "Bank baseline",
            new Split(bank.Id, 10_000, "Asset increase"),
            new Split(salary.Id, -10_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(baselineYear, 9, 10),
            "Cash baseline",
            new Split(cash.Id, 7_000, "Asset increase"),
            new Split(salary.Id, -7_000, "Income credit"));

        var closeResponse = await client.PostAsync($"/api/v1/fiscal-years/{baselineYear}/close", null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
            var cashSnapshot = await db.AccountYearSnapshots
                .FirstOrDefaultAsync(x => x.Year == baselineYear && x.AccountId == new AccountId(cash.Id));

            cashSnapshot.Should().NotBeNull();
            db.AccountYearSnapshots.Remove(cashSnapshot!);
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync(
            $"/api/v1/reports/monthly-evolution?year={selectedYear}&scope=asset-total");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        dto.Should().NotBeNull();

        var firstMonth = dto!.Series.Single().Points.Single(p => p.Month == 1);
        firstMonth.EndBalanceCents.Should().Be(17_000);
        firstMonth.DeltaVsPreviousMonthCents.Should().Be(0);
        firstMonth.DeltaVsYearStartCents.Should().Be(0);
    }

    [Fact]
    public async Task Reporting_Stock_And_Flow_Metrics_Are_Not_Equivalent()
    {
        var year = DateTime.UtcNow.Year - 1;

        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var ownerEquity = await TestHelpers.CreateAccountAsync(client, "Owner Equity", "Equity", "Other");

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 2),
            "Owner contribution",
            new Split(bank.Id, 10_000, "Asset increase"),
            new Split(ownerEquity.Id, -10_000, "Equity credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 5),
            "Salary",
            new Split(bank.Id, 20_000, "Asset increase"),
            new Split(salary.Id, -20_000, "Income credit"));

        await PostTransactionAsync(
            client,
            new DateOnly(year, 1, 10),
            "Groceries",
            new Split(bank.Id, -5_000, "Asset decrease"),
            new Split(groceries.Id, 5_000, "Expense debit"));

        var monthlySummaryResponse = await client.GetAsync(
            $"/api/v1/reports/monthly-summary?from={year}-01-01&to={year}-02-01");
        monthlySummaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var monthlySummary = await monthlySummaryResponse.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        monthlySummary.Should().NotBeNull();

        var monthlyEvolutionResponse = await client.GetAsync(
            $"/api/v1/reports/monthly-evolution?year={year}&scope=asset-total");
        monthlyEvolutionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var evolution = await monthlyEvolutionResponse.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>();
        evolution.Should().NotBeNull();

        var januaryPoint = evolution!.Series.Single().Points.Single(p => p.Month == 1);

        monthlySummary!.Net.Should().Be(15_000);
        januaryPoint.DeltaVsPreviousMonthCents.Should().Be(25_000);
        januaryPoint.DeltaVsPreviousMonthCents.Should().NotBe(monthlySummary.Net);
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

    private sealed record MonthlySummaryDto(
        DateOnly From,
        DateOnly To,
        long IncomeTotal,
        long ExpenseTotal,
        long Net,
        int TransactionsCount);

    private sealed record MonthlyEvolutionReportDto(
        int Year,
        int Scope,
        List<MonthlyEvolutionSeriesDto> Series);

    private sealed record MonthlyEvolutionSeriesDto(
        string SeriesKey,
        string DisplayName,
        Guid? EntityId,
        string? EntityType,
        List<MonthlyEvolutionPointDto> Points);

    private sealed record MonthlyEvolutionPointDto(
        int Month,
        DateOnly MonthEndDate,
        long EndBalanceCents,
        long DeltaVsPreviousMonthCents,
        long DeltaVsYearStartCents);
}
