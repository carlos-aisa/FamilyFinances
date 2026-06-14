using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class AccountKindCatalogConfiguration : IEntityTypeConfiguration<AccountKindCatalog>
{
    public void Configure(EntityTypeBuilder<AccountKindCatalog> builder)
    {
        builder.ToTable("AccountKinds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new AccountKindCatalogId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Key)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.Nature)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.LegacyKind)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => new { x.IsActive, x.SortOrder, x.Name });
    }
}