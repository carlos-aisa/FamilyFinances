using Asp.Versioning;
using FamilyFinances.Application.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/accounts")]
public sealed class AccountsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<AccountDto>> Create(
        [FromServices] CreateAccountHandler handler,
        [FromBody] CreateAccountCommand command,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(command, ct));

    [HttpGet]
    [Authorize(Policy = "CanRead")]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> List(
        [FromServices] ListAccountsHandler handler,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(ct));
}
