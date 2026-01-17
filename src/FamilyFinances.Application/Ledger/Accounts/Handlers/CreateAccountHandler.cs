using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;


public sealed class CreateAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public CreateAccountHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<AccountDto> HandleAsync(CreateAccountRequest cmd, CancellationToken ct)
    {
        var normalizedName = NameNormalizer.Normalize(cmd.Name);
        var exists = await _accounts.ExistsByNormalizedNameAsync(normalizedName, excludingId: null, ct);
        if (exists)
            throw new ConflictException("Account name already exists.");

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
