using FamilyFinances.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyFinances.Infrastructure.Persistence.Configurations;

public sealed class FiscalYearClosureConfiguration : IEntityTypeConfiguration<FiscalYearClosure>
{
    public void Configure(EntityTypeBuilder<FiscalYearClosure> b)
    {
        b.ToTable("FiscalYearClosures");

        b.HasKey(x => x.Year);

        b.Property(x => x.Year)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(x => x.IsClosed)
            .IsRequired();

        b.Property(x => x.ClosedAtUtc);

        b.Property(x => x.ClosedByUserId)
            .HasMaxLength(450);

        b.Property(x => x.ReopenedAtUtc);

        b.Property(x => x.ReopenedByUserId)
            .HasMaxLength(450);

        b.HasIndex(x => new { x.IsClosed, x.Year });
    }
}
