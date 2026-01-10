using Asp.Versioning;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
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
