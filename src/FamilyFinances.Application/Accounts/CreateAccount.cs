using FamilyFinances.Application.Abstractions;
using FamilyFinances.Domain.Accounts;

namespace FamilyFinances.Application.Accounts;

public sealed record CreateAccountCommand(
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn);

public sealed class CreateAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public CreateAccountHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<AccountDto> HandleAsync(CreateAccountCommand cmd, CancellationToken ct)
    {
        var account = Account.Create(cmd.Name, cmd.Nature, cmd.Kind, cmd.OpenedOn);

        await _accounts.AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);

        return new AccountDto(
            account.Id.Value,
            account.Name,
            account.Nature,
            account.Kind,
            account.OpenedOn,
            account.IsClosed,
            account.ClosedOn);
    }
}
