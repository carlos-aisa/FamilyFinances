using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Infrastructure.Persistence.Models;

public sealed class AccountYearSnapshot
{
    public int Year { get; set; }
    public AccountId AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public long ClosingBalanceCents { get; set; }
    public DateTime ComputedAtUtc { get; set; }
}
