using System.Security.Claims;
using Asp.Versioning;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Handlers;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/fiscal-years")]
public sealed class FiscalYearsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Policies.CanRead)]
    public Task<IReadOnlyList<FiscalYearStatusDto>> List(
        [FromServices] ListFiscalYearsHandler handler,
        CancellationToken ct)
    {
        return handler.HandleAsync(ct);
    }

    [HttpPost("{year:int}/close")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<FiscalYearStatusDto>> Close(
        [FromRoute] int year,
        [FromServices] CloseFiscalYearHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new CloseFiscalYearRequest(year, GetActorUserId()),
            ct);

        return Ok(result);
    }

    [HttpPost("{year:int}/reopen")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<FiscalYearStatusDto>> Reopen(
        [FromRoute] int year,
        [FromServices] ReopenFiscalYearHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new ReopenFiscalYearRequest(year, GetActorUserId()),
            ct);

        return Ok(result);
    }

    private string? GetActorUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
    }
}
