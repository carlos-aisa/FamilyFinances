using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;

namespace FamilyFinances.Web.Api;

public interface IAccountsApi
{
    Task<IReadOnlyList<AccountDto>> ListAsync(CancellationToken ct);
    Task<AccountDto> CreateAsync(CreateAccountRequest requestBody, CancellationToken ct);
    Task RenameAsync(Guid accountId, string name, CancellationToken ct);
    Task CloseAsync(Guid accountId, CancellationToken ct);
    Task ReopenAsync(Guid accountId, CancellationToken ct);
}
