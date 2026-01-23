using Asp.Versioning;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/accounts")]
public sealed class AccountsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<AccountDto>> Create(
        [FromServices] CreateAccountHandler handler,
        [FromBody] CreateAccountRequest command,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(command, ct));

    [HttpGet]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> List(
        [FromServices] ListAccountsHandler handler,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(ct));

    [HttpGet("balances")]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<IReadOnlyList<AccountBalanceDto>>> GetBalances(
        [FromServices] IReportingReadRepository reportingRepo,
        CancellationToken ct)
        => Ok(await reportingRepo.GetAccountBalancesAsync(ct));

    [HttpGet("{id:guid}/movements")]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<AccountMovementsDto>> GetMovements(
        [FromRoute] Guid id,
        [FromServices] IReportingReadRepository reportingRepo,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        // Default date range: current month
        var fromDate = !string.IsNullOrWhiteSpace(from) 
            ? DateOnly.Parse(from) 
            : new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        
        var toDate = !string.IsNullOrWhiteSpace(to) 
            ? DateOnly.Parse(to) 
            : fromDate.AddMonths(1);

        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 100) pageSize = 100;

        var skip = (page - 1) * pageSize;

        try
        {
            var result = await reportingRepo.GetAccountMovementsAsync(
                id, fromDate, toDate, q, skip, pageSize, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/reconcile")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<ReconcileAccountResponse>> Reconcile(
        [FromRoute] Guid id,
        [FromBody] ReconcileAccountRequest request,
        [FromServices] ReconcileAccountHandler handler,
        CancellationToken ct)
    {
        try
        {
            var result = await handler.HandleAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/rename")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Rename(
        [FromRoute] Guid id,
        [FromBody] RenameAccountRequest request,
        [FromServices] RenameAccountHandler handler,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/close")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Close(
        [FromRoute] Guid id,
        [FromServices] CloseAccountHandler handler,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/reopen")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Reopen(
        [FromRoute] Guid id,
        [FromServices] ReopenAccountHandler handler,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Delete(
    [FromRoute] Guid id,
    [FromServices] DeleteAccountHandler handler,
    CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
