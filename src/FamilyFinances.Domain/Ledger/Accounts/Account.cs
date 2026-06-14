using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Accounts;

public sealed class Account
{
    private AccountKind _legacyKind = AccountKind.Other;

    public AccountId Id { get; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    // Accounting semantics
    public AccountNature Nature { get; }

    // User-facing classification
    public AccountKindCatalogId KindId { get; private set; }
    public AccountKindCatalog KindCatalog { get; private set; } = null!;

    // Backward-compatible projection used by existing read surfaces.
    public AccountKind Kind => KindCatalog?.LegacyKind ?? _legacyKind;

    public DateOnly OpenedOn { get; }
    public bool IsClosed { get; private set; }
    public DateOnly? ClosedOn { get; private set; }

    // Single-currency ledger (EUR)
    public Currency Currency => Currency.EUR;

#pragma warning disable CS8618
    private Account() { } // For EF Core
#pragma warning restore CS8618
    
    private Account(
        AccountId id,
        string name,
        string normalized,
        AccountNature nature,
        AccountKindCatalogId kindId,
        AccountKind legacyKind,
        DateOnly openedOn)
    {
        Id = id;
        Name = name;
        NormalizedName = normalized;
        Nature = nature;
        KindId = kindId;
        _legacyKind = legacyKind;
        OpenedOn = openedOn;
    }

    public static Account Create(
        string name,
        AccountNature nature,
        AccountKindCatalogId kindId,
        AccountKind legacyKind,
        DateOnly openedOn)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Account name is required.");
        
        if (name.Length > 200)
            throw new DomainException("Account name is too long (max 200).");

        var normalized = NameNormalizer.Normalize(name);

        if (openedOn == default)
            throw new DomainException("OpenedOn date is required.");

        if (kindId == default)
            throw new DomainException("Account kind is required.");

       return new Account(
            AccountId.New(),
            name,
            normalized,
            nature,
            kindId,
            legacyKind,
            openedOn);
    }

    public static Account Create(
        string name,
        AccountNature nature,
        AccountKind kind,
        DateOnly openedOn)
    {
        var syntheticKindId = new AccountKindCatalogId(
            Guid.Parse($"00000000-0000-0000-0000-{((int)kind).ToString("D12")}"));

        return Create(name, nature, syntheticKindId, kind, openedOn);
    }

    public void AssignKind(AccountKindCatalogId kindId)
    {
        if (kindId == default)
            throw new DomainException("Account kind is required.");

        KindId = kindId;
    }

    public void SetLegacyKind(AccountKind legacyKind)
    {
        _legacyKind = legacyKind;
    }

    public void Rename(string newName)
    {
        newName = (newName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Account name is required.");

        Name = newName;
    }

    public void Close(DateOnly closedOn)
    {
        if (closedOn == default)
            throw new DomainException("ClosedOn date is required.");

        if (closedOn < OpenedOn)
            throw new DomainException("ClosedOn cannot be earlier than OpenedOn.");

        IsClosed = true;
        ClosedOn = closedOn;
    }

    public void Reopen()
    {
        IsClosed = false;
        ClosedOn = null;
    }
}
