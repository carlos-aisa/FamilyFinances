using FamilyFinances.Application.Abstractions;

namespace FamilyFinances.Application.Accounts;

public sealed class ListAccountsHandler
{
    private readonly IAccountRepository _accounts;

    public ListAccountsHandler(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<IReadOnlyList<AccountDto>> HandleAsync(CancellationToken ct)
    {
        var list = await _accounts.ListAsync(ct);

        return list
            .Select(a => new AccountDto(
                a.Id.Value, a.Name, a.Nature, a.Kind, a.OpenedOn, a.IsClosed, a.ClosedOn))
            .ToList();
    }
}
