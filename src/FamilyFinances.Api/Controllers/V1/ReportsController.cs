using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? payeeId,
        [FromServices] GetMonthlySummaryHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(
            new GetMonthlySummaryQuery(from, to, accountId, payeeId),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("insights/pareto")]
    public async Task<ActionResult<ReportingParetoInsightsDto>> GetParetoInsights(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string? dimension,
        [FromQuery] int? topN,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? payeeId,
        [FromServices] GetReportingParetoInsightsHandler handler,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dimension))
            return BadRequest(new { error = "Query parameter 'dimension' is required." });

        if (!TryParseInsightDimension(dimension, out var parsedDimension))
            return BadRequest(new { error = "Query parameter 'dimension' must be one of: group, payee." });

        if (parsedDimension == ReportingInsightDimension.Payee && payeeId is not null)
            return BadRequest(new { error = "Query parameter 'payeeId' is not supported when dimension is 'payee'." });

        var dto = await handler.HandleAsync(
            new GetReportingParetoInsightsQuery(
                from,
                to,
                parsedDimension,
                topN ?? 5,
                accountId,
                payeeId),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("insights/anomalies")]
    public async Task<ActionResult<ReportingAnomalyInsightsDto>> GetAnomalyInsights(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] AccountNature? nature,
        [FromQuery] string? dimension,
        [FromQuery] int? lookbackMonths,
        [FromQuery] int? requiredHistoryMonths,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? payeeId,
        [FromServices] GetReportingAnomalyInsightsHandler handler,
        CancellationToken ct)
    {
        if (year is null)
            return BadRequest(new { error = "Query parameter 'year' is required." });

        if (month is null)
            return BadRequest(new { error = "Query parameter 'month' is required." });

        if (nature is null)
            return BadRequest(new { error = "Query parameter 'nature' is required." });

        if (nature is not AccountNature.Expense and not AccountNature.Income)
            return BadRequest(new { error = "Query parameter 'nature' must be either Expense or Income." });

        if (string.IsNullOrWhiteSpace(dimension))
            return BadRequest(new { error = "Query parameter 'dimension' is required." });

        if (!TryParseInsightDimension(dimension, out var parsedDimension))
            return BadRequest(new { error = "Query parameter 'dimension' must be one of: group, payee." });

        if (parsedDimension == ReportingInsightDimension.Payee && payeeId is not null)
            return BadRequest(new { error = "Query parameter 'payeeId' is not supported when dimension is 'payee'." });

        var dto = await handler.HandleAsync(
            new GetReportingAnomalyInsightsQuery(
                year.Value,
                month.Value,
                nature.Value,
                parsedDimension,
                lookbackMonths ?? 12,
                requiredHistoryMonths ?? 3,
                accountId,
                payeeId),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("category-totals")]
    public async Task<IActionResult> GetCategoryTotals(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] AccountNature nature,
        [FromQuery] Guid? payeeId,
        [FromServices] GetCategoryTotalsHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(
            new GetCategoryTotalsQuery(from, to, nature, payeeId),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("account-totals")]
    public async Task<IActionResult> GetAccountTotals(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] bool includeZeroAccounts,
        [FromServices] GetAccountTotalsHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(
            new GetAccountTotalsQuery(from, to, includeZeroAccounts),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("account-groups/{groupId:guid}/totals")]
    public async Task<ActionResult<AccountGroupTotalsDto>> GetAccountGroupTotals(
        [FromRoute] Guid groupId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] AccountNature? nature,
        [FromServices] GetAccountGroupTotalsHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(
            new GetAccountGroupTotalsQuery(groupId, from, to, nature),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("asset-total-balance")]
    public async Task<ActionResult<AssetTotalBalanceDto>> GetAssetTotalBalance(
        [FromQuery] DateOnly? asOf,
        [FromServices] GetAssetTotalBalanceHandler handler,
        CancellationToken ct)
    {
        if (asOf is null)
            return BadRequest(new { error = "Query parameter 'asOf' is required." });

        var dto = await handler.HandleAsync(
            new GetAssetTotalBalanceQuery(asOf.Value),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("state-evolution")]
    public Task<ActionResult<MonthlyEvolutionReportDto>> GetStateEvolution(
        [FromQuery] int? year,
        [FromQuery] string? scope,
        [FromServices] GetMonthlyEvolutionHandler handler,
        CancellationToken ct)
    {
        return GetStateEvolutionCore(year, scope, handler, ct);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("dashboard-overview")]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromServices] GetDashboardOverviewHandler handler,
        CancellationToken ct)
    {
        DateOnly asOf;
        if (year is null && month is null)
        {
            asOf = DateOnly.FromDateTime(DateTime.Today);
        }
        else
        {
            if (year is null || month is null)
                return BadRequest(new { error = "Query parameters 'year' and 'month' must be provided together." });

            var currentYear = DateTime.Today.Year;
            if (year < 2000 || year > currentYear)
                return BadRequest(new { error = $"Query parameter 'year' must be between 2000 and {currentYear}." });

            if (month is < 1 or > 12)
                return BadRequest(new { error = "Query parameter 'month' must be between 1 and 12." });

            var targetYear = year.Value;
            var targetMonth = month.Value;
            if (targetYear == DateTime.Today.Year && targetMonth == DateTime.Today.Month)
            {
                asOf = DateOnly.FromDateTime(DateTime.Today);
            }
            else
            {
                asOf = new DateOnly(targetYear, targetMonth, DateTime.DaysInMonth(targetYear, targetMonth));
            }
        }

        var dto = await handler.HandleAsync(new GetDashboardOverviewQuery(asOf), ct);
        return Ok(dto);
    }

    // Legacy alias kept for compatibility.
    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("monthly-evolution")]
    public Task<ActionResult<MonthlyEvolutionReportDto>> GetMonthlyEvolution(
        [FromQuery] int? year,
        [FromQuery] string? scope,
        [FromServices] GetMonthlyEvolutionHandler handler,
        CancellationToken ct)
    {
        return GetStateEvolutionCore(year, scope, handler, ct);
    }

    private async Task<ActionResult<MonthlyEvolutionReportDto>> GetStateEvolutionCore(
        int? year,
        string? scope,
        GetMonthlyEvolutionHandler handler,
        CancellationToken ct)
    {
        if (year is null)
            return BadRequest(new { error = "Query parameter 'year' is required." });

        if (string.IsNullOrWhiteSpace(scope))
            return BadRequest(new { error = "Query parameter 'scope' is required." });

        if (!TryParseMonthlyEvolutionScope(scope, out var parsedScope))
            return BadRequest(new { error = "Query parameter 'scope' must be one of: accounts, asset-total, account-groups, income-total, expense-total." });

        var dto = await handler.HandleAsync(
            new GetMonthlyEvolutionQuery(year.Value, parsedScope),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("monthly-charts/balance")]
    public async Task<ActionResult<MonthlyBalanceChartDto>> GetMonthlyBalanceChart(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? payeeId,
        [FromQuery] AccountNature? nature,
        [FromServices] GetMonthlyBalanceChartHandler handler,
        CancellationToken ct)
    {
        if (year is null)
            return BadRequest(new { error = "Query parameter 'year' is required." });

        if (month is null)
            return BadRequest(new { error = "Query parameter 'month' is required." });

        if (accountId is not null && nature is not null)
            return BadRequest(new { error = "Query parameters 'accountId' and 'nature' cannot be used together." });

        var dto = await handler.HandleAsync(
            new GetMonthlyBalanceChartQuery(year.Value, month.Value, accountId, payeeId, nature),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("monthly-charts/group-evolution")]
    public Task<ActionResult<MonthlyBalanceVsGroupsChartDto>> GetMonthlyGroupEvolutionChart(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromServices] GetMonthlyBalanceVsGroupsChartHandler handler,
        CancellationToken ct)
    {
        return GetMonthlyGroupEvolutionChartCore(year, month, handler, ct);
    }

    // Legacy alias kept for compatibility.
    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("monthly-charts/balance-vs-groups")]
    public Task<ActionResult<MonthlyBalanceVsGroupsChartDto>> GetMonthlyBalanceVsGroupsChart(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromServices] GetMonthlyBalanceVsGroupsChartHandler handler,
        CancellationToken ct)
    {
        return GetMonthlyGroupEvolutionChartCore(year, month, handler, ct);
    }

    private async Task<ActionResult<MonthlyBalanceVsGroupsChartDto>> GetMonthlyGroupEvolutionChartCore(
        int? year,
        int? month,
        GetMonthlyBalanceVsGroupsChartHandler handler,
        CancellationToken ct)
    {
        if (year is null)
            return BadRequest(new { error = "Query parameter 'year' is required." });

        if (month is null)
            return BadRequest(new { error = "Query parameter 'month' is required." });

        var dto = await handler.HandleAsync(
            new GetMonthlyBalanceVsGroupsChartQuery(year.Value, month.Value),
            ct);

        return Ok(dto);
    }

    [Authorize(Policy = Policies.CanRead)]
    [HttpGet("economic-state")]
    public async Task<ActionResult<EconomicStateDto>> GetEconomicState(
        [FromQuery] DateOnly? asOf,
        [FromServices] GetEconomicStateHandler handler,
        CancellationToken ct)
    {
        if (asOf is null)
            return BadRequest(new { error = "Query parameter 'asOf' is required." });

        var dto = await handler.HandleAsync(
            new GetEconomicStateQuery(asOf.Value),
            ct);

        return Ok(dto);
    }

    private static bool TryParseMonthlyEvolutionScope(string scope, out MonthlyEvolutionScope parsed)
    {
        switch (scope.Trim().ToLowerInvariant())
        {
            case "accounts":
                parsed = MonthlyEvolutionScope.Accounts;
                return true;
            case "asset-total":
                parsed = MonthlyEvolutionScope.AssetTotal;
                return true;
            case "account-groups":
                parsed = MonthlyEvolutionScope.AccountGroups;
                return true;
            case "income-total":
                parsed = MonthlyEvolutionScope.IncomeTotal;
                return true;
            case "expense-total":
                parsed = MonthlyEvolutionScope.ExpenseTotal;
                return true;
            default:
                parsed = default;
                return false;
        }
    }

    private static bool TryParseInsightDimension(string dimension, out ReportingInsightDimension parsed)
    {
        switch (dimension.Trim().ToLowerInvariant())
        {
            case "group":
            case "groups":
                parsed = ReportingInsightDimension.Group;
                return true;
            case "payee":
            case "payees":
                parsed = ReportingInsightDimension.Payee;
                return true;
            default:
                parsed = default;
                return false;
        }
    }
}
