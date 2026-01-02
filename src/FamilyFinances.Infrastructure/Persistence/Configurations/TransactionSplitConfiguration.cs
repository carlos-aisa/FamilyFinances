using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class TransactionSplitConfiguration : IEntityTypeConfiguration<TransactionSplit>
{
    public void Configure(EntityTypeBuilder<TransactionSplit> builder)
    {
        builder.ToTable("TransactionSplits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new TransactionSplitId(value))
            .ValueGeneratedNever();

        // Shadow FK to Transaction
        builder.Property<TransactionId>("TransactionId")
            .HasConversion(
                id => id.Value,
                value => new TransactionId(value))
            .IsRequired();

        builder.HasIndex("TransactionId");

        builder.Property(x => x.AccountId)
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        // Money is a value object (Cents). Store as long.
        builder.Property(x => x.Amount)
            .HasConversion(
                m => m.Cents,
                cents => new FamilyFinances.Domain.Common.Money(cents))
            .HasColumnName("AmountCents")
            .IsRequired();

        builder.Property(x => x.Memo)
            .HasMaxLength(500);

        builder.HasIndex(x => x.AccountId);
    }
}
