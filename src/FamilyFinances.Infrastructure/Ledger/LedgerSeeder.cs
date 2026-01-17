using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Ledger;

public static class LedgerSeeder
{
    public static async Task EnsureOpeningBalanceAccountAsync(LedgerDbContext context, CancellationToken ct = default)
    {
        const string accountName = "Opening Balance";
        var normalizedName = accountName.ToUpperInvariant();

        var exists = await context.Accounts
            .AnyAsync(a => a.NormalizedName == normalizedName && !a.IsClosed, ct);

        if (!exists)
        {
            var openingBalanceAccount = Account.Create(
                accountName,
                AccountNature.Equity,
                AccountKind.Other,
                DateOnly.FromDateTime(DateTime.UtcNow));

            await context.Accounts.AddAsync(openingBalanceAccount, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}
