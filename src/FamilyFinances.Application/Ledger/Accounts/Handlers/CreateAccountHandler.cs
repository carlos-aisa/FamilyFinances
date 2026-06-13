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

        var selectedKind = await ResolveSelectedKindAsync(cmd, ct);

        if (!selectedKind.IsActive)
            throw new DomainException("Selected account kind is inactive.");

        if (!AccountKindCatalogDefaults.IsCompatible(cmd.Nature, selectedKind))
            throw new DomainException("Selected account kind is not compatible with account nature.");

        var account = Account.Create(cmd.Name, cmd.Nature, selectedKind.Id, selectedKind.LegacyKind, cmd.OpenedOn);

        await _accounts.AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);

        return new AccountDto(
            account.Id.Value,
            account.Name,
            account.Nature,
            selectedKind.LegacyKind,
            account.OpenedOn,
            account.IsClosed,
            account.ClosedOn,
            selectedKind.Id.Value,
            selectedKind.Key,
            selectedKind.Name);
    }

    private async Task<AccountKindCatalog> ResolveSelectedKindAsync(CreateAccountRequest cmd, CancellationToken ct)
    {
        if (cmd.KindId.HasValue)
        {
            var byId = await _accounts.GetKindByIdAsync(new AccountKindCatalogId(cmd.KindId.Value), ct);
            if (byId is null)
                throw new DomainException("Selected account kind does not exist.");

            return byId;
        }

        var byLegacy = await _accounts.GetKindByLegacyAndNatureAsync(cmd.Kind, cmd.Nature, ct)
            ?? await _accounts.GetKindByLegacyAsync(cmd.Kind, ct);
        if (byLegacy is null)
            throw new DomainException("Selected account kind does not exist.");

        return byLegacy;
    }
}
