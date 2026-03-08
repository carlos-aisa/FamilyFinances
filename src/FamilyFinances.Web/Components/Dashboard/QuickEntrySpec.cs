using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Web.Components.Dashboard
{
    public sealed record QuickEntrySpec(
            string Title,
            string Hint,
            string? Guidance,
            HashSet<AccountNature> FromAllowed,
            HashSet<AccountNature> ToAllowed,
            bool ReplaceToWhenBothSelected);
}
