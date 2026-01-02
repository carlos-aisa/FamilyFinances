using Asp.Versioning;
using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Transactions;
using FamilyFinances.Application.Ledger.Transactions.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromServices] CreateTransactionHandler handler,
        [FromBody] CreateTransactionCommand command,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(command, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanRead")]
    public async Task<ActionResult<TransactionDto>> GetById(
        [FromServices] GetTransactionByIdHandler handler,
        Guid id,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
