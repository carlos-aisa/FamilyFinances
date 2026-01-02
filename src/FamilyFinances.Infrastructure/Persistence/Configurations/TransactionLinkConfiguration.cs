using FamilyFinances.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class TransactionLinkConfiguration : IEntityTypeConfiguration<TransactionLink>
{
    public void Configure(EntityTypeBuilder<TransactionLink> builder)
    {
        builder.ToTable("TransactionLinks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.SourceTransactionId)
            .HasConversion(
                id => id.Value,
                value => new TransactionId(value))
            .IsRequired();

        builder.Property(x => x.TargetTransactionId)
            .HasConversion(
                id => id.Value,
                value => new TransactionId(value))
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.LinkedOn)
            .IsRequired();

        // Prevent duplicates at DB level (matches our Domain helper)
        builder.HasIndex(x => new { x.SourceTransactionId, x.TargetTransactionId, x.Type })
            .IsUnique();

        builder.HasIndex(x => x.SourceTransactionId);
        builder.HasIndex(x => x.TargetTransactionId);
    }
}
