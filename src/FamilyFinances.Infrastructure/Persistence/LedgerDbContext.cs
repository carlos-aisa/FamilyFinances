using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;
using FamilyFinances.Infrastructure.Persistence.Models;
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
    public DbSet<Payee> Payees => Set<Payee>();
    public DbSet<AccountGroup> AccountGroups => Set<AccountGroup>();
    public DbSet<AccountGroupMember> AccountGroupMembers => Set<AccountGroupMember>();
    public DbSet<FiscalYearClosure> FiscalYearClosures => Set<FiscalYearClosure>();
    public DbSet<AccountYearSnapshot> AccountYearSnapshots => Set<AccountYearSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
