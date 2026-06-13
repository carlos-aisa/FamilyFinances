using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.Extensions.Localization;

namespace FamilyFinances.Web.Features.Accounts;

public static class AccountKindLabelResolver
{
    public static string Resolve(AccountDto account, IStringLocalizer localizer)
    {
        if (IsSystemKind(account.Kind, account.KindKey))
            return ResolveLegacy(account.Kind, localizer);

        if (!string.IsNullOrWhiteSpace(account.KindName))
            return account.KindName;

        return ResolveLegacy(account.Kind, localizer);
    }

    public static string Resolve(AccountKindCatalogDto kind, IStringLocalizer localizer)
    {
        if (kind.IsSystem)
            return ResolveLegacy(kind.LegacyKind, localizer);

        if (!string.IsNullOrWhiteSpace(kind.Name))
            return kind.Name;

        return ResolveLegacy(kind.LegacyKind, localizer);
    }

    public static string ResolveLegacy(AccountKind kind, IStringLocalizer localizer)
        => kind switch
        {
            AccountKind.Cash => localizer["Accounts_Kind_Cash"],
            AccountKind.Checking => localizer["Accounts_Kind_Checking"],
            AccountKind.Savings => localizer["Accounts_Kind_Savings"],
            AccountKind.Investment => localizer["Accounts_Kind_Investment"],
            AccountKind.CreditCard => localizer["Accounts_Kind_CreditCard"],
            AccountKind.Loan => localizer["Accounts_Kind_Loan"],
            AccountKind.Mortgage => localizer["Accounts_Kind_Mortgage"],
            AccountKind.ExpenseCategory => localizer["Accounts_Kind_ExpenseCategory"],
            AccountKind.IncomeSource => localizer["Accounts_Kind_IncomeSource"],
            AccountKind.Other => localizer["Accounts_Kind_Other"],
            _ => kind.ToString()
        };

    private static bool IsSystemKind(AccountKind legacyKind, string kindKey)
    {
        if (string.IsNullOrWhiteSpace(kindKey))
            return false;

        var expectedSystemKey = AccountKindCatalogDefaults.GetKey(legacyKind);
        return string.Equals(expectedSystemKey, kindKey, StringComparison.OrdinalIgnoreCase);
    }
}
