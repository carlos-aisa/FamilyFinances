using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Payees;

public sealed class Payee
{
    public PayeeId Id { get; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    // v0.3.0: keep as string to avoid introducing a Category model early (v0.4.0).
    public string? DefaultCategory { get; private set; }

#pragma warning disable CS8618
    private Payee() { } // For EF Core
#pragma warning restore CS8618

    private Payee(PayeeId id, string name, string normalizedName, string? defaultCategory)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        DefaultCategory = defaultCategory;
    }

    public static Payee Create(string name, string? defaultCategory = null)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Payee name is required.");

        if (name.Length > 200)
            throw new DomainException("Payee name is too long (max 200).");

        defaultCategory = string.IsNullOrWhiteSpace(defaultCategory) ? null : defaultCategory.Trim();
        if (defaultCategory is not null && defaultCategory.Length > 100)
            throw new DomainException("DefaultCategory is too long (max 100).");

        var normalized = Normalize(name);

        return new Payee(PayeeId.New(), name, normalized, defaultCategory);
    }

    public void Rename(string newName)
    {
        newName = (newName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Payee name is required.");

        if (newName.Length > 200)
            throw new DomainException("Payee name is too long (max 200).");

        Name = newName;
        NormalizedName = Normalize(newName);
    }

    public void SetDefaultCategory(string? category)
    {
        category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        if (category is not null && category.Length > 100)
            throw new DomainException("DefaultCategory is too long (max 100).");

        DefaultCategory = category;
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();
}
