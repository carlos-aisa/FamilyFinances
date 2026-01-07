using Asp.Versioning;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromServices] CreateTransactionHandler handler,
        [FromBody] CreateTransactionRequest command,
        CancellationToken ct)
        => Ok(await handler.HandleAsync(command, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<TransactionDto>> GetById(
        [FromServices] GetTransactionByIdHandler handler,
        Guid id,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> List(
    [FromServices] ListTransactionsHandler handler,
    [FromQuery] int take,
    CancellationToken ct)
    => Ok(await handler.HandleAsync(take, ct));
}
