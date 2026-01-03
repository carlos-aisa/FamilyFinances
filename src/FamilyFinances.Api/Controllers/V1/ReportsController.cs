using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? payeeId,
        [FromServices] GetMonthlySummaryHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(
            new GetMonthlySummaryQuery(year, month, accountId, payeeId),
            ct);

        return Ok(dto);
    }

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
}
