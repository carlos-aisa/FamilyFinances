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
}
