using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class AccountGroupMemberConfiguration : IEntityTypeConfiguration<AccountGroupMember>
{
    public void Configure(EntityTypeBuilder<AccountGroupMember> b)
    {
        b.ToTable("AccountGroupMembers");

        // Composite key prevents duplicates
        b.HasKey(x => new { x.GroupId, x.AccountId });

        b.Property(x => x.GroupId)
            .HasConversion(
                id => id.Value,
                value => new AccountGroupId(value));

        b.Property(x => x.AccountId)
            .HasConversion(
                id => id.Value,
                value => new AccountId(value));

        // FK to AccountGroups
        b.HasOne<AccountGroup>()
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Accounts
        b.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
