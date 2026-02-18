using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class AssetTotalBalanceTests
{
    [Fact]
    public async Task AssetTotalBalance_Returns_Correct_Total_And_AccountCount_For_AsOf_Date()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var cash = await TestHelpers.CreateAccountAsync(client, "Cash Wallet", "Asset", "Cash");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary payment",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 100_000, memo = "Income in bank" },
                new { accountId = salary.Id, amountCents = -100_000, memo = "Income source" }
            }
        })).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "ATM withdrawal",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -20_000, memo = "Cash withdrawal" },
                new { accountId = cash.Id, amountCents = 20_000, memo = "Cash wallet increase" }
            }
        })).EnsureSuccessStatusCode();

        // Must be excluded (after asOf)
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-02-01",
            description = "February salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 50_000, memo = "Income in bank" },
                new { accountId = salary.Id, amountCents = -50_000, memo = "Income source" }
            }
        })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/v1/reports/asset-total-balance?asOf=2026-01-31");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<AssetTotalBalanceDto>();
        dto.Should().NotBeNull();

        dto!.AsOf.Should().Be(new DateOnly(2026, 1, 31));
        dto.TotalCents.Should().Be(100_000);
        dto.AssetAccountsCount.Should().Be(2);
    }

    [Fact]
    public async Task AssetTotalBalance_AsOf_Date_Is_Inclusive()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-31",
            description = "Month-end income",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 1_000, memo = "Asset increase" },
                new { accountId = salary.Id, amountCents = -1_000, memo = "Income credit" }
            }
        })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/v1/reports/asset-total-balance?asOf=2026-01-31");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<AssetTotalBalanceDto>();
        dto.Should().NotBeNull();
        dto!.TotalCents.Should().Be(1_000);
    }

    [Fact]
    public async Task AssetTotalBalance_Returns_Zero_When_No_Asset_Splits_Exist()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create account but no transactions.
        await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");

        var response = await client.GetAsync("/api/v1/reports/asset-total-balance?asOf=2026-01-31");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<AssetTotalBalanceDto>();
        dto.Should().NotBeNull();
        dto!.TotalCents.Should().Be(0);
        dto.AssetAccountsCount.Should().Be(0);
    }

    [Fact]
    public async Task AssetTotalBalance_Without_AsOf_Returns_BadRequest()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync("/api/v1/reports/asset-total-balance");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record AssetTotalBalanceDto(
        DateOnly AsOf,
        long TotalCents,
        int AssetAccountsCount
    );
}
