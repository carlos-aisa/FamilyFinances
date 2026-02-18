using Asp.Versioning;
using FamilyFinances.Application.Ledger.FiscalYears.Handlers;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/history")]
public sealed class HistoryController : ControllerBase
{
    [HttpGet("transactions")]
    [Authorize(Policy = Policies.CanRead)]
    public Task<IReadOnlyList<TransactionListItemDto>> ListTransactions(
        [FromQuery] int year,
        [FromQuery] int take,
        [FromServices] ListHistoricalTransactionsHandler handler,
        CancellationToken ct)
    {
        var safeTake = take < 1 ? 200 : Math.Min(take, 1000);
        return handler.HandleAsync(new ListHistoricalTransactionsRequest(year, safeTake), ct);
    }

    [HttpGet("movements")]
    [Authorize(Policy = Policies.CanRead)]
    public Task<AccountMovementsDto> GetMovements(
        [FromQuery] Guid accountId,
        [FromQuery] int year,
        [FromQuery] string? q,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] GetHistoricalAccountMovementsHandler handler,
        CancellationToken ct)
    {
        return handler.HandleAsync(
            new GetHistoricalAccountMovementsRequest(
                accountId,
                year,
                q,
                page,
                pageSize),
            ct);
    }
}
