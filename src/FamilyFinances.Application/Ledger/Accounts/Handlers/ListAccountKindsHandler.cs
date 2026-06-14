using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Dtos;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class ListAccountKindsHandler
{
    private readonly IAccountRepository _accounts;

    public ListAccountKindsHandler(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<IReadOnlyList<AccountKindCatalogDto>> HandleAsync(bool includeInactive, CancellationToken ct)
    {
        var kinds = await _accounts.ListKindsAsync(includeInactive, ct);

        return kinds
            .Select(x => new AccountKindCatalogDto(
                x.Id.Value,
                x.Key,
                x.Name,
                x.IsSystem,
                x.IsActive,
                x.SortOrder,
                x.LegacyKind,
                x.Nature))
            .ToList();
    }
}
