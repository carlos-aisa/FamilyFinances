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
    [HttpGet("any")]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<object>> HasAny(
        [FromServices] HasAnyTransactionHandler handler,
        CancellationToken ct)
    {
        var hasAny = await handler.HandleAsync(ct);
        return Ok(new { hasAny });
    }

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
    public async Task<ActionResult<IReadOnlyList<TransactionListItemDto>>> List(
        [FromServices] ListTransactionsHandler handler,
        [FromQuery] int take,
        CancellationToken ct)
    => Ok(await handler.HandleAsync(take, ct));

    [HttpGet("search-expenses")]
    [Authorize(Policy = Policies.CanRead)]
    public async Task<ActionResult<IReadOnlyList<ExpenseSearchResultDto>>> SearchExpenses(
        [FromServices] SearchExpensesHandler handler,
        [FromQuery] string q,
        [FromQuery] Guid? expenseAccountId,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var results = await handler.HandleAsync(q, expenseAccountId, limit, ct);
        return Ok(results);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromServices] UpdateTransactionHandler _handler,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken ct)
    {
        var ok = await _handler.HandleAsync(request, ct);

        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.CanWrite)]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteTransactionHandler handler,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await handler.HandleAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
