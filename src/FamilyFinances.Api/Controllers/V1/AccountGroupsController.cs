using Asp.Versioning;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Handlers;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/account-groups")]
public sealed class AccountGroupsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanWrite)] 
    public async Task<ActionResult<AccountGroupDto>> Create(
        [FromBody] CreateAccountGroupRequest request,
        [FromServices] CreateAccountGroupHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(request, ct);
        return Ok(dto);
    }

    [HttpGet]
    [Authorize(Policy = Policies.CanRead)] 
    public async Task<ActionResult<IReadOnlyList<AccountGroupDto>>> List(
        [FromServices] ListAccountGroupsHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(ct);
        return Ok(dto);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.CanRead)] 
    public async Task<ActionResult<AccountGroupDetailsDto>> GetById(
        [FromRoute] Guid id,
        [FromServices] GetAccountGroupByIdHandler handler,
        CancellationToken ct)
    {
        var dto = await handler.HandleAsync(id, ct);
        return Ok(dto);
    }

    [HttpPost("{id:guid}/accounts/{accountId:guid}")]
    [Authorize(Policy = Policies.CanWrite)] 
    public async Task<IActionResult> AddAccount(
        [FromRoute] Guid id,
        [FromRoute] Guid accountId,
        [FromServices] AddAccountToGroupHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new AddAccountToGroupRequest(id, accountId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/accounts/{accountId:guid}")]
    [Authorize(Policy = Policies.CanWrite)] 
    public async Task<IActionResult> RemoveAccount(
        [FromRoute] Guid id,
        [FromRoute] Guid accountId,
        [FromServices] RemoveAccountFromGroupHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RemoveAccountFromGroupRequest(id, accountId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/rename")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Rename(
        [FromRoute] Guid id,
        [FromBody] RenameAccountGroupRequest request,
        [FromServices] RenameAccountGroupHandler handler,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromServices] DeleteAccountGroupHandler handler,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
