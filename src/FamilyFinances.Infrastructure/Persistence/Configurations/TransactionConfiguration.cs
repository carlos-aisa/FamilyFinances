using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new TransactionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.BookedOn)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')");

        // Splits are stored in a separate table with a shadow FK "TransactionId"
        builder.HasMany(x => x.Splits)
            .WithOne()
            .HasForeignKey("TransactionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Splits).AutoInclude(false);

        builder.Property(t => t.PayeeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new PayeeId(value.Value) : (PayeeId?)null);

        builder.HasOne(x=> x.Payee)
            .WithMany()
            .HasForeignKey(t => t.PayeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
