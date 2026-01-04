using Asp.Versioning;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Application.Ledger.Payees.Handlers;
using FamilyFinances.Application.Ledger.Payees.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/payees")]
public sealed class PayeesController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<PayeeDto>> Create(
        [FromServices] CreatePayeeHandler handler,
        [FromBody] CreatePayeeRequest command,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(command, ct);

        // We return the trimmed name to match Domain normalization behavior.
        var name = (command.Name ?? string.Empty).Trim();

        return Ok(new PayeeDto(id.Value, name));
    }

    [HttpGet]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<IReadOnlyList<PayeeDto>>> List(
        [FromServices] ListPayeesHandler handler,
        CancellationToken ct)
    {
        var payees = await handler.HandleAsync(new ListPayeesRequest(), ct);

        var result = payees
            .Select(p => new PayeeDto(p.Id.Value, p.Name))
            .ToList()
            .AsReadOnly();

        return Ok(result);
    }
}
