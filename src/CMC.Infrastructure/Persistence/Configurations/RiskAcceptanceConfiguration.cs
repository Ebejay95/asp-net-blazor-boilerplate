using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class RiskAcceptanceConfiguration : IEntityTypeConfiguration<RiskAcceptance>
{
    public void Configure(EntityTypeBuilder<RiskAcceptance> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.CustomerId).IsRequired();
        e.Property(x => x.ControlId).IsRequired();
        e.Property(x => x.Reason).IsRequired().HasMaxLength(1024);
        e.Property(x => x.RiskAcceptedBy).IsRequired().HasMaxLength(128);
        e.Property(x => x.CreatedAt).IsRequired();
        e.Property(x => x.UpdatedAt).IsRequired();
        e.Property(x => x.IsDeleted).HasDefaultValue(false);
        e.Property(x => x.DeletedBy).HasMaxLength(320);

        e.HasIndex(x => new { x.CustomerId, x.ControlId });
    }
}
