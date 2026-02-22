using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FamilyFinances.Application.Reporting.Dtos;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class ReportingInsightsTests
{
    [Fact]
    public async Task ParetoInsights_Returns_Group_And_Payee_Dimensions()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var rent = await TestHelpers.CreateAccountAsync(client, "Rent", "Expense", "Other");

        var foodGroup = await CreateGroupAsync(client, "Food");
        var housingGroup = await CreateGroupAsync(client, "Housing");
        await AddAccountToGroupAsync(client, foodGroup.Id, groceries.Id);
        await AddAccountToGroupAsync(client, housingGroup.Id, rent.Id);

        var employer = await CreatePayeeAsync(client, "Employer");
        var mercadona = await CreatePayeeAsync(client, "Mercadona");
        var landlord = await CreatePayeeAsync(client, "Landlord");

        await PostTransactionAsync(
            client,
            "2026-01-05",
            "Salary",
            employer.Id,
            new Split(bank.Id, 200_000),
            new Split(salary.Id, -200_000));

        await PostTransactionAsync(
            client,
            "2026-01-10",
            "Groceries",
            mercadona.Id,
            new Split(bank.Id, -30_000),
            new Split(groceries.Id, 30_000));

        await PostTransactionAsync(
            client,
            "2026-01-15",
            "Rent",
            landlord.Id,
            new Split(bank.Id, -70_000),
            new Split(rent.Id, 70_000));

        var byGroup = await client.GetAsync("/api/v1/reports/insights/pareto?from=2026-01-01&to=2026-02-01&dimension=group&topN=5");
        byGroup.StatusCode.Should().Be(HttpStatusCode.OK);

        var byGroupDto = await byGroup.Content.ReadFromJsonAsync<ReportingParetoInsightsDto>();
        byGroupDto.Should().NotBeNull();
        byGroupDto!.Expense.TotalAmountCents.Should().Be(100_000);
        byGroupDto.Expense.Contributors.Select(x => x.DisplayName).Should().Contain(["Housing", "Food"]);

        var byPayee = await client.GetAsync("/api/v1/reports/insights/pareto?from=2026-01-01&to=2026-02-01&dimension=payee&topN=5");
        byPayee.StatusCode.Should().Be(HttpStatusCode.OK);

        var byPayeeDto = await byPayee.Content.ReadFromJsonAsync<ReportingParetoInsightsDto>();
        byPayeeDto.Should().NotBeNull();
        byPayeeDto!.Expense.TotalAmountCents.Should().Be(100_000);
        byPayeeDto.Expense.Contributors.Select(x => x.DisplayName).Should().Contain(["Landlord", "Mercadona"]);
    }

    [Fact]
    public async Task ParetoInsights_Rejects_PayeeFilter_When_Dimension_Is_Payee()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var payee = await CreatePayeeAsync(client, "Mercadona");

        var response = await client.GetAsync(
            $"/api/v1/reports/insights/pareto?from=2026-01-01&to=2026-02-01&dimension=payee&payeeId={payee.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AnomalyInsights_Returns_InsufficientHistory_State()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var payee = await CreatePayeeAsync(client, "Mercadona");

        await PostTransactionAsync(
            client,
            "2026-01-10",
            "January groceries",
            payee.Id,
            new Split(bank.Id, -10_000),
            new Split(groceries.Id, 10_000));

        await PostTransactionAsync(
            client,
            "2026-02-10",
            "February groceries",
            payee.Id,
            new Split(bank.Id, -40_000),
            new Split(groceries.Id, 40_000));

        var response = await client.GetAsync(
            "/api/v1/reports/insights/anomalies?year=2026&month=2&nature=Expense&dimension=payee&lookbackMonths=12&requiredHistoryMonths=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<ReportingAnomalyInsightsDto>();
        dto.Should().NotBeNull();
        dto!.Contributors.Should().ContainSingle();
        dto.Contributors[0].DisplayName.Should().Be("Mercadona");
        dto.Contributors[0].IsInsufficientHistory.Should().BeTrue();
        dto.Contributors[0].IsAnomaly.Should().BeFalse();
    }

    private static async Task<AccountGroupDto> CreateGroupAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/account-groups", new { name, description = $"{name} group" });
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

    private static async Task<PayeeDto> CreatePayeeAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/payees", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<PayeeDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private static async Task PostTransactionAsync(
        HttpClient client,
        string bookedOn,
        string description,
        Guid? payeeId,
        params Split[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn,
            description,
            payeeId,
            splits = splits.Select(x => new
            {
                accountId = x.AccountId,
                amountCents = x.AmountCents,
                memo = "insight test"
            })
        });

        response.EnsureSuccessStatusCode();
    }

    private sealed record Split(Guid AccountId, long AmountCents);
    private sealed record AccountGroupDto(Guid Id, string Name, string? Description);
    private sealed record PayeeDto(Guid Id, string Name);
}
