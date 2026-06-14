using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Web.Api;

public interface IAccountsApi
{
    Task<IReadOnlyList<AccountDto>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<AccountKindCatalogDto>> ListKindsAsync(bool includeInactive, CancellationToken ct);
    Task<AccountKindCatalogDto> CreateKindAsync(string name, AccountNature nature, CancellationToken ct);
    Task SetKindActiveAsync(Guid kindId, bool isActive, CancellationToken ct);
    Task DeleteKindAsync(Guid kindId, CancellationToken ct);
    Task SetAccountKindAsync(Guid accountId, Guid kindId, CancellationToken ct);
    Task<IReadOnlyList<AccountBalanceDto>> GetBalancesAsync(CancellationToken ct);
    Task<AccountMovementsDto> GetMovementsAsync(
        Guid accountId, 
        DateOnly? fromInclusive = null, 
        DateOnly? toExclusive = null, 
        string? searchQuery = null, 
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int page = 1, 
        int pageSize = 50, 
        CancellationToken ct = default);
    Task<AccountDto> CreateAsync(CreateAccountRequest requestBody, CancellationToken ct);
    Task RenameAsync(Guid accountId, string name, CancellationToken ct);
    Task CloseAsync(Guid accountId, CancellationToken ct);
    Task ReopenAsync(Guid accountId, CancellationToken ct);
    Task<ReconcileAccountResponse> ReconcileAsync(
        Guid accountId,
        decimal actualBalance,
        DateOnly asOfDate,
        string? note,
        CancellationToken ct);
    Task DeleteAsync(Guid accountId, CancellationToken ct);
}

