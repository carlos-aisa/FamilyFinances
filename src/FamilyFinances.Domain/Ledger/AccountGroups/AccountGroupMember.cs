using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Domain.Ledger.AccountGroups
{
    public sealed class AccountGroupMember
    {
        public AccountGroupId GroupId { get; private set; }
        public AccountId AccountId { get; private set; }

        private AccountGroupMember() { } // EF

        public AccountGroupMember(AccountGroupId groupId, AccountId accountId)
        {
            GroupId = groupId;
            AccountId = accountId;
        }
    }
}
