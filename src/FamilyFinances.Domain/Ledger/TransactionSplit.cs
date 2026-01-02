using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger;

public sealed class TransactionSplit
{
    public TransactionSplitId Id { get; }
    public AccountId AccountId { get; }
    public Money Amount { get; }
    public string? Memo { get; }

#pragma warning disable CS8618
    private TransactionSplit() { } // For EF Core
#pragma warning restore CS8618

    private TransactionSplit(TransactionSplitId id, AccountId accountId, Money amount, string? memo)
    {
        Id = id;
        AccountId = accountId;
        Amount = amount;
        Memo = string.IsNullOrWhiteSpace(memo) ? null : memo.Trim();
    }

    public static TransactionSplit Create(AccountId accountId, Money amount, string? memo = null)
    {
        if (accountId.Value == Guid.Empty)
            throw new DomainException("Split AccountId is required.");

        return new TransactionSplit(TransactionSplitId.New(), accountId, amount, memo);
    }
}
