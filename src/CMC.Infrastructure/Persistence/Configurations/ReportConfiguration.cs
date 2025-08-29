using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.CustomerId);
        e.Property(x => x.DefinitionId).IsRequired();
        e.Property(x => x.Frozen).IsRequired();
        e.Property(x => x.CreatedAt).IsRequired();
        e.Property(x => x.UpdatedAt).IsRequired();
        e.Property(x => x.IsDeleted).HasDefaultValue(false);
        e.Property(x => x.DeletedBy).HasMaxLength(320);
    }
}
