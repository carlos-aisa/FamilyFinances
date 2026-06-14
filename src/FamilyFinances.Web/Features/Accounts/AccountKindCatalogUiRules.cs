using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Web.Features.Accounts;

public static class AccountKindCatalogUiRules
{
    public static IReadOnlyList<AccountKindCatalogDto> GetAllowedKinds(
        IEnumerable<AccountKindCatalogDto> kinds,
        AccountNature nature,
        Func<AccountKindCatalogDto, string> getLabel)
    {
        var allowedSystemKinds = nature switch
        {
            AccountNature.Asset => new HashSet<AccountKind> { AccountKind.Cash, AccountKind.Checking, AccountKind.Savings, AccountKind.Investment, AccountKind.Other },
            AccountNature.Liability => new HashSet<AccountKind> { AccountKind.CreditCard, AccountKind.Loan, AccountKind.Mortgage, AccountKind.Other },
            AccountNature.Expense => new HashSet<AccountKind> { AccountKind.ExpenseCategory, AccountKind.Other },
            AccountNature.Income => new HashSet<AccountKind> { AccountKind.IncomeSource, AccountKind.Other },
            AccountNature.Equity => new HashSet<AccountKind> { AccountKind.Other },
            _ => new HashSet<AccountKind> { AccountKind.Other }
        };

        return kinds
            .Where(x => x.IsActive)
            .Where(x => x.IsSystem
                ? allowedSystemKinds.Contains(x.LegacyKind)
                : x.Nature == nature)
            .OrderBy(getLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.IsSystem ? 0 : 1)
            .ToList();
    }

    public static AccountKindCatalogDto? GetDefaultKind(
        IEnumerable<AccountKindCatalogDto> kinds,
        AccountNature nature,
        Func<AccountKindCatalogDto, string> getLabel)
    {
        var allowed = GetAllowedKinds(kinds, nature, getLabel);
        return nature switch
        {
            AccountNature.Asset => allowed.FirstOrDefault(x => x.LegacyKind == AccountKind.Checking) ?? allowed.FirstOrDefault(),
            AccountNature.Liability => allowed.FirstOrDefault(x => x.LegacyKind == AccountKind.CreditCard) ?? allowed.FirstOrDefault(),
            AccountNature.Expense => allowed.FirstOrDefault(x => x.LegacyKind == AccountKind.ExpenseCategory) ?? allowed.FirstOrDefault(),
            AccountNature.Income => allowed.FirstOrDefault(x => x.LegacyKind == AccountKind.IncomeSource) ?? allowed.FirstOrDefault(),
            _ => allowed.FirstOrDefault()
        };
    }
}
