using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class AccountYearSnapshotConfiguration : IEntityTypeConfiguration<AccountYearSnapshot>
{
    public void Configure(EntityTypeBuilder<AccountYearSnapshot> b)
    {
        b.ToTable("AccountYearSnapshots");

        b.HasKey(x => new { x.Year, x.AccountId });

        b.Property(x => x.Year)
            .IsRequired();

        b.Property(x => x.AccountId)
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        b.Property(x => x.ClosingBalanceCents)
            .IsRequired();

        b.Property(x => x.ComputedAtUtc)
            .IsRequired();

        b.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.AccountId, x.Year });
    }
}
