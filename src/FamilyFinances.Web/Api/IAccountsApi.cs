using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Web.Api;

public interface IAccountsApi
{
    Task<IReadOnlyList<AccountDto>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<AccountBalanceDto>> GetBalancesAsync(CancellationToken ct);
    Task<AccountMovementsDto> GetMovementsAsync(
        Guid accountId, 
        DateOnly? fromInclusive = null, 
        DateOnly? toExclusive = null, 
        string? searchQuery = null, 
        int page = 1, 
        int pageSize = 50, 
        CancellationToken ct = default);
    Task<AccountDto> CreateAsync(CreateAccountRequest requestBody, CancellationToken ct);
    Task RenameAsync(Guid accountId, string name, CancellationToken ct);
    Task CloseAsync(Guid accountId, CancellationToken ct);
    Task ReopenAsync(Guid accountId, CancellationToken ct);
    Task DeleteAsync(Guid accountId, CancellationToken ct);
}

