namespace FamilyFinances.Application.Ledger.AccountGroups.Requests
{
    public sealed record AddAccountToGroupRequest(
        Guid GroupId,
        Guid AccountId
    );
}
