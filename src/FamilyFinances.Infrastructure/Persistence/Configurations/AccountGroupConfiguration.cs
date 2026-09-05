using FamilyFinances.Domain.Ledger.AccountGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class AccountGroupConfiguration : IEntityTypeConfiguration<AccountGroup>
{
    public void Configure(EntityTypeBuilder<AccountGroup> b)
    {
        b.ToTable("AccountGroups");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new AccountGroupId(value));

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        b.HasIndex(x => x.NormalizedName)
            .IsUnique();

        b.Property(x => x.Description)
            .HasMaxLength(1000);

        b.Property(x => x.IsDashboardPinned)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
