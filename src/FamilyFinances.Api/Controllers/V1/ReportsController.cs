using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
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
}
