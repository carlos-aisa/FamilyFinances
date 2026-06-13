using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Accounts;

public sealed class AccountKindCatalog
{
    public AccountKindCatalogId Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public AccountNature Nature { get; private set; }
    public AccountKind LegacyKind { get; private set; }

    private AccountKindCatalog() { }

    private AccountKindCatalog(
        AccountKindCatalogId id,
        string key,
        string name,
        bool isSystem,
        bool isActive,
        int sortOrder,
        AccountNature nature,
        AccountKind legacyKind)
    {
        Id = id;
        SetKey(key);
        Rename(name);
        IsSystem = isSystem;
        IsActive = isActive;
        SortOrder = sortOrder;
        Nature = nature;
        LegacyKind = legacyKind;
    }

    public static AccountKindCatalog CreateSystem(
        string key,
        string name,
        int sortOrder,
        AccountNature nature,
        AccountKind legacyKind)
        => new(
            AccountKindCatalogId.New(),
            key,
            name,
            isSystem: true,
            isActive: true,
            sortOrder,
            nature,
            legacyKind);

    public static AccountKindCatalog CreateCustom(
        string key,
        string name,
        int sortOrder,
        AccountNature nature)
        => new(
            AccountKindCatalogId.New(),
            key,
            name,
            isSystem: false,
            isActive: true,
            sortOrder,
            nature,
            legacyKind: AccountKind.Other);

    public void Rename(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new DomainException("Account kind name is required.");

        if (trimmed.Length > 100)
            throw new DomainException("Account kind name is too long (max 100).");

        Name = trimmed;
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new DomainException("Sort order must be greater than or equal to zero.");

        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        if (IsSystem)
            throw new DomainException("System account kinds cannot be deactivated.");

        IsActive = false;
    }

    private void SetKey(string key)
    {
        var trimmed = (key ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new DomainException("Account kind key is required.");

        if (trimmed.Length > 64)
            throw new DomainException("Account kind key is too long (max 64).");

        Key = trimmed;
    }
}