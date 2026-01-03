namespace FamilyFinances.Application.Ledger.AccountGroups.Requests
{
    public sealed record RemoveAccountFromGroupRequest(
        Guid GroupId,
        Guid AccountId
    );
}
