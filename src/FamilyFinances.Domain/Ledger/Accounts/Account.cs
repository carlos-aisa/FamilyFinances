using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Accounts;

public sealed class Account
{
    public AccountId Id { get; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    // Accounting semantics
    public AccountNature Nature { get; }

    // User-facing classification
    public AccountKind Kind { get; }

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
        AccountKind kind,
        DateOnly openedOn)
    {
        Id = id;
        Name = name;
        NormalizedName = normalized;
        Nature = nature;
        Kind = kind;
        OpenedOn = openedOn;
    }

    public static Account Create(
        string name,
        AccountNature nature,
        AccountKind kind,
        DateOnly openedOn)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Account name is required.");
        
        if (name.Length > 200)
            throw new DomainException("Account name is too long (max 200).");

        var normalized = Normalize(name);

        if (openedOn == default)
            throw new DomainException("OpenedOn date is required.");

       return new Account(
            AccountId.New(),
            name,
            normalized,
            nature,
            kind,
            openedOn);
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

    private static string Normalize(string name) => name.Trim().ToUpperInvariant();
}
