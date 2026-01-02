using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence;

public sealed class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionSplit> TransactionSplits => Set<TransactionSplit>();
    public DbSet<TransactionLink> TransactionLinks => Set<TransactionLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
