using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Ledger;

public static class LedgerSeeder
{
    public static async Task EnsureAccountKindsAsync(LedgerDbContext context, CancellationToken ct = default)
    {
        var existing = await context.AccountKinds
            .ToDictionaryAsync(x => x.Key, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var definition in AccountKindCatalogDefaults.SystemDefinitions)
        {
            if (existing.ContainsKey(definition.Key))
                continue;

            var entity = AccountKindCatalog.CreateSystem(
                definition.Key,
                definition.Name,
                definition.SortOrder,
                definition.Nature,
                definition.LegacyKind);

            await context.AccountKinds.AddAsync(entity, ct);
        }

        await context.SaveChangesAsync(ct);
    }

    public static async Task EnsureOpeningBalanceAccountAsync(LedgerDbContext context, CancellationToken ct = default)
    {
        const string accountName = "Opening Balance";
        var normalizedName = accountName.ToUpperInvariant();

        var equityKind = await context.AccountKinds
            .FirstOrDefaultAsync(x => x.Key == AccountKindCatalogDefaults.GetKey(AccountKind.Other), ct);

        if (equityKind is null)
            throw new InvalidOperationException("System account kinds are not initialized.");

        var exists = await context.Accounts
            .AnyAsync(a => a.NormalizedName == normalizedName && !a.IsClosed, ct);

        if (!exists)
        {
            var openingBalanceAccount = Account.Create(
                accountName,
                AccountNature.Equity,
                equityKind.Id,
                equityKind.LegacyKind,
                DateOnly.FromDateTime(DateTime.UtcNow));

            await context.Accounts.AddAsync(openingBalanceAccount, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}
