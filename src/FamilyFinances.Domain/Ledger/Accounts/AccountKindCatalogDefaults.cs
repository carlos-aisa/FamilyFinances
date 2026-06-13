using System.Linq;

namespace FamilyFinances.Domain.Ledger.Accounts;

public static class AccountKindCatalogDefaults
{
    public sealed record Definition(AccountKind LegacyKind, AccountNature Nature, string Key, string Name, int SortOrder);

    public static readonly IReadOnlyList<Definition> SystemDefinitions =
    [
        new(AccountKind.Checking, AccountNature.Asset, "checking", "Checking", 10),
        new(AccountKind.Savings, AccountNature.Asset, "savings", "Savings", 20),
        new(AccountKind.CreditCard, AccountNature.Liability, "credit-card", "Credit Card", 30),
        new(AccountKind.Cash, AccountNature.Asset, "cash", "Cash", 40),
        new(AccountKind.Investment, AccountNature.Asset, "investment", "Investment", 50),
        new(AccountKind.ExpenseCategory, AccountNature.Expense, "expense-category", "Expense Category", 60),
        new(AccountKind.IncomeSource, AccountNature.Income, "income-source", "Income Source", 70),
        new(AccountKind.Mortgage, AccountNature.Liability, "mortgage", "Mortgage", 80),
        new(AccountKind.Loan, AccountNature.Liability, "loan", "Loan", 90),
        new(AccountKind.Other, AccountNature.Equity, "other", "Other", 100)
    ];

    public static string GetKey(AccountKind kind)
        => SystemDefinitions.First(d => d.LegacyKind == kind).Key;

    public static bool IsCompatible(AccountNature accountNature, AccountKindCatalog kind)
    {
        if (kind.IsSystem)
        {
            return kind.LegacyKind switch
            {
                AccountKind.Cash or AccountKind.Checking or AccountKind.Savings or AccountKind.Investment => accountNature == AccountNature.Asset,
                AccountKind.CreditCard or AccountKind.Loan or AccountKind.Mortgage => accountNature == AccountNature.Liability,
                AccountKind.ExpenseCategory => accountNature == AccountNature.Expense,
                AccountKind.IncomeSource => accountNature == AccountNature.Income,
                AccountKind.Other => true,
                _ => false
            };
        }

        return kind.Nature == accountNature;
    }
}